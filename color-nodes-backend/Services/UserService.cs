using color_nodes_backend.Data;
using color_nodes_backend.DTOs;
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
            var user = new Entities.User
            {
                Username = dto.Username,
                Score = 0
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

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
    }
}
