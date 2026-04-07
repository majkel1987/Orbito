using System.Text.Json;
using Orbito.Domain.Enums;

namespace Orbito.Domain.ValueObjects
{
    /// <summary>
    /// Immutable Value Object representing plan limitations collection.
    /// Use WithLimitation/WithoutLimitation methods to create new instances with modifications.
    /// </summary>
    public sealed class PlanLimitations : IEquatable<PlanLimitations>
    {
        private readonly IReadOnlyList<Limitation> _limitations;

        public IReadOnlyList<Limitation> Limitations => _limitations;

        private PlanLimitations(IReadOnlyList<Limitation> limitations)
        {
            _limitations = limitations ?? throw new ArgumentNullException(nameof(limitations));
        }

        public static PlanLimitations Create(IEnumerable<Limitation> limitations)
        {
            return new PlanLimitations(limitations?.ToList().AsReadOnly() ?? new List<Limitation>().AsReadOnly());
        }

        public static PlanLimitations Empty => new(Array.Empty<Limitation>());

        public static PlanLimitations CreateFromJson(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return Empty;

            try
            {
                var limitations = JsonSerializer.Deserialize<List<Limitation>>(json);
                return new PlanLimitations((limitations ?? new List<Limitation>()).AsReadOnly());
            }
            catch (JsonException)
            {
                return Empty;
            }
        }

        public string ToJson()
        {
            return JsonSerializer.Serialize(_limitations);
        }

        /// <summary>
        /// Creates a new PlanLimitations instance with the specified limitation added.
        /// </summary>
        public PlanLimitations WithLimitation(Limitation limitation)
        {
            if (limitation == null)
                throw new ArgumentNullException(nameof(limitation));

            var newLimitations = _limitations.ToList();
            newLimitations.Add(limitation);
            return new PlanLimitations(newLimitations.AsReadOnly());
        }

        /// <summary>
        /// Creates a new PlanLimitations instance with the specified limitation removed.
        /// </summary>
        public PlanLimitations WithoutLimitation(string limitationName)
        {
            var newLimitations = _limitations
                .Where(l => !l.Name.Equals(limitationName, StringComparison.OrdinalIgnoreCase))
                .ToList();
            return new PlanLimitations(newLimitations.AsReadOnly());
        }

        public bool HasLimitation(string limitationName)
        {
            return _limitations.Any(l => l.Name.Equals(limitationName, StringComparison.OrdinalIgnoreCase));
        }

        public Limitation? GetLimitation(string limitationName)
        {
            return _limitations.FirstOrDefault(l => l.Name.Equals(limitationName, StringComparison.OrdinalIgnoreCase));
        }

        public int? GetNumericLimit(string limitationName)
        {
            var limitation = GetLimitation(limitationName);
            return limitation?.NumericValue;
        }

        public bool Equals(PlanLimitations? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            return _limitations.Count == other._limitations.Count &&
                   _limitations.All(l => other._limitations.Contains(l));
        }

        public override bool Equals(object? obj) => obj is PlanLimitations other && Equals(other);

        public override int GetHashCode()
        {
            var hash = new HashCode();
            foreach (var limitation in _limitations.OrderBy(l => l.Name))
            {
                hash.Add(limitation);
            }
            return hash.ToHashCode();
        }

        public override string ToString() => $"Limitations: {_limitations.Count} items";

        public static bool operator ==(PlanLimitations? left, PlanLimitations? right)
        {
            if (left is null) return right is null;
            return left.Equals(right);
        }

        public static bool operator !=(PlanLimitations? left, PlanLimitations? right) => !(left == right);
    }

    /// <summary>
    /// Immutable Value Object representing a single plan limitation.
    /// </summary>
    public sealed class Limitation : IEquatable<Limitation>
    {
        public string Name { get; }
        public string? Description { get; }
        public int? NumericValue { get; }
        public string? StringValue { get; }
        public LimitationType Type { get; }

        // Parameterless constructor for JSON deserialization
        private Limitation()
        {
            Name = string.Empty;
        }

        public Limitation(string name, LimitationType type, string? description = null, int? numericValue = null, string? stringValue = null)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Type = type;
            Description = description;
            NumericValue = numericValue;
            StringValue = stringValue;
        }

        public bool Equals(Limitation? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            
            return Name == other.Name &&
                   Description == other.Description &&
                   NumericValue == other.NumericValue &&
                   StringValue == other.StringValue &&
                   Type == other.Type;
        }

        public override bool Equals(object? obj) => obj is Limitation other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Name, Description, NumericValue, StringValue, Type);
        public override string ToString() => $"{Name}: {NumericValue?.ToString() ?? StringValue ?? "Unlimited"}";
    }
}
