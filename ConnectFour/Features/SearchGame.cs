using System.Security.Claims;
using ConnectFour.Components.Shared.Board;
using ConnectFour.Extensions;
using ConnectFour.Models;
using ConnectFour.Persistence;
using Microsoft.AspNetCore.Http.HttpResults;
using MiWrap;

namespace ConnectFour.Features;

internal record SearchGame(GameId GameId) : IHttpQuery;

public class SearchGameEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder) =>
        builder.MapGet<SearchGame, SearchGameHandler>("game/{gameId}")
            .Produces(200)
            .Produces(400)
            .DisableAntiforgery();
}

internal class SearchGameHandler(
    ClaimsPrincipal user,
    GamesContext gamesContext) : IHttpQueryHandler<SearchGame>
{
    public async Task<IResult> HandleAsync(SearchGame query, CancellationToken cancellationToken = default)
    {
        var gameLog = gamesContext.MaybeGetState(query.GameId);
        if (gameLog is null) return Results.Redirect("/");
        var playerId = user.GetPlayerId();
        
        if (gameLog.IsComplete || !gameLog.IsPlayerInTheGame(playerId))
            return Results.Redirect($"/spectator-game/{query.GameId}");
        
        var board = new Dictionary<string, object?>
        {
            { "Board", Domain.Game.Board.LoadBoardFromLog(gameLog) }
        };
        
        return new RazorComponentResult(typeof(ActiveBoard), board);
    }
}