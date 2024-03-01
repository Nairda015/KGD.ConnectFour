using ConnectFour.Components.Shared;
using Microsoft.AspNetCore.Http.HttpResults;
using MiWrap;

namespace ConnectFour.Features;

internal record RefreshLobby : IHttpQuery;

public class RefreshLobbyEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder) =>
        builder.MapGet<RefreshLobby, RefreshLobbyHandler>("refresh-lobby")
            .Produces(200)
            .Produces(400)
            .DisableAntiforgery();
}

internal class RefreshLobbyHandler : IHttpQueryHandler<RefreshLobby>
{
    public async Task<IResult> HandleAsync(RefreshLobby command, CancellationToken cancellationToken = default)
        => new RazorComponentResult(typeof(PlayersList));
}