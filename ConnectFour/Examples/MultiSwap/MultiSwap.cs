using ConnectFour.Extensions;
using MiWrap;

namespace ConnectFour.Examples.MultiSwap;

internal record MultiSwapCommand : IHttpCommand;

public class NewMessageEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder) =>
        builder.MapFormPost<MultiSwapCommand, MultiSwapHandler>("test-multi")
            .Produces(200)
            .Produces(400)
            .DisableAntiforgery();
}

internal class MultiSwapHandler(BlazorRenderer renderer) : IHttpCommandHandler<MultiSwapCommand>
{
    public async Task<IResult> HandleAsync(MultiSwapCommand command, CancellationToken cancellationToken = default)
    {
        var result = await renderer.RenderComponent<MultiSwap>();
        return Results.Extensions.Html(result);
    }
}