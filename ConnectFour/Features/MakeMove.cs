using ConnectFour.Domain;
using ConnectFour.Extensions;
using ConnectFour.Hubs;
using ConnectFour.Models;
using ConnectFour.Persistence;
using MiWrap;

namespace ConnectFour.Features;

internal record MakeMove(PlayerId PlayerId, GameId GameId, int ColumnNumber) : IHttpCommand;

public class MakeMoveEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder) =>
        builder.MapFormPost<MakeMove, MakeMoveHandler>("move")
            .Produces(200)
            .Produces(400)
            .DisableAntiforgery();
}

internal class MakeMoveHandler(GamesContext gamesContext, GameHub hub, PlayersContext players) : IHttpCommandHandler<MakeMove>
{
    public async Task<IResult> HandleAsync(MakeMove command, CancellationToken cancellationToken = default)
    {
        var (playerId, gameId, chosenColumn) = command;
        var gameLog = gamesContext.GetState(gameId);

        if (gameLog.IsComplete) return Results.BadRequest("Game is already completed");
        if (gameLog.GetCurrentPlayerId != playerId) return Results.BadRequest("It's not your turn");

        var game = new Game(gameLog);
        var move = game.MakeMove(chosenColumn);

        if (move.MoveResult is MoveResult.ColumnFull) return Results.BadRequest("Column is full");

        gameLog.AddMove(chosenColumn);
        if (move.MoveResult is MoveResult.Win) gameLog.Complete();

        gamesContext.UpdateState(gameLog);

        //TODO: mark after adding move is buggy 
        //TODO: board is full scenario
        await hub.MarkMove(gameLog, move.Position!.Value, cancellationToken);
        if (gameLog.IsComplete)
        {
            var looser = gameLog.FirstPlayerConnection.PlayerId == playerId
                ? gameLog.SecondPlayerConnection.PlayerId
                : gameLog.FirstPlayerConnection.PlayerId;
            players.GameEnded(playerId, GameResult.Win);
            players.GameEnded(looser, GameResult.Lose);
            
            await hub.SendCompletedGameMessage(gameId, playerId);
        }

        return Results.Accepted();
    }
}