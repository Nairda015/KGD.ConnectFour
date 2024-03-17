using System.Security.Claims;
using ConnectFour.Extensions;
using ConnectFour.Hubs;
using ConnectFour.Persistence;
using MiWrap;

namespace ConnectFour.Features;

internal record Resign : IHttpCommand;

public class ResignEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder) =>
        builder.MapFormPost<Resign, ResignHandler>("resign")
            .Produces(200)
            .Produces(400)
            .DisableAntiforgery();
}

internal class ResignHandler(GamesContext gamesContext, GameHub hub, PlayersContext players, ClaimsPrincipal user) : IHttpCommandHandler<Resign>
{
    public async Task<IResult> HandleAsync(Resign _, CancellationToken cancellationToken = default)
    {
        var playerId = user.GetPlayerId();
        var gameLog = gamesContext.GetPlayerGame(playerId);
        if (gameLog is null) return Results.BadRequest("You are not active player of any game");

        if (gameLog.IsComplete) return Results.BadRequest("Game is already completed");
        if (gameLog.FirstPlayerConnection.PlayerId != playerId && gameLog.SecondPlayerConnection.PlayerId != playerId)
            return Results.BadRequest("You are not allowed to resign!");

        gameLog.Complete();
        gamesContext.UpdateState(gameLog);

        var winner = gameLog.FirstPlayerConnection.PlayerId == playerId
            ? gameLog.SecondPlayerConnection.PlayerId
            : gameLog.FirstPlayerConnection.PlayerId;
        
        await players.GameEnded(winner, playerId);
        
        await hub.SendResignationMessage(gameLog.GameId, winner);

        return Results.Accepted();
    }
}