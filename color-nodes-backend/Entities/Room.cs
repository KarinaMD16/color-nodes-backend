namespace color_nodes_backend.Entities
{
    public class Room
    {
        public int Id { get; set; }
        public string? Code { get; set; }
        public int LeaderId { get; set; }
        public bool isActive { get; set; } = true;


        // lista de users en la sala
        public List<User> Users { get; set; } = new List<User>();
    }
}
