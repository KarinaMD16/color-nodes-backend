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
        // métodos para el juego
        /*
        public async Task JoinGame(string gameId)
        {
            var group = $"game:{gameId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, group);
            await Clients.Group(group).SendAsync("PlayerJoinedGame", Context.ConnectionId);
        }

        public async Task LeaveGame(string gameId)
        {
            var group = $"game:{gameId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, group);
            await Clients.Group(group).SendAsync("PlayerLeftGame", Context.ConnectionId);
        }*/
    }

}
