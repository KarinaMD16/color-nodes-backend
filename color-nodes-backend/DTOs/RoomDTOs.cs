namespace color_nodes_backend.DTOs
{
    public class CreateRoomDto
    {
       public int LeaderId { get; set; }
    }

    public class RoomDto
    {
        public int Id { get; set; }
        public string? Code { get; set; }
        public int LeaderId { get; set; }
        public List<int> UserIds { get; set; } = new();
    }

    public class JoinRoomDto
    {
        public int UserId { get; set; }
    }
}
