using System.Collections.Concurrent;
using ConnectFour.Domain;
using ConnectFour.Models;
using ConnectFour.Persistence;
using Microsoft.AspNetCore.SignalR;

namespace ConnectFour.Hubs;

public record NewGameMessage(string PlayerId);

public class GameHub(IHubContext<GameHub> hubContext, GamesContext context, PlayersContext players) : Hub
{
    private static readonly ConcurrentQueue<PlayerConnection> UsersQueue = new();

    public async Task NewGame(NewGameMessage message)
    {
        var conn = new ConnectionId(Context.ConnectionId);
        var userId = new PlayerId(message.PlayerId);

        if (UsersQueue.IsEmpty)
        {
            UsersQueue.Enqueue(new PlayerConnection(userId, conn));
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

        var message = $"""
                        <div class="hidden" hx-get="/game-url/{log.GameId.ToString()}" hx-trigger="load"></div>
                        <div class="hidden" hx-get="/in-game-buttons" hx-trigger="load" hx-swap="outerHTML" hx-target="#new-game-buttons"></div>
                        <script>sessionStorage.setItem("GameId", "{log.GameId.ToString()}");</script>
                        """;
        await hubContext.Clients
            .Group(log.GameId.Value)
            .SendAsync("game-started", message, ct);
    }

    public async Task SendCompletedGameMessage(GameId gameId, PlayerId winnerId)
    {
        var message = $"""
                       <div class="hidden" hx-get="/new-game-buttons" hx-trigger="load" hx-swap="outerHTML" hx-target="#in-game-buttons"></div>
                       <script>alert("Player {winnerId.Value} won!");</script>
                       """;
        await hubContext.Clients
            .Group(gameId.Value)
            .SendAsync("game-completed", message);
    }
    
    public async Task SendResignationMessage(GameId gameId, PlayerId winnerId)
    {
        var message = $"""
                       <div class="hidden" hx-get="/new-game-buttons" hx-trigger="load" hx-swap="outerHTML" hx-target="#in-game-buttons"></div>
                       <script>alert("Resignation, player {winnerId.Value} won!");</script>
                       """;
        await hubContext.Clients
            .Group(gameId.Value)
            .SendAsync("game-completed", message);
    }

    public async Task MarkMove(GameLog log, Position movePosition, CancellationToken ct)
    {
        var colour = log.CurrentPlayerColor.ToString().ToLower();
        var message = $"""<div class="aspect-square bg-{colour}-600 border-2 rounded-full border-black"></div>""";

        await hubContext.Clients
            .Group(log.GameId.Value)
            .SendAsync($"mark-move-{movePosition.Row}-{movePosition.Column}", message, ct);
    }
}