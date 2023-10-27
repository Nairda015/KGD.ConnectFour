using System.Collections.Concurrent;
using ConnectFour.Domain;
using ConnectFour.Extensions;
using ConnectFour.Hubs;
using ConnectFour.Persistance;
using Microsoft.AspNetCore.Http.HttpResults;
using MiWrap;

namespace ConnectFour.Features;

internal record NewGame(string UserId) : IHttpCommand;

public class NewGameEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder) =>
        builder.MapFormPost<NewGame, NewGameHandler>("new-game")
            .Produces(200)
            .Produces(400)
            .DisableAntiforgery();
}

internal class NewGameHandler(IHttpContextAccessor context, InMemoryGamesState state, GameHub hub)
    : IHttpCommandHandler<NewGame>
{
    private static readonly ConcurrentQueue<string> UsersQueue = new();

    
    
    public async Task<IResult> HandleAsync(NewGame command, CancellationToken cancellationToken = default)
    {
        var gameId = GameId.Create();
        context.HttpContext?.Response.Headers.Add("HX-Replace-Url", gameId.Value);
        if (UsersQueue.IsEmpty)
        {
            UsersQueue.Enqueue(command.UserId);
            return Results.Accepted();
        }

        if (!UsersQueue.TryDequeue(out var secondUser))
        {
            
        }

        await hub.AddToGroup(gameId, cancellationToken);

        var gameLog = new GameLog
        {
            GameId = gameId,
            FirstPlayerId = command.UserId,
            SecondPlayerId = secondUser!
        };

        state.NewGame(gameLog);
        return Results.Accepted();
    }
}

