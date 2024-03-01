using ConnectFour.Extensions;
using MiWrap;

namespace ConnectFour.Examples.WebSocket;

internal record WsTestMessage(string Message) : IHttpCommand;

public class WsTestMessageEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder) =>
        builder.MapFormPost<WsTestMessage, WsTestMessageHandler>("new-message")
            .Produces(200)
            .Produces(400)
            .DisableAntiforgery();
}

internal class WsTestMessageHandler(WsHubTest hub) : IHttpCommandHandler<WsTestMessage>
{
    public async Task<IResult> HandleAsync(WsTestMessage command, CancellationToken cancellationToken = default)
    {
        await hub.SendMessage(command.Message);
        return Results.Accepted();
    }
}