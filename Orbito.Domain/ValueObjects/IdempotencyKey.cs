namespace Orbito.Domain.ValueObjects;

/// <summary>
/// Value Object representing an idempotency key for ensuring request uniqueness
/// </summary>
public sealed class IdempotencyKey : IEquatable<IdempotencyKey>
{
    /// <summary>
    /// The idempotency key value
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// The format of the idempotency key (guid or custom)
    /// </summary>
    public string Format { get; }

    // Parameterless constructor for EF Core
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor
    private IdempotencyKey() { }
#pragma warning restore CS8618

    private IdempotencyKey(string value, string format)
    {
        Value = value;
        Format = format;
    }

    /// <summary>
    /// Creates a new IdempotencyKey from a string value
    /// </summary>
    /// <param name="key">The idempotency key string</param>
    /// <returns>IdempotencyKey instance</returns>
    /// <exception cref="ArgumentException">Thrown when key is invalid</exception>
    public static IdempotencyKey Create(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Idempotency key cannot be null or empty", nameof(key));

        var trimmedKey = key.Trim();

        // Validate length
        if (trimmedKey.Length > 100)
            throw new ArgumentException("Idempotency key cannot exceed 100 characters", nameof(key));

        // Determine format and validate
        string format;
        if (IsValidGuid(trimmedKey))
        {
            format = "guid";
        }
        else if (IsValidCustomKey(trimmedKey))
        {
            format = "custom";
        }
        else
        {
            throw new ArgumentException(
                "Idempotency key must be a valid GUID or contain only alphanumeric characters, hyphens, and underscores",
                nameof(key));
        }

        return new IdempotencyKey(trimmedKey, format);
    }

    /// <summary>
    /// Creates a new IdempotencyKey from a GUID
    /// </summary>
    /// <param name="guid">The GUID value</param>
    /// <returns>IdempotencyKey instance</returns>
    public static IdempotencyKey Create(Guid guid)
    {
        if (guid == Guid.Empty)
            throw new ArgumentException("GUID cannot be empty", nameof(guid));

        return new IdempotencyKey(guid.ToString(), "guid");
    }

    /// <summary>
    /// Generates a new random IdempotencyKey using GUID
    /// </summary>
    /// <returns>New IdempotencyKey instance</returns>
    public static IdempotencyKey New() => Create(Guid.NewGuid());

    /// <summary>
    /// Checks if the key is in GUID format
    /// </summary>
    public bool IsGuid => Format == "guid";

    /// <summary>
    /// Checks if the key is in custom format
    /// </summary>
    public bool IsCustom => Format == "custom";

    /// <summary>
    /// Gets the key as GUID if it's in GUID format
    /// </summary>
    /// <returns>GUID representation or null if not GUID format</returns>
    public Guid? AsGuid()
    {
        return IsGuid && Guid.TryParse(Value, out var guid) ? guid : null;
    }

    /// <summary>
    /// Validates if a string is a valid GUID
    /// </summary>
    private static bool IsValidGuid(string key)
    {
        return Guid.TryParse(key, out _);
    }

    /// <summary>
    /// Validates if a string is a valid custom key (alphanumeric, hyphens, underscores only)
    /// </summary>
    private static bool IsValidCustomKey(string key)
    {
        // Allow alphanumeric characters, hyphens, and underscores
        // Must be at least 1 character and not exceed 100 characters
        return key.Length >= 1 && 
               key.Length <= 100 && 
               key.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '_');
    }

    public static implicit operator string(IdempotencyKey idempotencyKey) => idempotencyKey.Value;
    public static explicit operator IdempotencyKey(string key) => Create(key);

    public bool Equals(IdempotencyKey? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Value == other.Value && Format == other.Format;
    }

    public override bool Equals(object? obj)
    {
        return obj is IdempotencyKey other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Value, Format);
    }

    public static bool operator ==(IdempotencyKey? left, IdempotencyKey? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(IdempotencyKey? left, IdempotencyKey? right)
    {
        return !Equals(left, right);
    }

    public override string ToString()
    {
        return Value;
    }
}
