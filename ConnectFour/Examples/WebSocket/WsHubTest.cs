using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;

namespace ConnectFour.Examples.WebSocket;


public class WsHubTest(IHubContext<WsHubTest> hubContext) : Hub
{
    public async Task SendMessage(string message)
    {
        await Clients.All.SendAsync("new-message", $"""<p> {message} </p>""");
    }
    
    public static string Create(int size = 10)
    {
        var buffer = new byte[size];
        Random.Shared.NextBytes(buffer);
        return Base64UrlEncoder.Encode(buffer);
    }
    
    public async Task SendHeartbeatMessage(CancellationToken ct)
    {
        // var data = new List<string>();
        //
        // for (int i = 0; i < 10_000_000; i++)
        // {
        //     data.Add(Create(7));
        // }
        //
        // //var stats = data.GroupBy(x => x.Length).ToList();
        // var result = data.DistinctBy(x => x).Count();
        // Console.WriteLine(result);
        
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