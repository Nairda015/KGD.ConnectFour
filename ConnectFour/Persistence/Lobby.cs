using System.Collections.Concurrent;
using ConnectFour.Domain;

namespace ConnectFour.Persistence;

public class Lobby
{
    private static readonly ConcurrentDictionary<PlayerId, GameId?> FromPlayerId = new();
    private static readonly ConcurrentDictionary<GameId, (PlayerId Player1, PlayerId Player2)> FromGameId = new();

    public bool AddNewPlayer(PlayerId player) => FromPlayerId.TryAdd(player, default);
    public bool AddNewGame(GameLog log)
    {
        var gameId = log.GameId;
        var player1 = log.FirstPlayer.PlayerId;
        var player2 = log.SecondPlayer.PlayerId;
        
        var a = FromGameId.TryAdd(gameId, (player1, player2));
        
        var u1GameId = FromPlayerId[player1];
        var b = FromPlayerId.TryUpdate(player1, gameId, u1GameId);

        var u2GameId = FromPlayerId[player2];
        var c = FromPlayerId.TryUpdate(player2, gameId, u2GameId);

        return a && b && c;
    }
    public bool GameFinished(GameId gameId)
    {
        var a = FromGameId.TryRemove(gameId, out var users);
        
        var b = FromPlayerId.TryUpdate(users.Player1, default, gameId);
        var c = FromPlayerId.TryUpdate(users.Player2, default, gameId);
        
        return a && b && c;
    }
    public bool Remove(PlayerId playerId)
    {
        var a = FromPlayerId.TryRemove(playerId, out var maybeGameId);
        if (maybeGameId is not { } gameId) return a;
        
        var b = FromGameId.TryRemove(gameId, out var users);
        FromPlayerId.TryRemove(users.Player1, out _);
        FromPlayerId.TryRemove(users.Player2, out _);

        return a && b;
    }
    public IEnumerable<(PlayerId, GameId?)> GetAll()
        => FromPlayerId.Select(x => (x.Key, x.Value));
}