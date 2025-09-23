using Microsoft.AspNetCore.Mvc;
using color_nodes_backend.Contracts;
using color_nodes_backend.Entities;
using color_nodes_backend.Services;

namespace color_nodes_backend.Controllers
{
    [ApiController]
    [Route("api/game")]
    public class GameController : ControllerBase
    {
        private readonly IGameService _games;

        public GameController(IGameService games)
        {
            _games = games;
        }

        [HttpPost("start")]
        public async Task<ActionResult<GameStateResponse>> Start(
            [FromBody] StartGameRequest req,
            CancellationToken ct)
        {
            var g = await _games.StartGameForRoom(req.RoomCode, ct);
            return Ok(ToResponse(g));
        }

        // fase inicial: colocar los 6 vasos
        [HttpPost("{id:guid}/place-initial")]
        public async Task<ActionResult<GameStateResponse>> PlaceInitial(
            Guid id,
            [FromBody] PlaceInitialCupsRequest req,
            CancellationToken ct)
        {
            var res = await _games.PlaceInitialCups(id, req.PlayerId, req.Cups, ct);
            return Ok(ToResponse(res.Game));
        }

        // juego
        [HttpPost("{id:guid}/swap")]
        public async Task<ActionResult<GameStateResponse>> Swap(
            Guid id,
            [FromBody] SwapRequest req,
            CancellationToken ct)
        {
            var res = await _games.ApplySwap(id, req.PlayerId, req.FromIndex, req.ToIndex, ct);
            return Ok(ToResponse(res.Game));
        }

        [HttpPost("{id:guid}/tick")]
        public async Task<ActionResult<GameStateResponse>> Tick(
            Guid id,
            CancellationToken ct)
        {
            var g = await _games.EnsureTurnFresh(id, ct);
            return Ok(ToResponse(g));
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<GameStateResponse>> Get(Guid id, CancellationToken ct)
        {
            var g = await _games.GetState(id, ct);
            return Ok(ToResponse(g));
        }
        private GameStateResponse ToResponse(Game g) =>
            new(
                GameId: g.Id,
                RoomCode: g.RoomCode,
                Status: g.Status.ToString(),
                Cups: g.Cups,
                Hits: g.LastHits,
                TotalMoves: g.TotalMoves,
                CurrentPlayerId: g.CurrentPlayerId,
                PlayerOrder: g.PlayerOrder,
                TurnEndsAtUtc: g.TurnEndsAtUtc,
                TargetPattern: g.Status == GameStatus.Finished ? g.TargetPattern : null,
                AvailableColors: _games.GetPalette()
            );
    }
}