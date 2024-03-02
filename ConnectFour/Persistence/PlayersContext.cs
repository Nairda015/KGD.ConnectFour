using System.Threading.Channels;
using ConnectFour.Models;

namespace ConnectFour.Persistence;

//Update LastTimeActive only in connection events
public class PlayersContext(Channel<LobbyUpdateToken> lobbyChannel) : Dictionary<PlayerId, Player>
{
    private const int LobbyPlayersCount = 10;

    public ValueTask PlayerConnected(PlayerId playerId)
    {
        if (TryGetValue(playerId, out var player))
        {
            player.IsActive = true;
            player.LastTimeActive = Today();
            return ShouldUpdateCache(player, InvalidationScenario.KnownPlayerConnected)
                ? UpdateCache()
                : ValueTask.CompletedTask;
        }

        var newPlayer = new Player
        {
            Id = playerId,
            IsActive = true,
            LastTimeActive = Today(),
            Score = new Score()
        };

        Add(playerId, newPlayer);
        return ShouldUpdateCache(newPlayer, InvalidationScenario.LobbyIsNotFull)
            ? UpdateCache()
            : ValueTask.CompletedTask;
    }

    public ValueTask PlayerDisconnected(PlayerId id)
    {
        var player = this[id];
        player.IsActive = false;
        if (ShouldUpdateCache(player, InvalidationScenario.PlayerDisconnected)) return UpdateCache();
        player.LastTimeActive = Today();
        return ValueTask.CompletedTask;
    }

    public ValueTask GameStarted(PlayerId playerId, GameId gameId)
    {
        var player = this[playerId];
        player.CurrentGame = gameId;
        return ShouldUpdateCache(player, InvalidationScenario.PlayerGameStarted)
            ? UpdateCache()
            : ValueTask.CompletedTask;
    }

    public async ValueTask GameEnded(PlayerId winnerId, PlayerId loserId)
    {
        var winner = this[winnerId];
        var loser = this[loserId];
        
        GameEnded(winner, GameResult.Win);
        GameEnded(loser, GameResult.Lose);

        if (ShouldUpdateCache(winner, InvalidationScenario.GameEnded)) await UpdateCache();
        if (ShouldUpdateCache(loser, InvalidationScenario.GameEnded)) await UpdateCache();
    }
    
    private static void GameEnded(Player player, GameResult gameResult)
    {
        player.CurrentGame = null;
        _ = gameResult switch
        {
            GameResult.Win => player.Score.LogWin(),
            GameResult.Draw => player.Score.LogDraw(),
            GameResult.Lose => player.Score.LogLose(),
            _ => throw new ArgumentOutOfRangeException(nameof(gameResult), gameResult, null)
        };
    }

    public Score GetPlayerScore(PlayerId playerId) => ContainsKey(playerId) ? this[playerId].Score : Score.Default;
    private List<LobbyReadModel> _cachedBestPlayers = new(LobbyPlayersCount);
    public IEnumerable<LobbyReadModel> GetBestPlayers() => _cachedBestPlayers;

    private bool ShouldUpdateCache(Player player, InvalidationScenario scenario) => scenario switch
    {
        InvalidationScenario.KnownPlayerConnected => _cachedBestPlayers.Any(x => x.Score.Wins <= player.Score.Wins),
        InvalidationScenario.GameEnded => _cachedBestPlayers.Any(x => x.Score.Wins <= player.Score.Wins),
        InvalidationScenario.PlayerDisconnected => _cachedBestPlayers.Any(x => x.PlayerId == player.Id),
        InvalidationScenario.PlayerGameStarted => _cachedBestPlayers.Any(x => x.PlayerId == player.Id),
        InvalidationScenario.LobbyIsNotFull => _cachedBestPlayers.Count < LobbyPlayersCount,
        _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, null)
    };

    private ValueTask UpdateCache()
    {
        _cachedBestPlayers = Values
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.Score.Wins)
            .Take(LobbyPlayersCount)
            .Select(x => new LobbyReadModel(x.Score, x.Id, x.CurrentGame))
            .ToList();

        return lobbyChannel.Writer.WriteAsync(new LobbyUpdateToken());
    }

    private static DateOnly Today() => DateOnly.FromDateTime(DateTime.Today);
}

public record LobbyReadModel(Score Score, PlayerId PlayerId, GameId? GameId);

public enum GameResult
{
    Lose = 0,
    Win = 1,
    Draw = 2,
}

public record Player
{
    public required PlayerId Id { get; init; }
    public required Score Score { get; init; } //TODO: initialization scenarios?
    public bool IsActive { get; set; }
    public required DateOnly LastTimeActive { get; set; }
    public GameId? CurrentGame { get; set; }
}

public enum InvalidationScenario
{
    KnownPlayerConnected = 0,
    GameEnded = 1,
    PlayerDisconnected = 2,
    PlayerGameStarted = 3,
    LobbyIsNotFull = 4,
}

public struct LobbyUpdateToken;

public class Score
{
    public static readonly Score Default = new();
    public int Wins { get; private set; }
    public int Draws { get; private set; }
    public int Losses { get; private set; }
    public int LogWin() => ++Wins;
    public int LogDraw() => ++Draws;
    public int LogLose() => ++Losses;
    public override string ToString() => $"{Wins} / {Draws} / {Losses}";
}