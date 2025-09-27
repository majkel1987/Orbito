namespace Orbito.Domain.ValueObjects
{
    public sealed class TransactionReference : IEquatable<TransactionReference>
    {
        private const int MaxLength = 255;

        public string Value { get; }

        private TransactionReference(string value)
        {
            Value = value;
        }

        public static TransactionReference Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Transaction reference cannot be null or empty.", nameof(value));

            if (value.Length > MaxLength)
                throw new ArgumentException($"Transaction reference cannot exceed {MaxLength} characters.", nameof(value));

            return new TransactionReference(value);
        }

        public static TransactionReference Generate()
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var uniqueId = Guid.NewGuid().ToString("N")[..8];
            return new TransactionReference($"TXN_{timestamp}_{uniqueId}");
        }

        public static TransactionReference GenerateWithPrefix(string prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix))
                throw new ArgumentException("Prefix cannot be null or empty.", nameof(prefix));

            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var uniqueId = Guid.NewGuid().ToString("N")[..8];
            return new TransactionReference($"{prefix}_{timestamp}_{uniqueId}");
        }

        public bool Equals(TransactionReference? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Value == other.Value;
        }

        public override bool Equals(object? obj) => obj is TransactionReference other && Equals(other);

        public override int GetHashCode() => Value.GetHashCode();

        public override string ToString() => Value;

        public static implicit operator string(TransactionReference reference) => reference.Value;

        public static bool operator ==(TransactionReference? left, TransactionReference? right)
        {
            if (left is null) return right is null;
            return left.Equals(right);
        }

        public static bool operator !=(TransactionReference? left, TransactionReference? right)
        {
            return !(left == right);
        }
    }
}