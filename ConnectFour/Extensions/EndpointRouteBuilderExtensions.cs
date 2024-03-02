using Microsoft.AspNetCore.Mvc;
using MiWrap;

namespace ConnectFour.Extensions;

public static class EndpointRouteBuilderExtensions
{
    public static RouteHandlerBuilder MapFormPost<TCommand, THandler>(this IEndpointRouteBuilder endpoints, string template) 
        where TCommand : IHttpCommand
        where THandler : IHttpCommandHandler<TCommand> =>
        endpoints.MapPost(template, async (
                    THandler handler,
                    [FromForm] TCommand command,
                    CancellationToken cancellationToken) =>
                await handler.HandleAsync(command, cancellationToken))
            .WithName(typeof(TCommand).FullName!);
    
    public static RouteHandlerBuilder MapFormPut<TCommand, THandler>(this IEndpointRouteBuilder endpoints, string template) 
        where TCommand : IHttpCommand
        where THandler : IHttpCommandHandler<TCommand> =>
        endpoints.MapPut(template, async (
                    THandler handler,
                    [FromForm] TCommand command,
                    CancellationToken cancellationToken) =>
                await handler.HandleAsync(command, cancellationToken))
            .WithName(typeof(TCommand).FullName!);
    
    public static RouteHandlerBuilder MapFormDelete<TCommand, THandler>(this IEndpointRouteBuilder endpoints, string template) 
        where TCommand : IHttpCommand
        where THandler : IHttpCommandHandler<TCommand> =>
        endpoints.MapDelete(template, async (
                    THandler handler,
                    [FromForm] TCommand command,
                    CancellationToken cancellationToken) =>
                await handler.HandleAsync(command, cancellationToken))
            .WithName(typeof(TCommand).FullName!);
}