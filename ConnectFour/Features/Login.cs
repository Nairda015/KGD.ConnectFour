using System.Security.Claims;
using ConnectFour.Models;
using Microsoft.AspNetCore.Authentication;
using MiWrap;

namespace ConnectFour.Features;

internal record Login : IHttpQuery;
public class LoginEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder) =>
        builder.MapGet<Login, LoginHandler>("login")
            .Produces(302)
            .DisableAntiforgery();
}

internal class LoginHandler(IHttpContextAccessor httpContextAccessor) : IHttpQueryHandler<Login>
{
    public async Task<IResult> HandleAsync(Login command, CancellationToken cancellationToken = default)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (!httpContext!.User.Identity!.IsAuthenticated)
        {
            var identity = new ClaimsIdentity(new[] { new Claim("id", PlayerId.Create()) }, "xd");
            await httpContext.SignInAsync("xd", new ClaimsPrincipal(identity));
        }
    
        return Results.Redirect("/");
    }
}