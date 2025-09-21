using color_nodes_backend.Data;
using color_nodes_backend.DTOs;
using color_nodes_backend.Entities;
using Microsoft.Data.Sqlite;
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

        public async Task<RoomResponse> CreateRoomAsync(string username)
        {
            User user = await _context.Users
                .AsTracking()
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user != null && user.RoomId != null)
                throw new InvalidOperationException($"El usuario {username} ya está en una sala.");

            if (user == null)
            {
                try
                {
                    user = new User { Username = username };
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync(); 
                }
                catch (DbUpdateException ex) when (
                    ex.InnerException is SqliteException sqliteEx &&
                    sqliteEx.SqliteErrorCode == 19
                )
                {
                    user = await _context.Users.FirstAsync(u => u.Username == username);

                    if (user.RoomId != null)
                        throw new InvalidOperationException($"El usuario {username} ya está en una sala.");
                }
            }

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
                Users = room.Users.ToList()
            };
        }

        public async Task<RoomResponse> JoinRoomAsync(string username, string roomCode)
        {
            var room = await _context.Rooms
                .Include(r => r.Users)
                .FirstOrDefaultAsync(r => r.Code == roomCode);

            if (room == null)
                throw new KeyNotFoundException("Sala no encontrada.");

            // validar que no haya un juego activo en esa sala
            var existingGame = await _context.Games
                .Where(g => g.RoomCode == roomCode && g.Status != GameStatus.Finished)
                .FirstOrDefaultAsync();

            if (existingGame != null)
                throw new InvalidOperationException("No puedes unirte, la sala ya tiene un juego en progreso.");

            var existingUser = await _context.Users
                .Include(u => u.Room)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (existingUser != null && existingUser.RoomId != null)
                throw new InvalidOperationException($"El usuario {username} ya está en otra sala.");

            var user = existingUser ?? new User { Username = username };
            if (existingUser == null) _context.Users.Add(user);

            if (room.Users.Count == 4)
                throw new InvalidOperationException("La sala está llena.");

            room.Users.Add(user);
            await _context.SaveChangesAsync();

            return new RoomResponse
            {
                Code = room.Code,
                LeaderId = room.LeaderId,
                Users = room.Users.ToList()
            };
        }


        public async Task<string> LeaveRoomAsync(int userId, string roomCode)
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

            if (room.LeaderId == user.Id && room.Users.Any())
            {
                var newLeader = room.Users[_random.Next(room.Users.Count)];
                room.LeaderId = newLeader.Id;
            }

            if (!room.Users.Any())
            {
                room.isActive = false;
            }

            await _context.SaveChangesAsync();

            return user.Username;
        }


        public async Task<List<RoomResponse>> GetActiveRoomsAsync()
        {
            return await _context.Rooms
                .Where(r => r.isActive)
                .Include(r => r.Users)
                .Select(r => new RoomResponse
                {
                    Code = r.Code,
                    LeaderId = r.LeaderId,
                    Users = r.Users.ToList()
                }).ToListAsync();
        }

        public async Task<ServiceResult<RoomResponse>> GetRoomByCodeAsync(string roomCode)
        {
            var room = await _context.Rooms
                .Include(r => r.Users)
                .FirstOrDefaultAsync(r => r.Code == roomCode);

            if (room == null)
                return new ServiceResult<RoomResponse>
                {
                    Success = false,
                    Message = "La sala no existe."
                };

            if (!room.isActive)
                return new ServiceResult<RoomResponse>
                {
                    Success = false,
                    Message = "La sala está inactiva."
                };

            return new ServiceResult<RoomResponse>
            {
                Success = true,
                Data = new RoomResponse
                {
                    Code = room.Code,
                    LeaderId = room.LeaderId,
                    Users = room.Users.ToList()
                }
            };
        }


        public async Task<List<UserRankDto>> GetLeaderboardAsync(string roomCode)
        {
            var room = await _context.Rooms
                .Include(r => r.Users)
                .FirstOrDefaultAsync(r => r.Code == roomCode);

            if (room == null)
                throw new KeyNotFoundException("Sala no encontrada.");

            return room.Users
                .OrderByDescending(u => u.Score)
                .Select((u, index) => new UserRankDto
                {
                    Rank = index + 1,
                    Username = u.Username,
                    Score = u.Score,
                })
                .ToList();
        }


    }


}
