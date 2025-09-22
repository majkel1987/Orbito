using System.Text.Json;

namespace Orbito.Domain.ValueObjects
{
    public sealed class PlanFeatures : IEquatable<PlanFeatures>
    {
        private readonly List<Feature> _features;

        public IReadOnlyList<Feature> Features => _features.AsReadOnly();

        private PlanFeatures(List<Feature> features)
        {
            _features = features ?? throw new ArgumentNullException(nameof(features));
        }

        public static PlanFeatures Create(List<Feature> features)
        {
            return new PlanFeatures(features);
        }

        public static PlanFeatures CreateFromJson(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new PlanFeatures(new List<Feature>());

            try
            {
                var features = JsonSerializer.Deserialize<List<Feature>>(json);
                return new PlanFeatures(features ?? new List<Feature>());
            }
            catch (JsonException)
            {
                return new PlanFeatures(new List<Feature>());
            }
        }

        public string ToJson()
        {
            return JsonSerializer.Serialize(_features);
        }

        public void AddFeature(Feature feature)
        {
            if (feature == null)
                throw new ArgumentNullException(nameof(feature));

            _features.Add(feature);
        }

        public void RemoveFeature(string featureName)
        {
            var feature = _features.FirstOrDefault(f => f.Name.Equals(featureName, StringComparison.OrdinalIgnoreCase));
            if (feature != null)
            {
                _features.Remove(feature);
            }
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
        public override int GetHashCode() => _features.GetHashCode();
        public override string ToString() => $"Features: {_features.Count} items";
    }

    public sealed class Feature : IEquatable<Feature>
    {
        public string Name { get; }
        public string? Description { get; }
        public string? Value { get; }
        public bool IsEnabled { get; }

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
