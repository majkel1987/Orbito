namespace Orbito.Domain.ValueObjects
{
    public sealed class TenantId : IEquatable<TenantId>
    {
        public Guid Value { get; }

        // Parameterless constructor for EF Core
        private TenantId()
        {
            Value = Guid.Empty;
        }

        private TenantId(Guid value)
        {
            Value = value;
        }

        /// <summary>
        /// Creates a TenantId from a GUID value.
        /// Use TenantId.Empty for system-wide entities without tenant context.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when value is Guid.Empty. Use TenantId.Empty instead.</exception>
        public static TenantId Create(Guid value)
        {
            if (value == Guid.Empty)
                throw new ArgumentException("TenantId cannot be empty. Use TenantId.Empty for system-wide entities.", nameof(value));

            return new TenantId(value);
        }

        /// <summary>
        /// Creates a TenantId from a GUID, allowing empty GUID for system entities.
        /// Prefer using TenantId.Empty directly for clarity.
        /// </summary>
        public static TenantId CreateAllowingEmpty(Guid value)
        {
            return new TenantId(value);
        }

        public static TenantId New() => new(Guid.NewGuid());

        /// <summary>
        /// Gets an empty TenantId (for system-wide entities without tenant context)
        /// </summary>
        public static TenantId Empty => new(Guid.Empty);

        public static implicit operator Guid(TenantId tenantId) => tenantId.Value;
        public static explicit operator TenantId(Guid guid) => Create(guid);

        public bool Equals(TenantId? other) => other is not null && Value.Equals(other.Value);
        public override bool Equals(object? obj) => obj is TenantId other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString();

        // Equality operators for proper value comparison
        public static bool operator ==(TenantId? left, TenantId? right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left is null || right is null) return false;
            return left.Equals(right);
        }

        public static bool operator !=(TenantId? left, TenantId? right) => !(left == right);
    }
}
