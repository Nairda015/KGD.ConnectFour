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

        await hubContext.Clients
            .Group(log.GameId.Value)
            .SendAsync("game-started", "game-started", ct);
    }

    public async Task SendCompletedGameMessage(GameId gameId, PlayerId playerId)
    {
        lobby.UpdateLobbyAfterGameEnded(gameId);
        await hubContext.Clients
            .Group(gameId.Value)
            .SendAsync($"game-completed-{gameId.Value}", $"Player {playerId.Value} won!</p>");
    }
}