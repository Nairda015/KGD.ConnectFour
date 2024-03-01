using ConnectFour.Extensions;
using ConnectFour.Hubs;
using ConnectFour.Models;
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

internal class ResignHandler(GamesContext gamesContext, GameHub hub, PlayersContext players) : IHttpCommandHandler<Resign>
{
    public async Task<IResult> HandleAsync(Resign command, CancellationToken cancellationToken = default)
    {
        var (playerId, gameId) = command;
        var gameLog = gamesContext.GetState(gameId);

        if (gameLog.IsComplete) return Results.BadRequest("Game is already completed");
        if (gameLog.FirstPlayerConnection.PlayerId != playerId && gameLog.SecondPlayerConnection.PlayerId != playerId)
            return Results.BadRequest("You are not allowed to resign!");

        gameLog.Complete();
        gamesContext.UpdateState(gameLog);

        var winner = gameLog.FirstPlayerConnection.PlayerId == playerId
            ? gameLog.SecondPlayerConnection.PlayerId
            : gameLog.FirstPlayerConnection.PlayerId;
        await hub.SendResignationMessage(gameId, winner);
        
        players.GameEnded(winner, GameResult.Win);
        players.GameEnded(playerId, GameResult.Lose);

        return Results.Accepted();
    }
}