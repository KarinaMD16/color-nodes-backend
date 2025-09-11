using Microsoft.AspNetCore.SignalR;

namespace color_nodes_backend.Hubs
{
    public class GameHub : Hub<IGameClient>
    {
        public async Task JoinRoom(string roomCode, string username)
        {
            var group = $"room:{roomCode}";
            await Groups.AddToGroupAsync(Context.ConnectionId, group);
            await Clients.Group(group).PlayerJoined(username);
        }

        public async Task LeaveRoom(string roomCode, string username)
        {
            var group = $"room:{roomCode}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, group);
            await Clients.Group(group).PlayerLeft(username);
        }

        public Task JoinGame(string gameId)
            => Groups.AddToGroupAsync(Context.ConnectionId, $"game:{gameId}");
    }

}
