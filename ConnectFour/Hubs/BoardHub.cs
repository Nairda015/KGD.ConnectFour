using Microsoft.AspNetCore.SignalR;

namespace ConnectFour.Hubs;

public class BoardHub : Hub
{
    public async Task SendMessage(string message)
    {
        await Clients.All.SendAsync("new-message", $"""<p> {message} </p>""");
    }
}

public class WsHubTest(IHubContext<WsHubTest> hubContext) : Hub
{
    public async Task SendMessage(string message)
    {
        await Clients.All.SendAsync("new-message", $"""<p> {message} </p>""");
    }
    
    public async Task SendHeartbeatMessage(CancellationToken ct)
    {
        var random = Random.Shared.Next();

        if (random %2 == 0)
        {
            var message = $"""<div class="bg-red-200">{random}</div>""";
            await hubContext.Clients.All.SendAsync("new-hb-message", message, ct);
        }
        else
        {
            var message = $"""<div class="bg-blue-200">{random}</div>""";
            await hubContext.Clients.All.SendAsync("new-hb-message2", message, ct);
        }
    }
}

public class BackgroundPublisher(WsHubTest hub) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await hub.SendHeartbeatMessage(stoppingToken);

            await Task.Delay(1000, stoppingToken);
        }
    }
}