namespace ConnectFour.Models;

public readonly record struct ConnectionId(string Value) : IParsable<ConnectionId>
{
    public override string ToString() => Value;
    public static ConnectionId Parse(string s, IFormatProvider? provider) => new(s);
    public static bool TryParse(string? s, IFormatProvider? provider, out ConnectionId result)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            result = default;
            return false;
        }
        try
        {
            result = new ConnectionId(s);
            return true;
        }
        catch (Exception)
        {
            result = default;
            return false;
        }
    }
    public static implicit operator string(ConnectionId d) => d.Value;
}