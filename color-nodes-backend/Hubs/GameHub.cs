using Microsoft.AspNetCore.SignalR;

namespace color_nodes_backend.Hubs
{
    public class GameHub : Hub
    {
        // Unirse a sala
        public async Task JoinRoom(string roomCode, string username)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);
            await Clients.Group(roomCode).SendAsync("PlayerJoined", username);
        }

        // Salir de sala
        public async Task LeaveRoom(string roomCode, string username)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomCode);
            await Clients.Group(roomCode).SendAsync("PlayerLeft", username);
        }
    }
}
