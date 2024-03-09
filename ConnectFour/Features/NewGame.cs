using ConnectFour.Domain;
using ConnectFour.Extensions;
using ConnectFour.Hubs;
using ConnectFour.Models;
using ConnectFour.Persistence;
using MiWrap;

namespace ConnectFour.Features;

internal record NewGame(PlayerId PlayerId) : IHttpCommand;

public class NewGameEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder) =>
        builder.MapFormPost<NewGame, NewGameHandler>("new-game")
            .Produces(200)
            .Produces(400)
            .DisableAntiforgery();
}

internal class NewGameHandler(
    IHttpContextAccessor httpContextAccessor,
    GameHub hub,
    GamesContext gamesContext,
    PlayersContext playersContext) : IHttpCommandHandler<NewGame>
{
    public async Task<IResult> HandleAsync(NewGame command, CancellationToken cancellationToken = default)
    {
        var firstPlayerConnection = GameHub.GetPlayerConnection(command.PlayerId);
        var secondPlayerConnection = GameHub.FindOpponent();
        
        if (secondPlayerConnection is null)
        {
            await hub.AddPlayerToQueue(firstPlayerConnection);
            httpContextAccessor.HttpContext!.Response.Headers.Append("HX-Push-Url", $"game/{firstPlayerConnection.GameId}");
            return Results.Accepted();
        }

        var gameId = secondPlayerConnection.GameId;
        
        var log = new GameLog
        {
            GameId = gameId,
            FirstPlayerConnection = firstPlayerConnection,
            SecondPlayerConnection = secondPlayerConnection,
        };
        
        gamesContext.StartGameRecording(log);

        await playersContext.GameStarted(log);
        
        await hub.AddPlayersToGroup(log, cancellationToken);
        await hub.NotifyAboutGameStart(gameId, cancellationToken);
        
        httpContextAccessor.HttpContext!.Response.Headers.Append("HX-Push-Url", $"game/{gameId}");
        return Results.Accepted();
    }
}