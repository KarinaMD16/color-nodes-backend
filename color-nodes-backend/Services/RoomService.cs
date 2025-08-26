using color_nodes_backend.Data;
using color_nodes_backend.DTOs;
using color_nodes_backend.Entities;
using Microsoft.EntityFrameworkCore;

public class RoomService : IRoomService
{
    private readonly AppDbContext _context;
    private readonly Random _random = new();

    public RoomService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<RoomDto> CreateRoomAsync(int leaderId)
    {
        var leader = await _context.Users.FindAsync(leaderId);
        if (leader == null)
            throw new KeyNotFoundException("Usuario líder no encontrado.");

        var room = new Room
        {
            LeaderId = leaderId,
            Code = GenerateRoomCode(),
            isActive = true,
            Users = new List<User> { leader }
        };
        leader.Room = room;

        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();

        return MapToDto(room);
    }

    public async Task<RoomDto?> JoinRoomAsync(string code, int userId)
    {
        var room = await _context.Rooms.Include(r => r.Users)
            .FirstOrDefaultAsync(r => r.Code == code && r.isActive);

        if (room == null) throw new KeyNotFoundException("Sala no encontrada o inactiva.");

        var user = await _context.Users.FindAsync(userId);
        if (user == null) throw new KeyNotFoundException("Usuario no encontrado.");

        if (room.Users.Any(u => u.Id == userId))
            throw new InvalidOperationException("El usuario ya está en esta sala.");

        if (room.Users.Count >= 4)
            throw new InvalidOperationException("La sala ya está llena (máximo 4 jugadores).");

        room.Users.Add(user);
        user.RoomId = room.Id;

        await _context.SaveChangesAsync();
        return MapToDto(room);
    }

    public async Task LeaveRoomAsync(int roomId, int userId)
    {
        var room = await _context.Rooms.Include(r => r.Users)
            .FirstOrDefaultAsync(r => r.Id == roomId);
        if (room == null) throw new KeyNotFoundException("Sala no encontrada.");

        var user = room.Users.FirstOrDefault(u => u.Id == userId);
        if (user == null) throw new InvalidOperationException("El usuario no pertenece a esta sala.");

        room.Users.Remove(user);
        user.RoomId = null;

        if (room.LeaderId == userId)
        {
            if (room.Users.Any())
            {
                var randomIndex = _random.Next(room.Users.Count);
                room.LeaderId = room.Users[randomIndex].Id;
            }
            else
            {
                room.isActive = false;
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task<RoomDto?> GetRoomByCodeAsync(string code)
    {
        var room = await _context.Rooms.Include(r => r.Users)
            .FirstOrDefaultAsync(r => r.Code == code && r.isActive);
        return room == null ? null : MapToDto(room);
    }

    public async Task<RoomDto?> GetRoomByIdAsync(int roomId)
    {
        var room = await _context.Rooms.Include(r => r.Users)
            .FirstOrDefaultAsync(r => r.Id == roomId && r.isActive);
        return room == null ? null : MapToDto(room);
    }

    public async Task<List<RoomDto>> GetActiveRoomsAsync()
    {
        var rooms = await _context.Rooms.Include(r => r.Users)
            .Where(r => r.isActive)
            .ToListAsync();
        return rooms.Select(MapToDto).ToList();
    }

    // mapeo Room -> RoomDto
    private RoomDto MapToDto(Room room)
    {
        return new RoomDto
        {
            Id = room.Id,
            Code = room.Code,
            LeaderId = room.LeaderId,
            UserIds = room.Users.Select(u => u.Id).ToList()
        };
    }

    private string GenerateRoomCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, 6)
            .Select(s => s[_random.Next(s.Length)]).ToArray());
    }
}
