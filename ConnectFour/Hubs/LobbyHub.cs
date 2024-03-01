using ConnectFour.Models;
using ConnectFour.Persistence;
using Microsoft.AspNetCore.SignalR;

namespace ConnectFour.Hubs;

public class LobbyHub(PlayersContext players, ILogger<LobbyHub> logger) : Hub
{
    public override Task OnConnectedAsync()
    {
        var ctx = Context.GetHttpContext()!;
        var queryString = ctx.Request.Query["playerId"].ToString();
        var playerId = new PlayerId(queryString);
        logger.LogDebug("Player with Id {PlayerId} connected", playerId);
        logger.LogDebug("Player with Id {PlayerId} lobby connection id {ConnectionId}", playerId, Context.ConnectionId);
        players.PlayerConnected(playerId);
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        var ctx = Context.GetHttpContext()!;
        var queryString = ctx.Request.Query["playerId"].ToString();
        var playerId = new PlayerId(queryString);
        logger.LogDebug("Player with Id {PlayerId} disconnected", playerId);
        players.PlayerDisconnected(playerId);
        return base.OnDisconnectedAsync(exception);
    }

    public void SendLobbyUpdatedNotification()
    {
        //TODO: HACKS!!!
        const string message = """<div class="hidden" hx-get="/refresh-lobby" hx-trigger="load" hx-swap="outerHTML" hx-target="#lobby-table-body"></div>""";
        
        Clients.All.SendAsync("lobby-updated", message);
    }
}