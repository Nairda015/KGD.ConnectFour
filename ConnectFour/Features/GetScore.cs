using ConnectFour.Extensions;
using ConnectFour.Models;
using ConnectFour.Persistence;
using MiWrap;

namespace ConnectFour.Features;

internal record GetScore(PlayerId PlayerId) : IHttpCommand;
public class GetScoreEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder) =>
        builder.MapFormPost<GetScore, GetScoreHandler>("score")
            .Produces(200)
            .Produces(400)
            .DisableAntiforgery();
}

internal class GetScoreHandler(PlayersContext ctx) : IHttpCommandHandler<GetScore>
{
    public async Task<IResult> HandleAsync(GetScore command, CancellationToken cancellationToken = default)
        => Results.Extensions.Html(ctx.GetPlayerScore(command.PlayerId).ToString());
}