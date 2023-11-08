using System.Collections.Concurrent;
using ConnectFour.Components.Shared;
using ConnectFour.Components.Shared.Notifications;
using ConnectFour.Domain;
using ConnectFour.Extensions;
using ConnectFour.Models;
using ConnectFour.Persistence;
using Microsoft.AspNetCore.SignalR;

namespace ConnectFour.Hubs;

public record NewGameMessage(string PlayerId);

public class GameHub(IHubContext<GameHub> hubContext,
    GamesContext context,
    PlayersContext players,
    BlazorRenderer renderer) : Hub
{
    private static readonly ConcurrentQueue<PlayerConnection> UsersQueue = new();

    public async Task NewGame(NewGameMessage message)
    {
        var conn = new ConnectionId(Context.ConnectionId);
        var userId = new PlayerId(message.PlayerId);
        
        if (UsersQueue.IsEmpty)
        {
            UsersQueue.Enqueue(new PlayerConnection(userId, conn));
            await hubContext.Clients
                .Client(conn)
                .SendAsync("show-indicator", await renderer.RenderComponent<Indicator>());
            return;
        }

        //TODO: is this good idea?
        if (!UsersQueue.TryDequeue(out var secondUser)) return;

        var log = new GameLog
        {
            GameId = GameId.Create(),
            FirstPlayerConnection = new PlayerConnection(userId, conn),
            SecondPlayerConnection = secondUser,
        };

        await StartGame(log, CancellationToken.None);
    }

    private async Task StartGame(GameLog log, CancellationToken ct)
    {
        var (gameId, firstPlayerId, secondPlayerId) = log;
        context.NewGame(log);
        players.GameStarted(firstPlayerId, gameId);
        players.GameStarted(secondPlayerId, gameId);

        await hubContext.Groups.AddToGroupAsync(log.FirstPlayerConnection.Connection, log.GameId.Value, ct);
        await hubContext.Groups.AddToGroupAsync(log.SecondPlayerConnection.Connection, log.GameId.Value, ct);

        //TODO: HACKS!!!
        var message = $"""
                       <div class="hidden" hx-get="/game-url/{log.GameId.ToString()}" hx-trigger="load"></div>
                       <div class="hidden" hx-get="/in-game-buttons" hx-trigger="load" hx-swap="outerHTML" hx-target="#new-game-buttons"></div>
                       <div class="hidden" hx-get="/refresh-board" hx-trigger="load" hx-swap="outerHTML" hx-target="#board"></div>
                       <script>sessionStorage.setItem("GameId", "{log.GameId.ToString()}");</script>
                       """;
        await hubContext.Clients
            .Group(log.GameId.Value)
            .SendAsync("game-started", message, ct);
    }

    public async Task SendCompletedGameMessage(GameId gameId, PlayerId winnerId)
    {
        var message = await renderer.RenderComponent<GameCompletedMessage>(
            new Dictionary<string, object?> {{nameof(GameCompletedMessage.PlayerId), winnerId}});
        await hubContext.Clients
            .Group(gameId.Value)
            .SendAsync("game-completed", message);
    }

    public async Task SendResignationMessage(GameId gameId, PlayerId winnerId)
    {
        var message = await renderer.RenderComponent<ResignationMessage>(
            new Dictionary<string, object?> {{nameof(ResignationMessage.PlayerId), winnerId}});
        await hubContext.Clients
            .Group(gameId.Value)
            .SendAsync("game-completed", message);
    }

    public async Task MarkMove(GameLog log, Position movePosition, CancellationToken ct)
    {
        var colour = log.CurrentPlayerColor.ToString().ToLower();
        var message = await renderer.RenderComponent<Disc>(
            new Dictionary<string, object?> {{nameof(Disc.Colour), colour}});

        await hubContext.Clients
            .Group(log.GameId.Value)
            .SendAsync($"mark-move-{movePosition.Row}-{movePosition.Column}", message, ct);
    }
}