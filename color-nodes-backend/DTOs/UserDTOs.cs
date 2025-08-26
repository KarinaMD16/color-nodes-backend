namespace color_nodes_backend.DTOs
{
    public class CreateUserDto
    {
        public string Username { get; set; } = string.Empty;
    }

    public class UpdateUserDto
    {
        public string? Username { get; set; }
        public double Score { get; set; }
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public double Score { get; set; }
        public int? RoomId { get; set; }
    }
}
