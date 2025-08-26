using color_nodes_backend.DTOs;

public interface IRoomService
{
    Task<RoomDto> CreateRoomAsync(int leaderId);
    Task<RoomDto?> JoinRoomAsync(string code, int userId);
    Task LeaveRoomAsync(string code, int userId);
    Task<RoomDto?> GetRoomByCodeAsync(string code);
    Task<RoomDto?> GetRoomByIdAsync(int roomId);
    Task<List<RoomDto>> GetActiveRoomsAsync();
}

