using System.Text.Json.Serialization;

namespace color_nodes_backend.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public double Score { get; set; }

        // relación con las salas, un user puede estar o no en una sala
        public int? RoomId { get; set; }
        [JsonIgnore]
        public Room? Room { get; set; }
    }
}
