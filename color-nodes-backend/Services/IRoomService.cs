using color_nodes_backend.DTOs;

namespace color_nodes_backend.Services
{
    public interface IRoomService
    {
        Task<RoomResponse> CreateRoomAsync(string username);
        Task<RoomResponse> JoinRoomAsync(string username, string roomCode);
        Task<string> LeaveRoomAsync(int userId);
        Task<List<RoomResponse>> GetActiveRoomsAsync();
        Task<RoomResponse?> GetRoomByCodeAsync(string roomCode);
    }
}
