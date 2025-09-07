using System.ComponentModel.DataAnnotations;

namespace color_nodes_backend.Entities
{
    public class GameMove
    {
        [Key]
        public long Id { get; set; }

        public Guid GameId { get; set; }
        public int PlayerId { get; set; }

        public int FromIndex { get; set; }
        public int ToIndex { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
