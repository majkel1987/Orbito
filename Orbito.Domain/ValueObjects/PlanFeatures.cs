using System.Text.Json;

namespace Orbito.Domain.ValueObjects
{
    /// <summary>
    /// Immutable Value Object representing plan features collection.
    /// Use WithFeature/WithoutFeature methods to create new instances with modifications.
    /// </summary>
    public sealed class PlanFeatures : IEquatable<PlanFeatures>
    {
        private readonly IReadOnlyList<Feature> _features;

        public IReadOnlyList<Feature> Features => _features;

        private PlanFeatures(IReadOnlyList<Feature> features)
        {
            _features = features ?? throw new ArgumentNullException(nameof(features));
        }

        public static PlanFeatures Create(IEnumerable<Feature> features)
        {
            return new PlanFeatures(features?.ToList().AsReadOnly() ?? new List<Feature>().AsReadOnly());
        }

        public static PlanFeatures Empty => new(Array.Empty<Feature>());

        public static PlanFeatures CreateFromJson(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return Empty;

            try
            {
                var features = JsonSerializer.Deserialize<List<Feature>>(json);
                return new PlanFeatures((features ?? new List<Feature>()).AsReadOnly());
            }
            catch (JsonException)
            {
                return Empty;
            }
        }

        public string ToJson()
        {
            return JsonSerializer.Serialize(_features);
        }

        /// <summary>
        /// Creates a new PlanFeatures instance with the specified feature added.
        /// </summary>
        public PlanFeatures WithFeature(Feature feature)
        {
            if (feature == null)
                throw new ArgumentNullException(nameof(feature));

            var newFeatures = _features.ToList();
            newFeatures.Add(feature);
            return new PlanFeatures(newFeatures.AsReadOnly());
        }

        /// <summary>
        /// Creates a new PlanFeatures instance with the specified feature removed.
        /// </summary>
        public PlanFeatures WithoutFeature(string featureName)
        {
            var newFeatures = _features
                .Where(f => !f.Name.Equals(featureName, StringComparison.OrdinalIgnoreCase))
                .ToList();
            return new PlanFeatures(newFeatures.AsReadOnly());
        }

        public bool HasFeature(string featureName)
        {
            return _features.Any(f => f.Name.Equals(featureName, StringComparison.OrdinalIgnoreCase));
        }

        public Feature? GetFeature(string featureName)
        {
            return _features.FirstOrDefault(f => f.Name.Equals(featureName, StringComparison.OrdinalIgnoreCase));
        }

        public bool Equals(PlanFeatures? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            return _features.Count == other._features.Count &&
                   _features.All(f => other._features.Contains(f));
        }

        public override bool Equals(object? obj) => obj is PlanFeatures other && Equals(other);

        public override int GetHashCode()
        {
            var hash = new HashCode();
            foreach (var feature in _features.OrderBy(f => f.Name))
            {
                hash.Add(feature);
            }
            return hash.ToHashCode();
        }

        public override string ToString() => $"Features: {_features.Count} items";

        public static bool operator ==(PlanFeatures? left, PlanFeatures? right)
        {
            if (left is null) return right is null;
            return left.Equals(right);
        }

        public static bool operator !=(PlanFeatures? left, PlanFeatures? right) => !(left == right);
    }

    /// <summary>
    /// Immutable Value Object representing a single plan feature.
    /// </summary>
    public sealed class Feature : IEquatable<Feature>
    {
        public string Name { get; }
        public string? Description { get; }
        public string? Value { get; }
        public bool IsEnabled { get; }

        // Parameterless constructor for JSON deserialization
        private Feature()
        {
            Name = string.Empty;
        }

        public Feature(string name, string? description = null, string? value = null, bool isEnabled = true)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description;
            Value = value;
            IsEnabled = isEnabled;
        }

        public bool Equals(Feature? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            
            return Name == other.Name &&
                   Description == other.Description &&
                   Value == other.Value &&
                   IsEnabled == other.IsEnabled;
        }

        public override bool Equals(object? obj) => obj is Feature other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Name, Description, Value, IsEnabled);
        public override string ToString() => $"{Name}: {Value ?? (IsEnabled ? "Enabled" : "Disabled")}";
    }
}
