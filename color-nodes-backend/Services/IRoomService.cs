using color_nodes_backend.DTOs;

namespace color_nodes_backend.Services
{
    public interface IRoomService
    {
        Task<RoomResponse> CreateRoomAsync(string username);
        Task<RoomResponse> JoinRoomAsync(string username, string roomCode);
        Task<string> LeaveRoomAsync(int userId, string roomCode);
        Task<List<RoomResponse>> GetActiveRoomsAsync();
        Task<ServiceResult<RoomResponse>> GetRoomByCodeAsync(string roomCode);
        Task<List<UserRankDto>> GetLeaderboardAsync(string roomCode);


    }
}
