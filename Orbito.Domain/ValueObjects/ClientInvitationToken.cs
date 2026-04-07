using System.Security.Cryptography;

namespace Orbito.Domain.ValueObjects;

public sealed class ClientInvitationToken : IEquatable<ClientInvitationToken>
{
    public string Token { get; }
    public DateTime ExpiresAt { get; }

    public bool IsExpired => DateTime.UtcNow > ExpiresAt;

    // Parameterless constructor for EF Core
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor
    private ClientInvitationToken() { }
#pragma warning restore CS8618

    private ClientInvitationToken(string token, DateTime expiresAt)
    {
        Token = token;
        ExpiresAt = expiresAt;
    }

    public static ClientInvitationToken Create(TimeSpan validFor)
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');

        return new ClientInvitationToken(token, DateTime.UtcNow.Add(validFor));
    }

    public bool Equals(ClientInvitationToken? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Token == other.Token;
    }

    public override bool Equals(object? obj) => obj is ClientInvitationToken other && Equals(other);
    public override int GetHashCode() => Token.GetHashCode();

    public static bool operator ==(ClientInvitationToken? left, ClientInvitationToken? right) => Equals(left, right);
    public static bool operator !=(ClientInvitationToken? left, ClientInvitationToken? right) => !Equals(left, right);

    public override string ToString() => Token;
}
