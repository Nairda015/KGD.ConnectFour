using ConnectFour.Components.Shared.Game;
using ConnectFour.Models;
using ConnectFour.Persistence;
using Microsoft.AspNetCore.Http.HttpResults;
using MiWrap;

namespace ConnectFour.Features;

internal record RefreshControlPanel(PlayerId PlayerId) : IHttpQuery;

public class RefreshControlPanelEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder) =>
        builder.MapGet<RefreshControlPanel, RefreshControlPanelHandler>("refresh-control-panel")
            .Produces(200)
            .Produces(400)
            .DisableAntiforgery();
}

internal class RefreshControlPanelHandler(PlayersContext ctx) : IHttpQueryHandler<RefreshControlPanel>
{
    public async Task<IResult> HandleAsync(RefreshControlPanel query, CancellationToken cancellationToken = default)
    {
        var player = ctx[query.PlayerId];

        //TODO: Check if in queue
        return player.CurrentGame is not null
            ? new RazorComponentResult(typeof(InGameButtons))
            : new RazorComponentResult(typeof(NewGameButtons));
    }
}