namespace color_nodes_backend.DTOs
{
    public class CreateRoomDto
    {
        public string Username { get; set; } = string.Empty;
    }

    public class JoinRoomDto
    {
        public string RoomCode { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
    }

    public class LeaveRoomDto
    {
        public int UserId { get; set; } 
    }

    public class RoomResponse
    {
        public string Code { get; set; } = null!;
        public int LeaderId { get; set; }
        public List<string> Users { get; set; } = new();
    }
}
