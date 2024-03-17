using System.Diagnostics;
using ConnectFour.Components.Shared.Board;
using ConnectFour.Components.Shared.Game;
using ConnectFour.Components.Shared.Notifications;
using ConnectFour.Domain;
using ConnectFour.Extensions;
using ConnectFour.Models;
using ConnectFour.Persistence;
using Microsoft.AspNetCore.SignalR;

namespace ConnectFour.Hubs;

public class GameHub(
    IHubContext<GameHub> hubContext,
    GamesContext gamesContext,
    PlayersContext playersContext,
    BlazorRenderer renderer) : Hub
{
    private static readonly SortedSet<PlayerConnection> UsersQueue = new(new PlayerConnectionComparer());
    private static readonly Dictionary<PlayerId, ConnectionId> ConnectedPlayers = new();

    public static PlayerConnection GetPlayerConnection(PlayerId playerId) => new(playerId, ConnectedPlayers[playerId]);
    public static PlayerConnection? FindOpponent()
    {
        var user = UsersQueue.Min;
        if (user is null) return null;

        UsersQueue.Remove(user);
        return user;
    }

    public bool CheckIfInTheQueue(PlayerId playerId) 
        => UsersQueue.FirstOrDefault(x => x.PlayerId == playerId) is not null;

    public async Task AddPlayerToQueue(PlayerConnection connection)
    {
        UsersQueue.Add(connection);
        await hubContext.Clients
            .Client(connection.Connection)
            .SendAsync("show-indicator", await renderer.RenderComponent<Indicator>());
    }

    public async Task AddPlayersToGroup(GameLog log, CancellationToken ct)
    {
        await hubContext.Groups.AddToGroupAsync(log.FirstPlayerConnection.Connection, log.GameId, ct);
        await hubContext.Groups.AddToGroupAsync(log.SecondPlayerConnection.Connection, log.GameId, ct);
    }
    
    public async Task MakeMove(MakeMoveMessage message)
    {
        var (playerId, gameId, chosenColumn) = message.ToMarkMove();
        var gameLog = gamesContext.GetState(new GameId(gameId));

        if (gameLog.IsComplete) return;
        if (gameLog.GetCurrentPlayerId != playerId) return;

        var game = new Game(gameLog);
        var move = game.MakeMove(chosenColumn);

        if (move.MoveResult is MoveResult.ColumnFull) return;

        gameLog.AddMove(chosenColumn);
        if (move.MoveResult is MoveResult.Win) gameLog.Complete();

        gamesContext.UpdateState(gameLog);

        //TODO: mark after adding move is buggy 
        //TODO: board is full scenario
        await MarkMove(gameLog, move.Position!.Value);
        
        if (gameLog.IsComplete)
        {
            var looser = gameLog.FirstPlayerConnection.PlayerId == playerId
                ? gameLog.SecondPlayerConnection.PlayerId
                : gameLog.FirstPlayerConnection.PlayerId;
            await playersContext.GameEnded(playerId, looser);
            
            await SendCompletedGameMessage(gameId, playerId);
        }
    }
    
    private async Task MarkMove(GameLog log, Position movePosition)
    {
        var colour = log.CurrentPlayerColor.ToString().ToLower();
        var message = await renderer.RenderComponent<Disc>(
            new Dictionary<string, object?> {{nameof(Disc.Colour), colour}});

        await hubContext.Clients
            .Group(log.GameId.Value)
            .SendAsync($"mark-move-{movePosition.Row}-{movePosition.Column}", message);
    }

    public async Task SendCompletedGameMessage(GameId gameId, PlayerId winnerId)
    {
        var message = await renderer.RenderComponent<GameCompletedMessage>(
            new Dictionary<string, object?> {{nameof(GameCompletedMessage.PlayerId), winnerId}});
        
        await NotifyAboutGameEnd(gameId, message);
    }

    public async Task SendResignationMessage(GameId gameId, PlayerId winnerId)
    {
        var message = await renderer.RenderComponent<ResignationMessage>(
            new Dictionary<string, object?> {{nameof(ResignationMessage.PlayerId), winnerId}});
        await NotifyAboutGameEnd(gameId, message);
    }
    
    public async Task NotifyAboutGameStart(GameId gameId, CancellationToken ct)
    {
        await hubContext.Clients
            .Group(gameId)
            .SendAsync("refresh-board", ct);
        
        await hubContext.Clients
            .Group(gameId)
            .SendAsync("refresh-control-panel", ct);
    }
    
    private async Task NotifyAboutGameEnd(GameId gameId, string message)
    {
        await hubContext.Clients
            .Group(gameId.Value)
            .SendAsync("game-completed", message);
        
        await hubContext.Clients
            .Group(gameId.Value)
            .SendAsync("player-score-updated");
        
        await hubContext.Clients
            .Group(gameId.Value)
            .SendAsync("refresh-control-panel");
    }
    
    public override async Task OnConnectedAsync()
    {
        var ctx = Context.GetHttpContext()!;
        var playerId = ctx.User.GetPlayerId();
        var added = ConnectedPlayers.TryAdd(playerId, new ConnectionId(Context.ConnectionId));
        if (!added) ConnectedPlayers[playerId] = new ConnectionId(Context.ConnectionId);
        await base.OnConnectedAsync();
    }
    
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var user = UsersQueue.FirstOrDefault(x => x.Connection == Context.ConnectionId);
        if (user is not null)
        {
            UsersQueue.Remove(user);
            ConnectedPlayers.Remove(user.PlayerId);
        }
        await base.OnDisconnectedAsync(exception);
    }
}

class PlayerConnectionComparer : IComparer<PlayerConnection>
{
    public int Compare(PlayerConnection? x, PlayerConnection? y)
    {
        if (x is null || y is null) throw new ArgumentNullException();
        return x.ConnectionTime.CompareTo(y.ConnectionTime);
    }
}

public record MakeMove(PlayerId PlayerId, GameId GameId, int ColumnNumber);

public record MakeMoveMessage(string PlayerId, string GameId, string ColumnNumber)
{
    internal MakeMove ToMarkMove() => new(PlayerId, GameId, int.Parse(ColumnNumber));
}