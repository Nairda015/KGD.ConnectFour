using System.Collections.Concurrent;
using ConnectFour.Domain;
using ConnectFour.Persistence;
using Microsoft.AspNetCore.SignalR;

namespace ConnectFour.Hubs;

public record NewGameMessage(string PlayerId);

public class GameHub(IHubContext<GameHub> hubContext, InMemoryGamesState state, LobbyHub lobby) : Hub
{
    private static readonly ConcurrentQueue<Player> UsersQueue = new();

    public async Task NewGame(NewGameMessage message)
    {
        var conn = Context.ConnectionId;
        var userId = new PlayerId(message.PlayerId);

        if (UsersQueue.IsEmpty)
        {
            UsersQueue.Enqueue(new Player(userId, conn));
            return;
        }

        //TODO: is this good idea?
        if (!UsersQueue.TryDequeue(out var secondUser)) return;

        var log = new GameLog
        {
            GameId = GameId.Create(),
            FirstPlayer = new Player(userId, conn),
            SecondPlayer = secondUser,
        };

        await StartGame(log, CancellationToken.None);
    }

    private async Task StartGame(GameLog log, CancellationToken ct)
    {
        state.NewGame(log);
        lobby.UpdateLobbyAfterGameStarted(log);
        
        await hubContext.Groups.AddToGroupAsync(log.FirstPlayer.Connection, log.GameId.Value, ct);
        await hubContext.Groups.AddToGroupAsync(log.SecondPlayer.Connection, log.GameId.Value, ct);

        var message = $"""
                        <div class="hidden" hx-get="/game-url/{log.GameId.ToString()}" hx-trigger="load"></div>
                        <div class="hidden" hx-get="/game-buttons" hx-trigger="load" hx-swap="outerHTML" hx-target="#new-game-button"></div>
                        <script>sessionStorage.setItem("GameId", "{log.GameId.ToString()}");</script>
                        """;
        await hubContext.Clients
            .Group(log.GameId.Value)
            .SendAsync("game-started", message, ct);
    }

    public async Task SendCompletedGameMessage(GameId gameId, PlayerId playerId)
    {
        lobby.UpdateLobbyAfterGameEnded(gameId);
        
        var message = $"""<script>alert("Resignation, player {playerId.Value} won!");</script>""";
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