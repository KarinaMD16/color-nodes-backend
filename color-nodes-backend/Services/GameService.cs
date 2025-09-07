using Microsoft.EntityFrameworkCore;
using color_nodes_backend.Data;
using color_nodes_backend.Entities;
using color_nodes_backend.Contracts;

namespace color_nodes_backend.Services
{
    public class GameService : IGameService
    {
        private readonly AppDbContext _db;
        private readonly Random _rnd = new();

        public GameService(AppDbContext db) { _db = db; }

        public Game StartGameForRoom(string roomCode)
        {
            if (string.IsNullOrWhiteSpace(roomCode))
                throw new ArgumentException("roomCode requerido", nameof(roomCode));

            var room = _db.Rooms
                .Include(r => r.Users)
                .FirstOrDefault(r => r.Code == roomCode)
                ?? throw new InvalidOperationException("Sala no encontrada");

            //  random p turn
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
                .OrderBy(_ => _rnd.Next()) // random target colrs
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
                MaxMovesPerTurn = 2,
                TurnDurationSeconds = 0,
                TurnEndsAtUtc = DateTime.MaxValue,
                LastHits = 0,
                TotalMoves = 0,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            };

            _db.Games.Add(game);
            _db.SaveChanges();
            return game;
        }

        public GameResult PlaceInitialCups(Guid gameId, int playerId, List<string> cups)
        {
            var g = _db.Games.FirstOrDefault(x => x.Id == gameId)
                ?? throw new KeyNotFoundException("Partida no encontrada.");

            var beforeHits = g.LastHits;
            var beforePlayer = g.CurrentPlayerId;

            EnsureTurnFreshInTx(g); // timer !!!!!

            if (g.Status != GameStatus.Setup)
            {
                throw new InvalidOperationException("La partida no está en estado de Setup. Ya no puedes hacer eso. ");
            }

            if (playerId != (g.CurrentPlayerId ?? -1))
            {
                throw new UnauthorizedAccessException("¡No es tu turno! Espera a que el jugador actual termine su turno o se le acabe el tiempo. ");
            }

            if (cups is null || cups.Count != 6)
            {
                throw new ArgumentException("Debes colocar exactamente 6 vasos para poder continuar.");
            }

            g.Cups = new List<string>(cups);
            g.LastHits = CountHits(g.Cups, g.TargetPattern); // calc aciertos 

            // avanzar
            AdvanceTurn(g, consumeWholeTurn: true);
            g.Status = GameStatus.InProgress;
            g.UpdatedAtUtc = DateTime.UtcNow;

            _db.SaveChanges();

            string hitMsg = g.LastHits == 1
                                        ? "1 acierto"
                                        : $"{g.LastHits} aciertos";
            // ? 
            bool turnChanged = g.Status == GameStatus.Finished || g.MovesThisTurn == 0;

            return new GameResult(g, hitMsg, turnChanged);
        }

        public GameResult ApplySwap(Guid gameId, int playerId, int fromIndex, int toIndex)
        {
            var g = _db.Games.FirstOrDefault(x => x.Id == gameId)
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
                _db.SaveChanges();
                // hit actual
                var hitMsgTimeout = g.LastHits == 1 ? "1 acierto" : $"{g.LastHits} aciertos";
                return new GameResult(g, hitMsgTimeout, TurnChanged: g.CurrentPlayerId != beforePlayer);
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
            g.Cups = cups;                              // reasignar nueva instancia

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

            _db.SaveChanges();

            //  estado de aciertos actual
            string hitMsg = g.LastHits == 1 ? "1 acierto" : $"{g.LastHits} aciertos";
            bool turnChanged = g.CurrentPlayerId != beforePlayer || g.Status == GameStatus.Finished;

            return new GameResult(g, hitMsg, turnChanged);
        }



        public Game EnsureTurnFresh(Guid gameId)
        {
            var g = _db.Games.FirstOrDefault(x => x.Id == gameId)
                ?? throw new KeyNotFoundException("Partida no encontrada");

            EnsureTurnFreshInTx(g);
            _db.SaveChanges();
            return g;
        }

        public Game GetState(Guid gameId)
        {
            var g = _db.Games.AsNoTracking().FirstOrDefault(x => x.Id == gameId)
                ?? throw new KeyNotFoundException("Partida no encontrada");

            return g;
        }






        // ---------- Helpers ----------
        private static readonly List<string> PredefinedPalette = new()
        {
            "#F8FFE5",
            "#06D6A0",
            "#1B9AAA",
            "#85718D",
            "#EF476F",
            "#FFC43D"
        };

        private static int CountHits(IReadOnlyList<string> cups, IReadOnlyList<string> target) // aciertos
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
                g.TurnEndsAtUtc = DateTime.MaxValue; // sin límite (modo pruebas)

            g.UpdatedAtUtc = DateTime.UtcNow;
        }

        private static int NextIndex(Game g)
        {
            if (g.PlayerOrder.Count == 0) return 0;
            return (g.CurrentPlayerIndex + 1) % g.PlayerOrder.Count;
        }
    }
}