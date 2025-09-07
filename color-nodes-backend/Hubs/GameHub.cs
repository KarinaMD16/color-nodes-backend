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

    }
}
