using color_nodes_backend.Data;
using color_nodes_backend.DTOs;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace color_nodes_backend.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserDto> CreateUserAsync(CreateUserDto dto)
        {
            var user = await _context.Users
                .AsTracking()
                .FirstOrDefaultAsync(u => u.Username == dto.Username);

            if (user != null)
            {
                return new UserDto
                {
                    Id = user.Id,
                    Username = user.Username ?? "",
                    Score = user.Score,
                    RoomId = user.RoomId
                };
            }

            user = new Entities.User
            {
                Username = dto.Username,
                Score = 0
            };

            _context.Users.Add(user);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (
                ex.InnerException is SqliteException sqliteEx &&
                sqliteEx.SqliteErrorCode == 19 // UNIQUE constraint failed
            )
            {
                user = await _context.Users.FirstAsync(u => u.Username == dto.Username);
            }

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username ?? "",
                Score = user.Score,
                RoomId = user.RoomId
            };
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return null;

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username ?? "",
                Score = user.Score,
                RoomId = user.RoomId
            };
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            return await _context.Users
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username ?? "",
                    Score = u.Score,
                    RoomId = u.RoomId
                })
                .ToListAsync();
        }

        public async Task<UserDto?> UpdateUserAsync(int id, UpdateUserDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return null;

            user.Username = dto.Username;
            user.Score = dto.Score;

            await _context.SaveChangesAsync();

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username ?? "",
                Score = user.Score,
                RoomId = user.RoomId
            };
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return true;
        }
        public async Task<IEnumerable<UserDto>> GetUsersOrderedByScoreAsync()
        {
            return await _context.Users
                .OrderByDescending(u => u.Score) 
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username ?? "",
                    Score = u.Score,
                    RoomId = u.RoomId
                })
                .ToListAsync();
        }

    }
}
