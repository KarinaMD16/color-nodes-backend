using System.Text.Json.Serialization;

namespace color_nodes_backend.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public double Score { get; set; }

        public double Room_Score { get; set; } = 0;

        // relación con las salas, un user puede estar o no en una sala
        public int? RoomId { get; set; }
        [JsonIgnore]
        public Room? Room { get; set; }
    }
}
