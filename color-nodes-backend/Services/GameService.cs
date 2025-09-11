using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using color_nodes_backend.Contracts;
using color_nodes_backend.Data;
using color_nodes_backend.Entities;
using color_nodes_backend.Hubs;

namespace color_nodes_backend.Services
{
    public class GameService : IGameService
    {
        private readonly AppDbContext _db;
        private readonly IHubContext<GameHub, IGameClient> _hub;
        private readonly Random _rnd = new();

        public GameService(AppDbContext db, IHubContext<GameHub, IGameClient> hub)
        {
            _db = db;
            _hub = hub;
        }
        public async Task<Game> StartGameForRoom(string roomCode, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(roomCode))
                throw new ArgumentException("roomCode requerido", nameof(roomCode));

            var room = await _db.Rooms
                .Include(r => r.Users)
                .FirstOrDefaultAsync(r => r.Code == roomCode, ct)
                ?? throw new InvalidOperationException("Sala no encontrada");

            // orden de turnos aleatorio
            var order = room.Users
                .OrderBy(_ => _rnd.Next())
                .Select(u => u.Id)
                .ToList();

            if (order.Count == 0 && room.LeaderId > 0)
            {
                order.Add(room.LeaderId);
            }

            if (order.Count == 0)
            {
                throw new InvalidOperationException("No hay jugadores para iniciar.");
            }

            var target = PredefinedPalette
                .OrderBy(_ => _rnd.Next())
                .Take(6)
                .ToList();

            var game = new Game
            {
                RoomCode = roomCode,
                Status = GameStatus.Setup,
                TargetPattern = target,
                Cups = new List<string>(),
                PlayerOrder = order,
                CurrentPlayerIndex = 0,
                MovesThisTurn = 0,
                MaxMovesPerTurn = 1,
                TurnDurationSeconds = 0,                 // 0 = sin límite (modo pruebas)
                TurnEndsAtUtc = DateTime.MaxValue,
                LastHits = 0,
                TotalMoves = 0,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            };

            _db.Games.Add(game);
            await _db.SaveChangesAsync(ct);

            // signalR
            var state = ToResponse(game);
            await _hub.Clients.Group($"room:{roomCode}").StateUpdated(state);
            await _hub.Clients.Group($"game:{game.Id}").StateUpdated(state);

            return game;
        }

        public async Task<GameResult> PlaceInitialCups(Guid gameId, int playerId, List<string> cups, CancellationToken ct = default)
        {
            var g = await _db.Games.FirstOrDefaultAsync(x => x.Id == gameId, ct)
                ?? throw new KeyNotFoundException("Partida no encontrada.");

            var beforePlayer = g.CurrentPlayerId;

            EnsureTurnFreshInTx(g); // avance por tiempo si aplica

            if (g.Status != GameStatus.Setup)
                throw new InvalidOperationException("La partida no está en estado de Setup.");

            if (playerId != (g.CurrentPlayerId ?? -1))
                throw new UnauthorizedAccessException("¡No es tu turno!");

            if (cups is null || cups.Count != 6)
                throw new ArgumentException("Debes colocar exactamente 6 vasos.");

            g.Cups = new List<string>(cups);
            g.LastHits = CountHits(g.Cups, g.TargetPattern);

            AdvanceTurn(g, consumeWholeTurn: true);
            g.Status = GameStatus.InProgress;
            g.UpdatedAtUtc = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);

            var hitMsg = g.LastHits == 1 ? "1 acierto" : $"{g.LastHits} aciertos";
            var turnChanged = g.Status == GameStatus.Finished ||
                              g.CurrentPlayerId != beforePlayer ||
                              g.MovesThisTurn == 0;

