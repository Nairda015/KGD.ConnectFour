using ConnectFour.Models;

namespace ConnectFour.Persistence;

//Update LastTimeActive only in connection events
public class PlayersContext : Dictionary<PlayerId, Player>
{
    private const int LobbyPlayersCount = 10;
    public void PlayerConnected(PlayerId playerId)
    {
        if (TryGetValue(playerId, out var player))
        {
            player.IsActive = true;
            player.LastTimeActive = Today();
            if (ShouldInvalidateCache(player, InvalidationScenario.KnownPlayerConnected)) InvalidateCache();
            return;
        }

        var newPlayer = new Player
        {
            Id = playerId,
            IsActive = true,
            LastTimeActive = Today(),
            Score = new Score()
        };

        Add(playerId, newPlayer);
        if (ShouldInvalidateCache(newPlayer, InvalidationScenario.LobbyIsNotFull)) InvalidateCache();
    }
    public void PlayerDisconnected(PlayerId id)
    {
        var player = this[id];
        player.IsActive = false;
        if (ShouldInvalidateCache(player, InvalidationScenario.KnownPlayerConnected)) InvalidateCache();
        player.LastTimeActive = Today();
    }
    public void GameStarted(PlayerId playerId, GameId gameId)
    {
        var player = this[playerId];
        player.CurrentGame = gameId;
        if (ShouldInvalidateCache(player, InvalidationScenario.TopPlayerGameStarted)) InvalidateCache();
    }
    public void GameEnded(PlayerId playerId, GameResult gameResult)
    {
        var player = this[playerId];

        player.CurrentGame = null;
        _ = gameResult switch
        {
            GameResult.Win => player.Score.LogWin(),
            GameResult.Draw => player.Score.LogDraw(),
            GameResult.Lose => player.Score.LogLose(),
            _ => throw new ArgumentOutOfRangeException(nameof(gameResult), gameResult, null)
        };

        if (ShouldInvalidateCache(player, InvalidationScenario.GameEnded)) InvalidateCache();
    }
    
    private List<LobbyReadModel>? _cachedBestPlayers;
    public IEnumerable<LobbyReadModel> GetBestPlayers() => _cachedBestPlayers ??= Values
        .Where(x => x.IsActive)
        .OrderByDescending(x => x.Score.Wins)
        .Take(LobbyPlayersCount)
        .Select(x => new LobbyReadModel(x.Score, x.Id, x.CurrentGame))
        .ToList();
    private bool ShouldInvalidateCache(Player player, InvalidationScenario scenario)
    {
        if(_cachedBestPlayers is null || Count is 0) return false;
        if (_cachedBestPlayers.Count < LobbyPlayersCount) return true;
        return scenario switch
        {
            InvalidationScenario.KnownPlayerConnected => player.Score.Wins > _cachedBestPlayers.Last().Score.Wins,
            InvalidationScenario.GameEnded => player.Score.Wins > _cachedBestPlayers.Last().Score.Wins,
            InvalidationScenario.TopPlayerDisconnected => _cachedBestPlayers.Any(x => x.PlayerId == player.Id),
            InvalidationScenario.TopPlayerGameStarted => _cachedBestPlayers.Any(x => x.PlayerId == player.Id),
            InvalidationScenario.LobbyIsNotFull => _cachedBestPlayers?.Count < LobbyPlayersCount,
            _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, null)
        };
    }
    private void InvalidateCache() => _cachedBestPlayers = null;
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
    TopPlayerDisconnected = 2,
    TopPlayerGameStarted = 3,
    LobbyIsNotFull = 4,
}

public class Score
{
    public int Wins { get; private set; }
    public int Draws { get; private set; }
    public int Losses { get; private set; }
    public int LogWin() => ++Wins;
    public int LogDraw() => ++Draws;
    public int LogLose() => ++Losses;
    public override string ToString() => $"{Wins} / {Draws} / {Losses}";
}