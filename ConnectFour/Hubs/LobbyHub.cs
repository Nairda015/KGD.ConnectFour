using System.Collections.Concurrent;
using ConnectFour.Components.Shared;
using ConnectFour.Persistance;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.SignalR;

namespace ConnectFour.Hubs;

public class LobbyHub(LobbyQueue lobbyQueue) : Hub
{
    public override Task OnConnectedAsync()
    {
        lobbyQueue.Add(Context.ConnectionId, null);
        Clients.All.SendAsync("lobby-update", $"Player added {Context.ConnectionId}");
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        lobbyQueue.Remove(Context.ConnectionId);
        Clients.All.SendAsync("lobby-update", $"Player removed {Context.ConnectionId}");
        return base.OnDisconnectedAsync(exception);
    }

    public void UpdateLobbyAfterGameStarted(GameId gameId)
    {
        Clients.All.SendAsync("lobby-update", $"Player {Context.ConnectionId} joined {gameId.Value}");
        lobbyQueue.Update(Context.ConnectionId, gameId);
    }
}

public class LobbyQueue
{
    private static readonly ConcurrentDictionary<string, GameId?> UsersList = new();

    public void Add(string key, GameId? value) => UsersList.TryAdd(key, value);
    public void Update(string key, GameId? value) => UsersList.TryUpdate(key, value, UsersList[key]);
    public void Remove(string key) => UsersList.TryRemove(key, out _);
    public IEnumerable<(string, GameId?)> GetAll() => UsersList.Select(x => (x.Key, x.Value));
    
}