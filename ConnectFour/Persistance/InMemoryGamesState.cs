using System.Collections.Concurrent;

namespace ConnectFour.Persistance;

public class InMemoryGamesState
{
    private readonly ConcurrentDictionary<int, int> _gamesState = new();
    
    public int GetState(int gameId) => _gamesState.TryGetValue(gameId, out var state) ? state : 0;

    public void UpdateState(int gameId)
    {
        var state = GetState(gameId);
        state++;
        _gamesState[gameId] = state;
    }
}