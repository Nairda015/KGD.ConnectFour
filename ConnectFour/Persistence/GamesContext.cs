using System.Collections.Concurrent;
using ConnectFour.Domain;
using ConnectFour.Models;

namespace ConnectFour.Persistence;

public class GamesContext
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
    public bool StartGameRecording(GameLog log) => _gamesState.TryAdd(log.GameId, log);

    public GameLog? GetPlayerGame(PlayerId playerId) => _gamesState.FirstOrDefault(x =>
        x.Value.FirstPlayerConnection.PlayerId == playerId ||
        x.Value.SecondPlayerConnection.PlayerId == playerId).Value;
}