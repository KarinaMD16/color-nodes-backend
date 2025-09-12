using color_nodes_backend.Contracts;

namespace color_nodes_backend.Hubs
{
    public interface IGameClient
    {
        Task StateUpdated(GameStateResponse state);
        Task TurnChanged(int currentPlayerId);
        Task HitFeedback(string message);
        Task Finished(GameStateResponse finalState);

        Task PlayerJoined(string username);
        Task PlayerLeft(string username);
    }
}
