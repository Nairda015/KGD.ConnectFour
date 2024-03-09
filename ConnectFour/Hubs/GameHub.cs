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
public class GameHub(IHubContext<GameHub> hubContext, BlazorRenderer renderer) : Hub
{
    private static readonly SortedSet<PlayerConnection> UsersSet = new(new PlayerConnectionComparer());
    private static readonly Dictionary<PlayerId, ConnectionId> ConnectedPlayers = new();
    public static PlayerConnection GetPlayerConnection(PlayerId playerId) => new(playerId, ConnectedPlayers[playerId]);
    public static PlayerConnection? FindOpponent()
    {
        var user = UsersSet.Min;
        if (user is null) return null;

        UsersSet.Remove(user);
        return user;
    }

    public async Task AddPlayerToQueue(PlayerConnection connection)
    {
        UsersSet.Add(connection);
        await hubContext.Clients
            .Client(connection.Connection)
            .SendAsync("show-indicator", await renderer.RenderComponent<Indicator>());
    }

    public async Task AddPlayersToGroup(GameLog log, CancellationToken ct)
    {
        await hubContext.Groups.AddToGroupAsync(log.FirstPlayerConnection.Connection, log.GameId, ct);
        await hubContext.Groups.AddToGroupAsync(log.SecondPlayerConnection.Connection, log.GameId, ct);
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
    
    public async Task NotifyAboutGameStart(GameId gameId, CancellationToken ct)
    {
        await hubContext.Clients
            .Group(gameId)
            .SendAsync("game-started", ct);
        
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
        var queryString = ctx.Request.Query["playerId"].ToString();
        var playerId = new PlayerId(queryString);
        ConnectedPlayers.Add(playerId, new ConnectionId(Context.ConnectionId));
        await base.OnConnectedAsync();
    }
    
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var user = UsersSet.FirstOrDefault(x => x.Connection == Context.ConnectionId);
        if (user is null) return;
        UsersSet.Remove(user);
        ConnectedPlayers.Remove(user.PlayerId);
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