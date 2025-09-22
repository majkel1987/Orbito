using System.Text.Json;

namespace Orbito.Domain.ValueObjects
{
    public sealed class PlanLimitations : IEquatable<PlanLimitations>
    {
        private readonly List<Limitation> _limitations;

        public IReadOnlyList<Limitation> Limitations => _limitations.AsReadOnly();

        private PlanLimitations(List<Limitation> limitations)
        {
            _limitations = limitations ?? throw new ArgumentNullException(nameof(limitations));
        }

        public static PlanLimitations Create(List<Limitation> limitations)
        {
            return new PlanLimitations(limitations);
        }

        public static PlanLimitations CreateFromJson(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new PlanLimitations(new List<Limitation>());

            try
            {
                var limitations = JsonSerializer.Deserialize<List<Limitation>>(json);
                return new PlanLimitations(limitations ?? new List<Limitation>());
            }
            catch (JsonException)
            {
                return new PlanLimitations(new List<Limitation>());
            }
        }

        public string ToJson()
        {
            return JsonSerializer.Serialize(_limitations);
        }

        public void AddLimitation(Limitation limitation)
        {
            if (limitation == null)
                throw new ArgumentNullException(nameof(limitation));

            _limitations.Add(limitation);
        }

        public void RemoveLimitation(string limitationName)
        {
            var limitation = _limitations.FirstOrDefault(l => l.Name.Equals(limitationName, StringComparison.OrdinalIgnoreCase));
            if (limitation != null)
            {
                _limitations.Remove(limitation);
            }
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
        public override int GetHashCode() => _limitations.GetHashCode();
        public override string ToString() => $"Limitations: {_limitations.Count} items";
    }

    public sealed class Limitation : IEquatable<Limitation>
    {
        public string Name { get; }
        public string? Description { get; }
        public int? NumericValue { get; }
        public string? StringValue { get; }
        public LimitationType Type { get; }

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

    public enum LimitationType
    {
        Numeric = 1,
        String = 2,
        Boolean = 3
    }
}
