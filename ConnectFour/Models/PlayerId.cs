using System.Globalization;
using Bogus;

namespace ConnectFour.Models;

public readonly record struct PlayerId(string Value) : IParsable<PlayerId>
{
    private static readonly Faker Faker = new();
    public override string ToString() => Value;
    public static PlayerId Create() => new(Faker.Internet.UserName());
    
    public static PlayerId Parse(string s, IFormatProvider? provider) => new(s);
    public static bool TryParse(string? s, IFormatProvider? provider, out PlayerId result)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            result = default;
            return false;
        }
        try
        {
            result = new PlayerId(s);
            return true;
        }
        catch (Exception)
        {
            result = default;
            return false;
        }
    }
    public static implicit operator string(PlayerId d) => d.Value;
    public static implicit operator PlayerId(string d) => Parse(d, CultureInfo.InvariantCulture);
}