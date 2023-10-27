using System.Collections.Concurrent;
using ConnectFour.Domain;

namespace ConnectFour.Persistance;

public class InMemoryGamesState
{
    private readonly ConcurrentDictionary<GameId, GameLog> _gamesState = new();
    
    public GameLog GetState(GameId gameId) => _gamesState[gameId];
    public void UpdateState(GameLog gameLog) => _gamesState[gameLog.GameId] = gameLog;
    public bool NewGame(GameLog log) => _gamesState.TryAdd(log.GameId, log);
}

public readonly record struct GameId(string Value)
{
    public static GameId Create() => new(Convert.ToBase64String(Guid.NewGuid().ToByteArray())[..^2]);
    public string Value { get; } = Value;
}

public readonly record struct UserId(string Value)
{
    public static UserId Create() => new(Convert.ToBase64String(Guid.NewGuid().ToByteArray())[..^2]);
    public string Value { get; } = Value;
}