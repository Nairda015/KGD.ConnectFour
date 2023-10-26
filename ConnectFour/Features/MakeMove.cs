using ConnectFour.Components.Shared;
using ConnectFour.Extensions;
using ConnectFour.Persistance;
using Microsoft.AspNetCore.Http.HttpResults;
using MiWrap;

namespace ConnectFour.Features;

internal record MakeMove(int GameId, int ColumnNumber) : IHttpCommand;

public class SyntaxTestEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder) =>
        builder.MapFormPost<MakeMove, MakeMoveHandler>("move")
            .Produces(200)
            .Produces(400)
            .DisableAntiforgery();
}

internal class MakeMoveHandler(InMemoryGamesState gamesState) : IHttpCommandHandler<MakeMove>
{
    public async Task<IResult> HandleAsync(MakeMove command, CancellationToken cancellationToken = default)
    {
        await Task.Delay(10, cancellationToken);
        Console.WriteLine(command);
        var state = gamesState.GetState(command.GameId);
        gamesState.UpdateState(command.GameId);
        return new RazorComponentResult(typeof(Disc), new { Colour = state % 2 == 0 ? "red" : "blue" });
    }
}