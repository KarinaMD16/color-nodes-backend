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

            //  Mensaje de sistema cuando alguien se une
            await Clients.Group(group).ChatMessage(new
            {
                id = Guid.NewGuid().ToString(),
                username = "Sistema",
                message = $"El jugador {username} se ha unido a la sala",
                timestamp = DateTime.UtcNow,
                isSystem = true
            });
        }

        public async Task LeaveRoom(string roomCode, string username)
        {
            var group = $"room:{roomCode}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, group);
            await Clients.Group(group).PlayerLeft(username);

            // Mensaje de sistema cuando alguien se va
            await Clients.Group(group).ChatMessage(new
            {
                id = Guid.NewGuid().ToString(),
                username = "Sistema",
                message = $"El jugador {username} ha salido de la sala",
                timestamp = DateTime.UtcNow,
                isSystem = true
            });
        }

        public Task JoinGame(string gameId)
            => Groups.AddToGroupAsync(Context.ConnectionId, $"game:{gameId}");

        public async Task SendChatMessage(string roomCode, string username, string message)
        {
            var group = $"room:{roomCode}";

            // Valida que el mensaje no esté vacío
            if (string.IsNullOrWhiteSpace(message)) return;

            // Limita la longitud del mensaje
            var trimmedMessage = message.Length > 50 ? message.Substring(0, 50) : message;

            await Clients.Group(group).ChatMessage(new
            {
                id = Guid.NewGuid().ToString(),
                username,
                message = trimmedMessage,
                timestamp = DateTime.UtcNow,
                isSystem = false
            });
        }
    }
}