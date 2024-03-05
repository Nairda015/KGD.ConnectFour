using ConnectFour.Components.Shared.Board;
using ConnectFour.Components.Shared.Game;
using ConnectFour.Components.Shared.Notifications;
using ConnectFour.Domain;
using ConnectFour.Extensions;
using ConnectFour.Models;
using ConnectFour.Persistence;
using Microsoft.AspNetCore.SignalR;

namespace ConnectFour.Hubs;

public record NewGameMessage(string PlayerId);

//TODO: Remove from queue after connection lost 
public class GameHub(IHubContext<GameHub> hubContext,
    GamesContext context,
    PlayersContext players,
    BlazorRenderer renderer,
    ILogger<GameHub> logger) : Hub
{
    private static readonly SortedSet<PlayerConnection> UsersSet = new(new PlayerConnectionComparer());

    public async Task NewGame(NewGameMessage message)
    {
        var connectionId = new ConnectionId(Context.ConnectionId);
        var playerId = new PlayerId(message.PlayerId);
        var playerConnection = new PlayerConnection(playerId, connectionId);
        
        logger.LogDebug("Player with id {PlayerId} requested new game", playerId);
        logger.LogDebug("Game hub connection id for {PlayerId} is {ConnectionId}", playerId, connectionId);
        
        if (UsersSet.Count is 0)
        {
            UsersSet.Add(playerConnection);
            await hubContext.Clients
                .Client(connectionId)
                .SendAsync("show-indicator", await renderer.RenderComponent<Indicator>());
            return;
        }

        //TODO: is this good idea? (multithreading)
        var secondUser = UsersSet.Min;
        if (secondUser is null) return;

        UsersSet.Remove(secondUser);
        
        var log = new GameLog
        {
            GameId = GameId.Create(),
            FirstPlayerConnection = playerConnection,
            SecondPlayerConnection = secondUser,
        };

        await StartGame(log, CancellationToken.None);
    }

    private async Task StartGame(GameLog log, CancellationToken ct)
    {
        var (gameId, firstPlayerId, secondPlayerId) = log;
        context.NewGame(log);
        await players.GameStarted(firstPlayerId, gameId);
        await players.GameStarted(secondPlayerId, gameId);

        await hubContext.Groups.AddToGroupAsync(log.FirstPlayerConnection.Connection, log.GameId.Value, ct);
        await hubContext.Groups.AddToGroupAsync(log.SecondPlayerConnection.Connection, log.GameId.Value, ct);

        //TODO: HACKS!!!
        var message = $"""
                       <script>sessionStorage.setItem("GameId", "{log.GameId.ToString()}");</script>
                       <div class="hidden" hx-get="/game-url/{log.GameId.ToString()}" hx-trigger="load"></div>
                       <div class="hidden" hx-get="/refresh-board" hx-trigger="load" hx-swap="outerHTML" hx-target="#board"></div>
                       """;
        
        await hubContext.Clients
            .Group(log.GameId.Value)
            .SendAsync("game-started", message, ct);
        
        await hubContext.Clients
            .Group(log.GameId.Value)
            .SendAsync("refresh-control-panel", ct);
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
    
    public async Task MarkMove(GameLog log, Position movePosition, CancellationToken ct)
    {
        var colour = log.CurrentPlayerColor.ToString().ToLower();
        var message = await renderer.RenderComponent<Disc>(
            new Dictionary<string, object?> {{nameof(Disc.Colour), colour}});

        await hubContext.Clients
            .Group(log.GameId.Value)
            .SendAsync($"mark-move-{movePosition.Row}-{movePosition.Column}", message, ct);
        
        await hubContext.Clients
            .Group(log.GameId.Value)
            .SendAsync("current-player", log.GetCurrentPlayerId, ct);
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
    
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var user = UsersSet.FirstOrDefault(x => x.Connection == Context.ConnectionId);
        if (user is null) return;
        UsersSet.Remove(user);
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