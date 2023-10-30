using ConnectFour.Extensions;
using ConnectFour.Hubs;
using ConnectFour.Persistence;
using MiWrap;

namespace ConnectFour.Features;

internal record Resign(PlayerId PlayerId, GameId GameId) : IHttpCommand;

public class ResignEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder) =>
        builder.MapFormPost<Resign, ResignHandler>("resign")
            .Produces(200)
            .Produces(400)
            .DisableAntiforgery();
}

internal class ResignHandler(InMemoryGamesState gamesState, GameHub hub) : IHttpCommandHandler<Resign>
{
    public async Task<IResult> HandleAsync(Resign command, CancellationToken cancellationToken = default)
    {
        var (playerId, gameId) = command;
        var gameLog = gamesState.GetState(gameId);

        if (gameLog.IsComplete) return Results.BadRequest("Game is already completed");
        if (gameLog.FirstPlayer.PlayerId != playerId && gameLog.SecondPlayer.PlayerId != playerId)
            return Results.BadRequest("You are not allowed to resign!");

        gameLog.Complete();
        gamesState.UpdateState(gameLog);

        var winner = gameLog.FirstPlayer.PlayerId == playerId
            ? gameLog.SecondPlayer.PlayerId
            : gameLog.FirstPlayer.PlayerId;
        await hub.SendCompletedGameMessage(gameId, winner);

        return Results.Accepted();
    }
}