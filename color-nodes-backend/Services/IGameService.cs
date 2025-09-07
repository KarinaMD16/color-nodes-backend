using color_nodes_backend.Contracts;
using color_nodes_backend.Entities;

namespace color_nodes_backend.Services
{
    public interface IGameService
    {
        // crear la partida para una sala existente 
        Game StartGameForRoom(string roomCode);
        GameResult PlaceInitialCups(Guid gameId, int playerId, List<string> cups);
        GameResult ApplySwap(Guid gameId, int playerId, int fromIndex, int toIndex);

        // forzar cambio de turno 
        Game EnsureTurnFresh(Guid gameId);
        Game GetState(Guid gameId);
    }
}