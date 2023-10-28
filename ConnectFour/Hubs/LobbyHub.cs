using ConnectFour.Domain;
using ConnectFour.Persistence;
using Microsoft.AspNetCore.SignalR;

namespace ConnectFour.Hubs;

public class LobbyHub(Lobby lobby) : Hub
{
    public override Task OnConnectedAsync()
    {
        //TODO get player id maybe from headers?
        var playerId = new PlayerId(Context.ConnectionId);
        lobby.AddNewPlayer(playerId);
        Clients.All.SendAsync("lobby-update", $"Player added {playerId.ToString()}");
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        var playerId = new PlayerId(Context.ConnectionId);
        lobby.Remove(playerId);
        Clients.All.SendAsync("lobby-update", $"Player removed {playerId.ToString()}");
        return base.OnDisconnectedAsync(exception);
    }

    public void UpdateLobbyAfterGameStarted(GameLog log)
    {
        lobby.AddNewGame(log);
        //TODO: fix this after response on github
        Clients.All.SendAsync("lobby-update", $"Game with Id {log.GameId} started");
    }
    
    public void UpdateLobbyAfterGameEnded(GameId gameId)
    {
        lobby.GameFinished(gameId);
        //TODO: fix this after response on github
        Clients.All.SendAsync("lobby-update", $"Game with Id {gameId.ToString()} finished");
    }
}