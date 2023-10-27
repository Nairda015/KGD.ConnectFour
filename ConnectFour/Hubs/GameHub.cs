using ConnectFour.Persistance;
using Microsoft.AspNetCore.SignalR;

namespace ConnectFour.Hubs;

public class GameHub(IHubContext<GameHub> hubContext) : Hub
{
    public async Task AddToGroup(GameId gameId, CancellationToken cancellationToken)
    {
        var connectionId = Context.ConnectionId;
        await hubContext.Groups.AddToGroupAsync(connectionId, gameId.Value, cancellationToken);
    }

    public async Task SendMessage(string message)
    {
        await hubContext.Clients.All.SendAsync("new-message", $"""<p> {message} </p>""");
    }

    public async Task SendCompletedGameMessage(GameId gameId, string playerId)
    {
        await hubContext.Clients
            .Group(gameId.Value)
            .SendAsync($"game-completed-{gameId}", $"Player {playerId} won!</p>");
    }
}