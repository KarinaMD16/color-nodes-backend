using Microsoft.AspNetCore.SignalR;

namespace color_nodes_backend.Hubs
{
    public class GameHub : Hub
    {
      
        public async Task JoinRoom(string roomCode, string username)
        {
            var group = $"room:{roomCode}";
            await Groups.AddToGroupAsync(Context.ConnectionId, group);
            await Clients.Group(group).SendAsync("PlayerJoined", username);
        }

        public async Task LeaveRoom(string roomCode, string username)
        {
            var group = $"room:{roomCode}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, group);
            await Clients.Group(group).SendAsync("PlayerLeft", username);
        }

        // métodos para el juego
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
        }
    }
}