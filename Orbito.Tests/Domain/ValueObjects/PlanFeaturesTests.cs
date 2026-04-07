using FluentAssertions;
using Orbito.Domain.ValueObjects;
using Xunit;

namespace Orbito.Tests.Domain.ValueObjects
{
    public class PlanFeaturesTests
    {
        [Fact]
        public void Create_WithValidFeatures_ShouldCreatePlanFeatures()
        {
            // Arrange
            var features = new List<Feature>
            {
                new Feature("Feature1", "Description1", "Value1", true),
                new Feature("Feature2", "Description2", "Value2", false)
            };

            // Act
            var planFeatures = PlanFeatures.Create(features);

            // Assert
            planFeatures.Should().NotBeNull();
            planFeatures.Features.Should().HaveCount(2);
            planFeatures.Features.Should().Contain(features);
        }

        [Fact]
        public void Create_WithEmptyList_ShouldCreateEmptyPlanFeatures()
        {
            // Arrange
            var features = new List<Feature>();

            // Act
            var planFeatures = PlanFeatures.Create(features);

            // Assert
            planFeatures.Should().NotBeNull();
            planFeatures.Features.Should().BeEmpty();
        }

        [Fact]
        public void Create_WithNullFeatures_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => PlanFeatures.Create(null!));
            exception.ParamName.Should().Be("features");
        }

        [Fact]
        public void CreateFromJson_WithValidJson_ShouldCreatePlanFeatures()
        {
            // Arrange
            var json = @"[
                {
                    ""name"": ""Feature1"",
                    ""description"": ""Description1"",
                    ""value"": ""Value1"",
                    ""isEnabled"": true
                },
                {
                    ""name"": ""Feature2"",
                    ""description"": ""Description2"",
                    ""value"": ""Value2"",
                    ""isEnabled"": false
                }
            ]";

            // Act
            var planFeatures = PlanFeatures.CreateFromJson(json);

            // Assert
            planFeatures.Should().NotBeNull();
            planFeatures.Features.Should().HaveCount(2);
            planFeatures.Features[0].Name.Should().Be("Feature1");
            planFeatures.Features[0].Description.Should().Be("Description1");
            planFeatures.Features[0].Value.Should().Be("Value1");
            planFeatures.Features[0].IsEnabled.Should().BeTrue();
            planFeatures.Features[1].Name.Should().Be("Feature2");
            planFeatures.Features[1].Description.Should().Be("Description2");
            planFeatures.Features[1].Value.Should().Be("Value2");
            planFeatures.Features[1].IsEnabled.Should().BeFalse();
        }

        [Fact]
        public void CreateFromJson_WithEmptyJson_ShouldCreateEmptyPlanFeatures()
        {
            // Arrange
            var json = "[]";

            // Act
            var planFeatures = PlanFeatures.CreateFromJson(json);

            // Assert
            planFeatures.Should().NotBeNull();
            planFeatures.Features.Should().BeEmpty();
        }

        [Fact]
        public void CreateFromJson_WithNullJson_ShouldCreateEmptyPlanFeatures()
        {
            // Act
            var planFeatures = PlanFeatures.CreateFromJson(null);

            // Assert
            planFeatures.Should().NotBeNull();
            planFeatures.Features.Should().BeEmpty();
        }

        [Fact]
        public void CreateFromJson_WithEmptyString_ShouldCreateEmptyPlanFeatures()
        {
            // Act
            var planFeatures = PlanFeatures.CreateFromJson("");

            // Assert
            planFeatures.Should().NotBeNull();
            planFeatures.Features.Should().BeEmpty();
        }

        [Fact]
        public void CreateFromJson_WithWhitespaceString_ShouldCreateEmptyPlanFeatures()
        {
            // Act
            var planFeatures = PlanFeatures.CreateFromJson("   ");

            // Assert
            planFeatures.Should().NotBeNull();
            planFeatures.Features.Should().BeEmpty();
        }

        [Fact]
        public void CreateFromJson_WithInvalidJson_ShouldCreateEmptyPlanFeatures()
        {
            // Arrange
            var invalidJson = "{ invalid json }";

            // Act
            var planFeatures = PlanFeatures.CreateFromJson(invalidJson);

            // Assert
            planFeatures.Should().NotBeNull();
            planFeatures.Features.Should().BeEmpty();
        }

        [Fact]
        public void ToJson_WithFeatures_ShouldReturnValidJson()
        {
            // Arrange
            var features = new List<Feature>
            {
                new Feature("Feature1", "Description1", "Value1", true),
                new Feature("Feature2", "Description2", "Value2", false)
            };
            var planFeatures = PlanFeatures.Create(features);

            // Act
            var json = planFeatures.ToJson();

            // Assert
            json.Should().NotBeNullOrEmpty();
            var deserializedFeatures = PlanFeatures.CreateFromJson(json);
            deserializedFeatures.Features.Should().HaveCount(2);
            deserializedFeatures.Features[0].Name.Should().Be("Feature1");
            deserializedFeatures.Features[1].Name.Should().Be("Feature2");
        }

        [Fact]
        public void ToJson_WithEmptyFeatures_ShouldReturnEmptyArrayJson()
        {
            // Arrange
            var planFeatures = PlanFeatures.Create(new List<Feature>());

            // Act
            var json = planFeatures.ToJson();

            // Assert
            json.Should().Be("[]");
        }

        [Fact]
        public void WithFeature_WithValidFeature_ShouldReturnNewInstanceWithFeature()
        {
            // Arrange
            var planFeatures = PlanFeatures.Create(new List<Feature>());
            var newFeature = new Feature("NewFeature", "New Description", "New Value", true);

            // Act
            var result = planFeatures.WithFeature(newFeature);

            // Assert
            result.Features.Should().HaveCount(1);
            result.Features[0].Should().Be(newFeature);
            planFeatures.Features.Should().HaveCount(0); // Original should be unchanged
        }

        [Fact]
        public void WithFeature_WithNullFeature_ShouldThrowArgumentNullException()
        {
            // Arrange
            var planFeatures = PlanFeatures.Create(new List<Feature>());

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => planFeatures.WithFeature(null!));
            exception.ParamName.Should().Be("feature");
        }

        [Fact]
        public void WithoutFeature_WithExistingFeature_ShouldReturnNewInstanceWithoutFeature()
        {
            // Arrange
            var features = new List<Feature>
            {
                new Feature("Feature1", "Description1", "Value1", true),
                new Feature("Feature2", "Description2", "Value2", false)
            };
            var planFeatures = PlanFeatures.Create(features);

            // Act
            var result = planFeatures.WithoutFeature("Feature1");

            // Assert
            result.Features.Should().HaveCount(1);
            result.Features[0].Name.Should().Be("Feature2");
            planFeatures.Features.Should().HaveCount(2); // Original should be unchanged
        }

        [Fact]
        public void WithoutFeature_WithNonExistingFeature_ShouldReturnNewInstanceWithSameFeatures()
        {
            // Arrange
            var features = new List<Feature>
            {
                new Feature("Feature1", "Description1", "Value1", true),
                new Feature("Feature2", "Description2", "Value2", false)
            };
            var planFeatures = PlanFeatures.Create(features);

            // Act
            var result = planFeatures.WithoutFeature("NonExistingFeature");

            // Assert
            result.Features.Should().HaveCount(2);
        }

        [Fact]
        public void WithoutFeature_WithCaseInsensitiveName_ShouldReturnNewInstanceWithoutFeature()
        {
            // Arrange
            var features = new List<Feature>
            {
                new Feature("Feature1", "Description1", "Value1", true),
                new Feature("Feature2", "Description2", "Value2", false)
            };
            var planFeatures = PlanFeatures.Create(features);

            // Act
            var result = planFeatures.WithoutFeature("feature1");

            // Assert
            result.Features.Should().HaveCount(1);
            result.Features[0].Name.Should().Be("Feature2");
        }

        [Fact]
        public void HasFeature_WithExistingFeature_ShouldReturnTrue()
        {
            // Arrange
            var features = new List<Feature>
            {
                new Feature("Feature1", "Description1", "Value1", true),
                new Feature("Feature2", "Description2", "Value2", false)
            };
            var planFeatures = PlanFeatures.Create(features);

            // Act
            var hasFeature = planFeatures.HasFeature("Feature1");

            // Assert
            hasFeature.Should().BeTrue();
        }

        [Fact]
        public void HasFeature_WithNonExistingFeature_ShouldReturnFalse()
        {
            // Arrange
            var features = new List<Feature>
            {
                new Feature("Feature1", "Description1", "Value1", true),
                new Feature("Feature2", "Description2", "Value2", false)
            };
            var planFeatures = PlanFeatures.Create(features);

            // Act
            var hasFeature = planFeatures.HasFeature("NonExistingFeature");

            // Assert
            hasFeature.Should().BeFalse();
        }

        [Fact]
        public void HasFeature_WithCaseInsensitiveName_ShouldReturnTrue()
        {
            // Arrange
            var features = new List<Feature>
            {
                new Feature("Feature1", "Description1", "Value1", true),
                new Feature("Feature2", "Description2", "Value2", false)
            };
            var planFeatures = PlanFeatures.Create(features);

            // Act
            var hasFeature = planFeatures.HasFeature("feature1");

            // Assert
            hasFeature.Should().BeTrue();
        }

        [Fact]
        public void GetFeature_WithExistingFeature_ShouldReturnFeature()
        {
            // Arrange
            var features = new List<Feature>
            {
                new Feature("Feature1", "Description1", "Value1", true),
                new Feature("Feature2", "Description2", "Value2", false)
            };
            var planFeatures = PlanFeatures.Create(features);

            // Act
            var feature = planFeatures.GetFeature("Feature1");

            // Assert
            feature.Should().NotBeNull();
            feature!.Name.Should().Be("Feature1");
            feature.Description.Should().Be("Description1");
            feature.Value.Should().Be("Value1");
            feature.IsEnabled.Should().BeTrue();
        }

        [Fact]
        public void GetFeature_WithNonExistingFeature_ShouldReturnNull()
        {
            // Arrange
            var features = new List<Feature>
            {
                new Feature("Feature1", "Description1", "Value1", true),
                new Feature("Feature2", "Description2", "Value2", false)
            };
            var planFeatures = PlanFeatures.Create(features);

            // Act
            var feature = planFeatures.GetFeature("NonExistingFeature");

            // Assert
            feature.Should().BeNull();
        }

        [Fact]
        public void GetFeature_WithCaseInsensitiveName_ShouldReturnFeature()
        {
            // Arrange
            var features = new List<Feature>
            {
                new Feature("Feature1", "Description1", "Value1", true),
                new Feature("Feature2", "Description2", "Value2", false)
            };
            var planFeatures = PlanFeatures.Create(features);

            // Act
            var feature = planFeatures.GetFeature("feature1");

            // Assert
            feature.Should().NotBeNull();
            feature!.Name.Should().Be("Feature1");
        }

        [Fact]
        public void Equals_WithSameFeatures_ShouldReturnTrue()
        {
            // Arrange
            var features1 = new List<Feature>
            {
                new Feature("Feature1", "Description1", "Value1", true),
                new Feature("Feature2", "Description2", "Value2", false)
            };
            var features2 = new List<Feature>
            {
                new Feature("Feature1", "Description1", "Value1", true),
                new Feature("Feature2", "Description2", "Value2", false)
            };

            var planFeatures1 = PlanFeatures.Create(features1);
            var planFeatures2 = PlanFeatures.Create(features2);

            // Act & Assert
            planFeatures1.Should().Be(planFeatures2);
            planFeatures1.Equals(planFeatures2).Should().BeTrue();
        }

        [Fact]
        public void Equals_WithDifferentFeatures_ShouldReturnFalse()
        {
            // Arrange
            var features1 = new List<Feature>
            {
                new Feature("Feature1", "Description1", "Value1", true)
            };
            var features2 = new List<Feature>
            {
                new Feature("Feature2", "Description2", "Value2", false)
            };

            var planFeatures1 = PlanFeatures.Create(features1);
            var planFeatures2 = PlanFeatures.Create(features2);

            // Act & Assert
            planFeatures1.Should().NotBe(planFeatures2);
            planFeatures1.Equals(planFeatures2).Should().BeFalse();
        }

        [Fact]
        public void Equals_WithNull_ShouldReturnFalse()
        {
            // Arrange
            var features = new List<Feature>
            {
                new Feature("Feature1", "Description1", "Value1", true)
            };
            var planFeatures = PlanFeatures.Create(features);

            // Act & Assert
            planFeatures.Equals(null).Should().BeFalse();
        }

        [Fact]
        public void ToString_ShouldReturnDescriptiveString()
        {
            // Arrange
            var features = new List<Feature>
            {
                new Feature("Feature1", "Description1", "Value1", true),
                new Feature("Feature2", "Description2", "Value2", false)
            };
            var planFeatures = PlanFeatures.Create(features);

            // Act
            var result = planFeatures.ToString();

            // Assert
            result.Should().Be("Features: 2 items");
        }
    }
}
