namespace Orbito.Domain.ValueObjects
{
    public sealed class TenantId : IEquatable<TenantId>
    {
        public Guid Value { get; }

        private TenantId(Guid value)
        {
            Value = value;
        }

        public static TenantId Create(Guid value)
        {
            if (value == Guid.Empty)
                throw new ArgumentException("TenantId cannot be empty", nameof(value));

            return new TenantId(value);
        }

        public static TenantId New() => new(Guid.NewGuid());

        public static implicit operator Guid(TenantId tenantId) => tenantId.Value;
        public static explicit operator TenantId(Guid guid) => Create(guid);

        public bool Equals(TenantId? other) => other is not null && Value.Equals(other.Value);
        public override bool Equals(object? obj) => obj is TenantId other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString();
    }
}
