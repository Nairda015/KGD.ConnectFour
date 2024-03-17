using System.Security.Claims;
using ConnectFour.Extensions;
using ConnectFour.Persistence;
using MiWrap;

namespace ConnectFour.Features;

internal record GetScore: IHttpCommand;
public class GetScoreEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder) =>
        builder.MapFormPost<GetScore, GetScoreHandler>("score")
            .Produces(200)
            .Produces(400)
            .DisableAntiforgery();
}

internal class GetScoreHandler(PlayersContext ctx, ClaimsPrincipal user) : IHttpCommandHandler<GetScore>
{
    public async Task<IResult> HandleAsync(GetScore _, CancellationToken cancellationToken = default)
        => Results.Extensions.Html(ctx.GetPlayerScore(user.GetPlayerId()).ToString());
}