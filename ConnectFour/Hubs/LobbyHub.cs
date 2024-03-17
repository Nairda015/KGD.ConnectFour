using System.Threading.Channels;
using ConnectFour.Extensions;
using ConnectFour.Persistence;
using Microsoft.AspNetCore.SignalR;

namespace ConnectFour.Hubs;

public class LobbyHub(PlayersContext players, ILogger<LobbyHub> logger) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var ctx = Context.GetHttpContext()!;
        var playerId = ctx.User.GetPlayerId();
        logger.LogDebug("Player with Id {PlayerId} connected", playerId);
        logger.LogDebug("Player with Id {PlayerId} lobby connection id {ConnectionId}", playerId, Context.ConnectionId);
        await players.PlayerConnected(playerId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var ctx = Context.GetHttpContext()!;
        var playerId = ctx.User.GetPlayerId();
        logger.LogDebug("Player with Id {PlayerId} disconnected", playerId);
        await players.PlayerDisconnected(playerId);
        await base.OnDisconnectedAsync(exception);
    }
}

public class LobbyUpdateConsumer(Channel<LobbyUpdateToken> channel, IHubContext<LobbyHub> hubContext) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!channel.Reader.Completion.IsCompleted && await channel.Reader.WaitToReadAsync(stoppingToken))
        {
            if (!channel.Reader.TryRead(out _)) continue;
            await hubContext.Clients.All.SendAsync("lobby-updated", stoppingToken);
        }
    }
}