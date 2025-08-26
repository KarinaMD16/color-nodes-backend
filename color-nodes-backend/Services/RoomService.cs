using color_nodes_backend.Data;
using color_nodes_backend.DTOs;
using color_nodes_backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace color_nodes_backend.Services
{
    public class RoomService : IRoomService
    {
        private readonly AppDbContext _context;
        private readonly Random _random = new();

        public RoomService(AppDbContext context)
        {
            _context = context;
        }

        // ✅ Crear sala y líder
        public async Task<RoomResponse> CreateRoomAsync(string username)
        {
            // Verificar que usuario no esté ya en otra sala
            var existingUser = await _context.Users
                .Include(u => u.Room)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (existingUser != null && existingUser.RoomId != null)
                throw new InvalidOperationException($"El usuario {username} ya está en una sala.");

            var user = existingUser ?? new User { Username = username };
            if (existingUser == null) _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var room = new Room
            {
                Code = Guid.NewGuid().ToString("N")[..6].ToUpper(),
                LeaderId = user.Id,
                Users = new List<User> { user }
            };

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            return new RoomResponse
            {
                Code = room.Code,
                LeaderId = room.LeaderId,
                Users = room.Users.Select(u => u.Username).ToList()
            };
        }

        // ✅ Unirse a sala
        public async Task<RoomResponse> JoinRoomAsync(string username, string roomCode)
        {
            var room = await _context.Rooms
                .Include(r => r.Users)
                .FirstOrDefaultAsync(r => r.Code == roomCode);

            if (room == null) throw new KeyNotFoundException("Sala no encontrada.");

            // Verificar que usuario no esté en otra sala
            var existingUser = await _context.Users
                .Include(u => u.Room)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (existingUser != null && existingUser.RoomId != null && existingUser.RoomId != room.Id)
                throw new InvalidOperationException($"El usuario {username} ya está en otra sala.");

            var user = existingUser ?? new User { Username = username };
            if (existingUser == null) _context.Users.Add(user);

            room.Users.Add(user);
            await _context.SaveChangesAsync();

            return new RoomResponse
            {
                Code = room.Code,
                LeaderId = room.LeaderId,
                Users = room.Users.Select(u => u.Username).ToList()
            };
        }

        // ✅ Salir de sala con userId
        public async Task<string> LeaveRoomAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Room)
                .ThenInclude(r => r.Users)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null || user.Room == null)
                throw new InvalidOperationException("El usuario no está en ninguna sala.");

            var room = user.Room;
            room.Users.Remove(user);
            user.RoomId = null;

            // Si el líder sale, pasar liderazgo a otro random
            if (room.LeaderId == user.Id && room.Users.Any())
            {
                var newLeader = room.Users[_random.Next(room.Users.Count)];
                room.LeaderId = newLeader.Id;
            }

            // Si la sala queda vacía, eliminarla
            if (!room.Users.Any())
            {
                _context.Rooms.Remove(room);
            }

            await _context.SaveChangesAsync();

            return user.Username; // ✅ devolvemos el username para mostrar en hub
        }

        // ✅ Obtener todas las salas activas
        public async Task<List<RoomResponse>> GetActiveRoomsAsync()
        {
            return await _context.Rooms
                .Include(r => r.Users)
                .Select(r => new RoomResponse
                {
                    Code = r.Code,
                    LeaderId = r.LeaderId,
                    Users = r.Users.Select(u => u.Username).ToList()
                }).ToListAsync();
        }

        // ✅ Obtener sala por código
        public async Task<RoomResponse?> GetRoomByCodeAsync(string roomCode)
        {
            var room = await _context.Rooms
                .Include(r => r.Users)
                .FirstOrDefaultAsync(r => r.Code == roomCode);

            if (room == null) return null;

            return new RoomResponse
            {
                Code = room.Code,
                LeaderId = room.LeaderId,
                Users = room.Users.Select(u => u.Username).ToList()
            };
        }
    }
}
