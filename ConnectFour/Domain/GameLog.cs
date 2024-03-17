using ConnectFour.Models;

namespace ConnectFour.Domain;

public class GameLog
{
    public required GameId GameId { get; init; }
    public required PlayerConnection FirstPlayerConnection { get; init; }
    public required PlayerConnection SecondPlayerConnection { get; init; }
    public PlayerId GetCurrentPlayerId => Log.Count % 2 is 0 ? FirstPlayerConnection.PlayerId : SecondPlayerConnection.PlayerId;
    public bool IsComplete { get; private set; }
    public List<int> Log { get; } = new();
    public PlayerColor CurrentPlayerColor => Log.Count % 2 is 0 ? PlayerColor.Red : PlayerColor.Yellow;
    public PlayerColor PreviousPlayerColor => Log.Count % 2 is 0 ? PlayerColor.Yellow : PlayerColor.Red;
    public void AddMove(int column) => Log.Add(column);
    public void Complete() => IsComplete = true;

    public void Deconstruct(out GameId gameId, out PlayerId firstPlayerId, out PlayerId secondPlayerId)
    {
        gameId = GameId;
        firstPlayerId = FirstPlayerConnection.PlayerId;
        secondPlayerId = SecondPlayerConnection.PlayerId;
    }

    public bool IsPlayerInTheGame(PlayerId playerId) =>
        FirstPlayerConnection.PlayerId == playerId || SecondPlayerConnection.PlayerId == playerId;
}