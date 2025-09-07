using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace color_nodes_backend.Entities
{
    public enum GameStatus
    {
        Setup = 0,       // primer player coloca los 6 vasos
        InProgress = 1,
        Finished = 2
    }

    public class Game
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string RoomCode { get; set; } = null!;

        public GameStatus Status { get; set; } = GameStatus.Setup;

        // estados dek board
        [Required]
        public List<string> Cups { get; set; } = new();

        [Required]
        public List<string> TargetPattern { get; set; } = new();

        // turnos 
        [Required]
        public List<int> PlayerOrder { get; set; } = new();

        public int CurrentPlayerIndex { get; set; } = 0;
        public int? CurrentPlayerId => (PlayerOrder.Count > 0 && CurrentPlayerIndex >= 0 && CurrentPlayerIndex < PlayerOrder.Count)
            ? PlayerOrder[CurrentPlayerIndex] : null;

            // control
        public int MovesThisTurn { get; set; } = 0;
        public int MaxMovesPerTurn { get; set; } = 2;
        public int TurnDurationSeconds { get; set; } = 0; 
        public DateTime TurnEndsAtUtc { get; set; } = DateTime.MaxValue;


        // aciertos
        public int LastHits { get; set; } = 0;
        public int TotalMoves { get; set; } = 0;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

        [NotMapped]
        public bool IsFinished => TargetPattern.Count == 6 && LastHits == 6;
    }
}
