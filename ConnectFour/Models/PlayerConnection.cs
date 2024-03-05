namespace ConnectFour.Models;

public record PlayerConnection(PlayerId PlayerId, ConnectionId Connection)
{
    public DateTime ConnectionTime { get; init; } = DateTime.Now;
    public virtual bool Equals(PlayerConnection? other)
    {
        return PlayerId.Equals(other?.PlayerId) && Connection.Equals(other.Connection);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(PlayerId, Connection);
    }
}