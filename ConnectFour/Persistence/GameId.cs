using System.Web;

namespace ConnectFour.Persistence;

public readonly record struct GameId(string Value) : IParsable<GameId>
{
    public static GameId Create() => new(HttpUtility.UrlEncode(Convert.ToBase64String(Guid.NewGuid().ToByteArray())[..^2]));
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