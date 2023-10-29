using System.Collections.Concurrent;
using ConnectFour.Domain;

namespace ConnectFour.Persistence;

public class InMemoryGamesState
{
    private readonly ConcurrentDictionary<GameId, GameLog> _gamesState = new();

    public bool TryGetGameState(GameId gameId, out GameLog? state)
    {
        var result = _gamesState.TryGetValue(gameId, out var log);
        state = log;
        return result;
    }

    public GameLog GetState(GameId gameId) => _gamesState[gameId];
    public void UpdateState(GameLog gameLog) => _gamesState[gameLog.GameId] = gameLog;
    public bool NewGame(GameLog log) => _gamesState.TryAdd(log.GameId, log);
}