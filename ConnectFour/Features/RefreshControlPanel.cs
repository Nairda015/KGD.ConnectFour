using System.Security.Claims;
using ConnectFour.Components.Shared.Game;
using ConnectFour.Extensions;
using ConnectFour.Hubs;
using ConnectFour.Models;
using ConnectFour.Persistence;
using Microsoft.AspNetCore.Http.HttpResults;
using MiWrap;

namespace ConnectFour.Features;

internal record RefreshControlPanel : IHttpQuery;

public class RefreshControlPanelEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder) =>
        builder.MapGet<RefreshControlPanel, RefreshControlPanelHandler>("refresh-control-panel")
            .Produces(200)
            .Produces(400)
            .DisableAntiforgery();
}

internal class RefreshControlPanelHandler(PlayersContext ctx, GameHub gameHub, ClaimsPrincipal user) : IHttpQueryHandler<RefreshControlPanel>
{
    public async Task<IResult> HandleAsync(RefreshControlPanel _, CancellationToken cancellationToken = default)
    {
        var player = ctx[user.GetPlayerId()];
        
        if (gameHub.CheckIfInTheQueue(player.Id)) return new RazorComponentResult(typeof(Indicator));

        return player.CurrentGame is not null
            ? new RazorComponentResult(typeof(InGameButtons))
            : new RazorComponentResult(typeof(NewGameButtons));
    }
}