namespace CodingChallenge.Api.Services;

public sealed class BoardInUseException : InvalidOperationException
{
    public BoardInUseException(int boardId)
        : base($"Board {boardId} cannot be deleted because it is referenced by an active order.")
    {
        BoardId = boardId;
    }

    public int BoardId { get; }
}
