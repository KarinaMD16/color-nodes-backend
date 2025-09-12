using color_nodes_backend.Contracts;
using color_nodes_backend.Entities;

namespace color_nodes_backend.Services
{
    public interface IGameService
    {
        Task<Game> StartGameForRoom(string roomCode, CancellationToken ct = default); // crear partida
        Task<GameResult> PlaceInitialCups(Guid gameId, int playerId, List<string> cups, CancellationToken ct = default); // fase inicial
        Task<GameResult> ApplySwap(Guid gameId, int playerId, int fromIndex, int toIndex, CancellationToken ct = default); // juego 
        Task<Game> EnsureTurnFresh(Guid gameId, CancellationToken ct = default);
        Task<Game> GetState(Guid gameId, CancellationToken ct = default);
        IReadOnlyList<string> GetPalette();
    }
}