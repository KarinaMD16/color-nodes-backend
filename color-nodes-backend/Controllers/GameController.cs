using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using color_nodes_backend.Contracts;
using color_nodes_backend.Entities;
using color_nodes_backend.Services;
using color_nodes_backend.Hubs;

namespace color_nodes_backend.Controllers
{
    [ApiController]
    [Route("api/game")]
    public class GameController : ControllerBase
    {
        private readonly IGameService _games;
        private readonly IHubContext<GameHub> _hub;

        public GameController(IGameService games, IHubContext<GameHub> hub)
        {
            _games = games;
            _hub = hub;
        }
        // inicaiar partida
        [HttpPost("start")] 
        public ActionResult<GameStateResponse> Start([FromBody] StartGameRequest req)
        {
            var g = _games.StartGameForRoom(req.RoomCode);
            BroadcastState(g, hitMessage: null, turnChanged: true);
            return Ok(ToResponse(g));
        }

        // primero - colocar vasos
        [HttpPost("{id:guid}/place-initial")]
        public async Task<ActionResult<GameStateResponse>> PlaceInitial(Guid id, [FromBody] PlaceInitialCupsRequest req)
        {
            var res = _games.PlaceInitialCups(id, req.PlayerId, req.Cups);
            await BroadcastStateAsync(res.Game, res.HitMessage, res.TurnChanged);
            return Ok(ToResponse(res.Game));
        }

        // juego
        [HttpPost("{id:guid}/swap")]
        public async Task<ActionResult<GameStateResponse>> Swap(Guid id, [FromBody] SwapRequest req)
        {
            var res = _games.ApplySwap(id, req.PlayerId, req.FromIndex, req.ToIndex);
            await BroadcastStateAsync(res.Game, res.HitMessage, res.TurnChanged);
            return Ok(ToResponse(res.Game));
        }

        // timeout 
        [HttpPost("{id:guid}/tick")]
        public async Task<ActionResult<GameStateResponse>> Tick(Guid id)
        {
            var before = _games.GetState(id);
            var g = _games.EnsureTurnFresh(id);

            var turnChanged = (g.CurrentPlayerId != before.CurrentPlayerId);
            if (turnChanged)
            {
                await BroadcastStateAsync(g, hitMessage: null, turnChanged: true);
            }
            return Ok(ToResponse(g));
        }

        // get partida
        [HttpGet("{id:guid}")]
        public ActionResult<GameStateResponse> Get(Guid id)
        {
            var g = _games.GetState(id);
            return Ok(ToResponse(g));
        }

        // ?
        private GameStateResponse ToResponse(Game g) =>
        new(
            g.Id,
            g.RoomCode,
            g.Status.ToString(),
            g.Cups,
            g.LastHits,
            g.TotalMoves,
            g.CurrentPlayerId,
            g.PlayerOrder,
            g.TurnEndsAtUtc,
            g.Status == GameStatus.Finished ? g.TargetPattern : null,
            AvailableColors: _games.GetPalette()
        );

        // signal R / hub notifs
        private void BroadcastState(Game g, string? hitMessage, bool turnChanged)
        {
            var group = $"room:{g.RoomCode}";
            _hub.Clients.Group(group).SendAsync("stateUpdated", ToResponse(g));

            if (!string.IsNullOrWhiteSpace(hitMessage))                 // notificar aciertos 
            { 
                _hub.Clients.Group(group).SendAsync("hitFeedback", new { message = hitMessage });
            }

            if (turnChanged)                                            // notificar cambio de turno
            {
                _hub.Clients.Group(group).SendAsync("turnChanged", 
                    new { currentPlayerId = g.CurrentPlayerId, turnEndsAtUtc = g.TurnEndsAtUtc });
            }
            if (g.Status == GameStatus.Finished)                        // fin de partida
            {
                _hub.Clients.Group(group).SendAsync("finished", 
                    new { gameId = g.Id, totalMoves = g.TotalMoves });
            }
        }

        private Task BroadcastStateAsync(Game g, string? hitMessage, bool turnChanged)
        {
            BroadcastState(g, hitMessage, turnChanged);
            return Task.CompletedTask;
        }
    }
}
