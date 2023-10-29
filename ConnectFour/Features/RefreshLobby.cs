using ConnectFour.Components.Shared;
using Microsoft.AspNetCore.Http.HttpResults;
using MiWrap;

namespace ConnectFour.Features;

internal record RefreshLobby : IHttpQuery;

public class LobbyQueueEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder) =>
        builder.MapGet<RefreshLobby, LobbyQueueHandler>("refresh-lobby")
            .Produces(200)
            .Produces(400)
            .DisableAntiforgery();
}

internal class LobbyQueueHandler : IHttpQueryHandler<RefreshLobby>
{
    public async Task<IResult> HandleAsync(RefreshLobby command, CancellationToken cancellationToken = default)
    {
        return new RazorComponentResult(typeof(PlayersList));
    }
}