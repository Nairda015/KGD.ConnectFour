using ConnectFour.Models;
using ConnectFour.Persistence;
using Microsoft.AspNetCore.SignalR;

namespace ConnectFour.Hubs;

public class LobbyHub(PlayersContext players) : Hub
{
    public override Task OnConnectedAsync()
    {
        var ctx = Context.GetHttpContext()!;
        var queryString = ctx.Request.Query["playerId"].ToString();
        var playerId = new PlayerId(queryString);
        players.PlayerConnected(playerId);
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        var ctx = Context.GetHttpContext()!;
        var queryString = ctx.Request.Query["playerId"].ToString();
        var playerId = new PlayerId(queryString);
        players.PlayerDisconnected(playerId);
        return base.OnDisconnectedAsync(exception);
    }
}