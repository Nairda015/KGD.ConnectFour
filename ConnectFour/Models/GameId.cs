using Microsoft.IdentityModel.Tokens;

namespace ConnectFour.Models;

public readonly record struct GameId(string Value) : IParsable<GameId>
{
    public static GameId Create()
    {
        var buffer = new byte[7]; //produce 10 char long ID
        Random.Shared.NextBytes(buffer);
        var id =  Base64UrlEncoder.Encode(buffer);
        return new GameId(id);
    }
    
    public override string ToString() => Value;
    public static GameId Parse(string s, IFormatProvider? provider) => new(s);
    public static bool TryParse(string? s, IFormatProvider? provider, out GameId result)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            result = default;
            return false;
        }
        try
        {
            result = new GameId(s);
            return true;
        }
        catch (Exception)
        {
            result = default;
            return false;
        }
    }
    public static implicit operator string(GameId d) => d.Value;
}