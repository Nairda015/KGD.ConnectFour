using ConnectFour.Persistance;

namespace ConnectFour.Domain;

public class GameLog
{
    public required GameId GameId { get; init; }
    public required string FirstPlayerId { get; init; }
    public required string SecondPlayerId { get; init; }
    public string CurrentPlayerId { get; private set; } = default!;
    public bool IsComplete { get; private set; }
    public List<int> Log { get; } = new();
    public Player CurrentPlayer => Log.Count % 2 is 0 ? Player.Red : Player.Yellow;
    public void AddMove(int column)
    {
        Log.Add(column);
        CurrentPlayerId = Log.Count % 2 is 0 ? FirstPlayerId : SecondPlayerId;
    }
    public void Complete() => IsComplete = true;
}