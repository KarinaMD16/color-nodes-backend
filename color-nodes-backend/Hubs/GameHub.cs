using color_nodes_backend.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace color_nodes_backend.Hubs
{
    public class GameHub : Hub<IGameClient>
    {
        private readonly AppDbContext _db;
        public GameHub(AppDbContext db) { _db = db; }

        public Task SubscribeRoom(string roomCode)
           => Groups.AddToGroupAsync(Context.ConnectionId, $"room:{roomCode}");

        // 🔹 Suscripción silenciosa al grupo de juego (mejor con Guid, pero string también sirve)
        public Task SubscribeGame(string gameId)
            => Groups.AddToGroupAsync(Context.ConnectionId, $"game:{gameId}");

        // 🔹 Desuscripción silenciosa de juego
        public Task UnsubscribeGame(string gameId)
            => Groups.RemoveFromGroupAsync(Context.ConnectionId, $"game:{gameId}");
        public async Task RequestRoomReset(string roomCode, string username)
        {
            // 1) Validar sala y que 'username' sea el líder
            var room = await _db.Rooms.AsNoTracking()
                .FirstOrDefaultAsync(r => r.Code == roomCode);

            if (room is null) throw new HubException("Sala no encontrada.");

            var leader = await _db.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == room.LeaderId);

            if (!string.Equals(leader?.Username, username, StringComparison.OrdinalIgnoreCase))
                throw new HubException("Sólo el host puede reiniciar la sala.");

            // 2) Notificar a todos en la sala que se re-suscriban al grupo y vuelvan a /room/$code
            var group = $"room:{roomCode}";
            await Clients.Group(group).ForceRejoin(roomCode);

            // 3) Mensaje de sistema (opcional)
            await Clients.Group(group).ChatMessage(new
            {
                id = Guid.NewGuid().ToString(),
                username = "Sistema",
                message = "El host reinició la sala. Re-suscribiéndose...",
                timestamp = DateTime.UtcNow,
                isSystem = true
            });
        }

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