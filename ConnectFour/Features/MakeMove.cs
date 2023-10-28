using ConnectFour.Domain;
using ConnectFour.Extensions;
using ConnectFour.Hubs;
using ConnectFour.Persistence;
using MiWrap;

namespace ConnectFour.Features;

internal record MakeMove(PlayerId PlayerId, int ChosenColumn) : IHttpCommand;

public class SyntaxTestEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder) =>
        builder.MapFormPost<MakeMove, MakeMoveHandler>("move")
            .Produces(200)
            .Produces(400)
            .DisableAntiforgery();
}

internal class MakeMoveHandler(InMemoryGamesState gamesState, GameHub hub, IHttpContextAccessor context) : IHttpCommandHandler<MakeMove>
{
    public async Task<IResult> HandleAsync(MakeMove command, CancellationToken cancellationToken = default)
    {
        var (playerId, chosenColumn) = command;
        var path = context.HttpContext?.Request.Path;
        var gameId = new GameId(path!);
        var gameLog = gamesState.GetState(gameId);

        if (gameLog.IsComplete) return Results.BadRequest("Game is already completed");
        if (gameLog.CurrentPlayerId != playerId) return Results.BadRequest("It's not your turn");

        var game = new Game(gameLog);
        var moveResult = game.MakeMove(chosenColumn);

        if (moveResult is MoveResult.ColumnFull) return Results.BadRequest("Column is full");

        gameLog.AddMove(chosenColumn);
        if (moveResult is MoveResult.Win) gameLog.Complete();

        gamesState.UpdateState(gameLog);

        if (gameLog.IsComplete) await hub.SendCompletedGameMessage(gameId, playerId);

        return Results.Accepted();
    }
}