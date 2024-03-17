using System.Diagnostics;
using System.Security.Claims;
using ConnectFour.Models;

namespace ConnectFour.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static PlayerId GetPlayerId(this ClaimsPrincipal principal)
    {
        if (!principal.HasClaim(x => x.Type == "id")) throw new UnreachableException();

        var idClaim = principal.Claims.First(x => x.Type == "id").Value;
        return new PlayerId(idClaim);
    }
}