            // Emitir eventos SignalR
            var state = ToResponse(g);
            var grp = $"game:{g.Id}";
            await _hub.Clients.Group(grp).StateUpdated(state);
            await _hub.Clients.Group(grp).HitFeedback(hitMsg);
            if (turnChanged)
                await _hub.Clients.Group(grp).TurnChanged(g.CurrentPlayerId ?? 0);
            if (state.Status == "Finished")
                await _hub.Clients.Group(grp).Finished(state);

            return new GameResult(g, hitMsg, turnChanged);
        }

        public async Task<GameResult> ApplySwap(Guid gameId, int playerId, int fromIndex, int toIndex, CancellationToken ct = default)
        {
            var g = await _db.Games.FirstOrDefaultAsync(x => x.Id == gameId, ct)
                ?? throw new KeyNotFoundException("Game no encontrado");

            var beforePlayer = g.CurrentPlayerId;

            EnsureTurnFreshInTx(g);

            if (g.Status != GameStatus.InProgress)
                throw new InvalidOperationException("La partida no está en progreso.");

            if (playerId != (g.CurrentPlayerId ?? -1))
                throw new UnauthorizedAccessException("No es tu turno.");

            if (g.TurnDurationSeconds > 0 && DateTime.UtcNow > g.TurnEndsAtUtc)
            {
                AdvanceTurn(g);
                await _db.SaveChangesAsync(ct);

                var dtoTimeout = ToResponse(g);
                var grpTimeout = $"game:{g.Id}";
                var msgTimeout = g.LastHits == 1 ? "1 acierto" : $"{g.LastHits} aciertos";

                await _hub.Clients.Group(grpTimeout).StateUpdated(dtoTimeout);
                await _hub.Clients.Group(grpTimeout).HitFeedback(msgTimeout);
                await _hub.Clients.Group(grpTimeout).TurnChanged(g.CurrentPlayerId ?? 0);
                if (dtoTimeout.Status == "Finished")
                    await _hub.Clients.Group(grpTimeout).Finished(dtoTimeout);

                return new GameResult(g, msgTimeout, TurnChanged: g.CurrentPlayerId != beforePlayer);
            }

            if (g.MovesThisTurn >= g.MaxMovesPerTurn)
                throw new InvalidOperationException("Límite de movimientos alcanzado en este turno.");

            if (!IsValidIndex(fromIndex) || !IsValidIndex(toIndex))
                throw new ArgumentOutOfRangeException("Índices fuera de rango (0..5).");

            if (fromIndex == toIndex)
                throw new ArgumentException("fromIndex y toIndex no pueden ser iguales.");

            // === SWAP ===
            var cups = g.Cups.ToList();
            (cups[fromIndex], cups[toIndex]) = (cups[toIndex], cups[fromIndex]);
            g.Cups = cups; // reasignar nueva instancia para EF

            g.MovesThisTurn++;
            g.TotalMoves++;
            g.LastHits = CountHits(g.Cups, g.TargetPattern);
            g.UpdatedAtUtc = DateTime.UtcNow;

            _db.GameMoves.Add(new GameMove
            {
                GameId = g.Id,
                PlayerId = playerId,
                FromIndex = fromIndex,
                ToIndex = toIndex,
                CreatedAtUtc = DateTime.UtcNow
            });

            if (g.IsFinished)
            {
                g.Status = GameStatus.Finished;
            }
            else if (g.MovesThisTurn >= g.MaxMovesPerTurn)
            {
                AdvanceTurn(g);
            }

            await _db.SaveChangesAsync(ct);

            // Emitir estado de aciertos y cambios
            var hitMsg = g.LastHits == 1 ? "1 acierto" : $"{g.LastHits} aciertos";
            var turnChanged = g.CurrentPlayerId != beforePlayer || g.Status == GameStatus.Finished;

            var dto = ToResponse(g);
            var grp = $"game:{g.Id}";
            await _hub.Clients.Group(grp).StateUpdated(dto);
            await _hub.Clients.Group(grp).HitFeedback(hitMsg);
            if (turnChanged)
                await _hub.Clients.Group(grp).TurnChanged(g.CurrentPlayerId ?? 0);
            if (dto.Status == "Finished")
                await _hub.Clients.Group(grp).Finished(dto);

            return new GameResult(g, hitMsg, turnChanged);
        }

        public async Task<Game> EnsureTurnFresh(Guid gameId, CancellationToken ct = default)
        {
            var g = await _db.Games.FirstOrDefaultAsync(x => x.Id == gameId, ct)
                ?? throw new KeyNotFoundException("Partida no encontrada");

            var before = g.CurrentPlayerId;
            EnsureTurnFreshInTx(g);
            var changed = before != g.CurrentPlayerId;

            await _db.SaveChangesAsync(ct);

            // Si cambió por timeout, avisamos a todos
            if (changed)
            {
                var state = ToResponse(g);
                var grp = $"game:{g.Id}";
                await _hub.Clients.Group(grp).StateUpdated(state);
                await _hub.Clients.Group(grp).TurnChanged(g.CurrentPlayerId ?? 0);
            }

            return g;
        }

        public async Task<Game> GetState(Guid gameId, CancellationToken ct = default)
        {
            var g = await _db.Games.AsNoTracking().FirstOrDefaultAsync(x => x.Id == gameId, ct)
                ?? throw new KeyNotFoundException("Partida no encontrada");

            return g;
        }

        public IReadOnlyList<string> GetPalette() => PredefinedPalette.AsReadOnly();

        // ================== HELPERS ==================

        private static readonly List<string> PredefinedPalette = new()
        {
            "#F8FFE5", "#06D6A0", "#1B9AAA", "#7067CF", "#EF476F", "#FFC43D"
        };

        private static int CountHits(IReadOnlyList<string> cups, IReadOnlyList<string> target)
        {
            var n = Math.Min(cups.Count, target.Count);
            var hits = 0;
            for (int i = 0; i < n; i++)
                if (cups[i] == target[i]) hits++;
            return hits;
        }

        private static bool IsValidIndex(int i) => i >= 0 && i < 6;

        private void EnsureTurnFreshInTx(Game g)
        {
            if (g.Status == GameStatus.Finished) return;

            // Si el timer está desactivado (0), no se avanza por tiempo.
            if (g.TurnDurationSeconds > 0 && DateTime.UtcNow > g.TurnEndsAtUtc)
            {
                AdvanceTurn(g);
            }
        }

        private void AdvanceTurn(Game g, bool consumeWholeTurn = false)
        {
            if (g.Status == GameStatus.Finished) return;

            g.MovesThisTurn = 0;
            g.CurrentPlayerIndex = NextIndex(g);

            // reloj del siguiente turno:
            if (g.TurnDurationSeconds > 0)
                g.TurnEndsAtUtc = DateTime.UtcNow.AddSeconds(g.TurnDurationSeconds);
            else
                g.TurnEndsAtUtc = DateTime.MaxValue;

            g.UpdatedAtUtc = DateTime.UtcNow;
        }

        private static int NextIndex(Game g)
        {
            if (g.PlayerOrder.Count == 0) return 0;
            return (g.CurrentPlayerIndex + 1) % g.PlayerOrder.Count;
        }

        // Mapeo único hacia el contrato del frontend
        private GameStateResponse ToResponse(Game g) => new(
            GameId: g.Id,
            RoomCode: g.RoomCode,
            Status: g.Status.ToString(),
            Cups: g.Cups?.ToList() ?? new(),
            Hits: g.LastHits,
            TotalMoves: g.TotalMoves,
            CurrentPlayerId: g.CurrentPlayerId,
            PlayerOrder: g.PlayerOrder?.ToList() ?? new(),
            TurnEndsAtUtc: g.TurnEndsAtUtc,
            TargetPattern: g.TargetPattern?.ToList(),
            AvailableColors: PredefinedPalette
        );
    }
}
