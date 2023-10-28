using ConnectFour.Persistence;

namespace ConnectFour.Domain;

public class GameLog
{
    public required GameId GameId { get; init; }
    public required Player FirstPlayer { get; init; }
    public required Player SecondPlayer { get; init; }
    public PlayerId CurrentPlayerId { get; private set; }
    public bool IsComplete { get; private set; }
    public List<int> Log { get; } = new();
    public PlayerColor CurrentPlayerColor => Log.Count % 2 is 0 ? PlayerColor.Red : PlayerColor.Yellow;
    public void AddMove(int column)
    {
        Log.Add(column);
        CurrentPlayerId = Log.Count % 2 is 0 ? FirstPlayer.PlayerId : SecondPlayer.PlayerId;
    }
    public void Complete() => IsComplete = true;
}