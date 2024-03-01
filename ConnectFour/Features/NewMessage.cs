using ConnectFour.Extensions;
using ConnectFour.Hubs;
using MiWrap;

namespace ConnectFour.Features;

internal record NewMessage(string Message) : IHttpCommand;

public class NewMessageEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder) =>
        builder.MapFormPost<NewMessage, NewMessageHandler>("new-message")
            .Produces(200)
            .Produces(400)
            .DisableAntiforgery();
}

internal class NewMessageHandler(WsHubTest hub) : IHttpCommandHandler<NewMessage>
{
    public async Task<IResult> HandleAsync(NewMessage command, CancellationToken cancellationToken = default)
    {
        await hub.SendMessage(command.Message);
        return Results.Accepted();
    }
}