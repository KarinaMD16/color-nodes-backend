using color_nodes_backend.Entities;

namespace color_nodes_backend.Contracts
{
    public record StartGameRequest(string RoomCode);
    public record PlaceInitialCupsRequest(int PlayerId, List<string> Cups);       // 6 vasos visibles (game board)
    public record SwapRequest(int PlayerId, int FromIndex, int ToIndex);

    public record GameStateResponse(
       Guid GameId,
       string RoomCode,
       string Status,
       List<string> Cups,
       int Hits,
       int TotalMoves,
       int? CurrentPlayerId,
       List<int> PlayerOrder,
       DateTime TurnEndsAtUtc,
       List<string>? TargetPattern 
   );
    public record GameResult(
        Game Game,
        string? HitMessage,   // null si no cambió el número de aciertos
        bool TurnChanged      // true si cambió el jugador o terminó
    );
}
