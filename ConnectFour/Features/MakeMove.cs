using ConnectFour.Domain;
using ConnectFour.Extensions;
using ConnectFour.Hubs;
using ConnectFour.Persistence;
using MiWrap;

namespace ConnectFour.Features;

internal record MakeMove(PlayerId PlayerId, GameId GameId, int ColumnNumber) : IHttpCommand;

public class SyntaxTestEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder) =>
        builder.MapFormPost<MakeMove, MakeMoveHandler>("move")
            .Produces(200)
            .Produces(400)
            .DisableAntiforgery();
}

internal class MakeMoveHandler(InMemoryGamesState gamesState, GameHub hub) : IHttpCommandHandler<MakeMove>
{
    public async Task<IResult> HandleAsync(MakeMove command, CancellationToken cancellationToken = default)
    {
        var (playerId, gameId, chosenColumn) = command;
        var gameLog = gamesState.GetState(gameId);

        if (gameLog.IsComplete) return Results.BadRequest("Game is already completed");
        if (gameLog.GetCurrentPlayerId != playerId) return Results.BadRequest("It's not your turn");

        var game = new Game(gameLog);
        var move = game.MakeMove(chosenColumn);

        if (move.MoveResult is MoveResult.ColumnFull) return Results.BadRequest("Column is full");

        gameLog.AddMove(chosenColumn);
        if (move.MoveResult is MoveResult.Win) gameLog.Complete();

        gamesState.UpdateState(gameLog);

        await hub.MarkMove(gameLog, move.Position!.Value, cancellationToken);
        if (gameLog.IsComplete) await hub.SendCompletedGameMessage(gameId, playerId);

        return Results.Accepted();
    }
}