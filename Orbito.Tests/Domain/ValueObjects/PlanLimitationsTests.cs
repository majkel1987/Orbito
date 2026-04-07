using FluentAssertions;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;
using Xunit;

namespace Orbito.Tests.Domain.ValueObjects
{
    public class PlanLimitationsTests
    {
        [Fact]
        public void Create_WithValidLimitations_ShouldCreatePlanLimitations()
        {
            // Arrange
            var limitations = new List<Limitation>
            {
                new Limitation("Limit1", LimitationType.Numeric, "Description1", 10, null),
                new Limitation("Limit2", LimitationType.String, "Description2", null, "Value2")
            };

            // Act
            var planLimitations = PlanLimitations.Create(limitations);

            // Assert
            planLimitations.Should().NotBeNull();
            planLimitations.Limitations.Should().HaveCount(2);
            planLimitations.Limitations.Should().Contain(limitations);
        }

        [Fact]
        public void Create_WithEmptyList_ShouldCreateEmptyPlanLimitations()
        {
            // Arrange
            var limitations = new List<Limitation>();

            // Act
            var planLimitations = PlanLimitations.Create(limitations);

            // Assert
            planLimitations.Should().NotBeNull();
            planLimitations.Limitations.Should().BeEmpty();
        }

        [Fact]
        public void Create_WithNullLimitations_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => PlanLimitations.Create(null!));
            exception.ParamName.Should().Be("limitations");
        }

        [Fact]
        public void CreateFromJson_WithValidJson_ShouldCreatePlanLimitations()
        {
            // Arrange
            var json = @"[
                {
                    ""name"": ""Limit1"",
                    ""description"": ""Description1"",
                    ""numericValue"": 10,
                    ""stringValue"": null,
                    ""type"": 1
                },
                {
                    ""name"": ""Limit2"",
                    ""description"": ""Description2"",
                    ""numericValue"": null,
                    ""stringValue"": ""Value2"",
                    ""type"": 2
                }
            ]";

            // Act
            var planLimitations = PlanLimitations.CreateFromJson(json);

            // Assert
            planLimitations.Should().NotBeNull();
            planLimitations.Limitations.Should().HaveCount(2);
            planLimitations.Limitations[0].Name.Should().Be("Limit1");
            planLimitations.Limitations[0].Description.Should().Be("Description1");
            planLimitations.Limitations[0].NumericValue.Should().Be(10);
            planLimitations.Limitations[0].StringValue.Should().BeNull();
            planLimitations.Limitations[0].Type.Should().Be(LimitationType.Numeric);
            planLimitations.Limitations[1].Name.Should().Be("Limit2");
            planLimitations.Limitations[1].Description.Should().Be("Description2");
            planLimitations.Limitations[1].NumericValue.Should().BeNull();
            planLimitations.Limitations[1].StringValue.Should().Be("Value2");
            planLimitations.Limitations[1].Type.Should().Be(LimitationType.String);
        }

        [Fact]
        public void CreateFromJson_WithEmptyJson_ShouldCreateEmptyPlanLimitations()
        {
            // Arrange
            var json = "[]";

            // Act
            var planLimitations = PlanLimitations.CreateFromJson(json);

            // Assert
            planLimitations.Should().NotBeNull();
            planLimitations.Limitations.Should().BeEmpty();
        }

        [Fact]
        public void CreateFromJson_WithNullJson_ShouldCreateEmptyPlanLimitations()
        {
            // Act
            var planLimitations = PlanLimitations.CreateFromJson(null);

            // Assert
            planLimitations.Should().NotBeNull();
            planLimitations.Limitations.Should().BeEmpty();
        }

        [Fact]
        public void CreateFromJson_WithEmptyString_ShouldCreateEmptyPlanLimitations()
        {
            // Act
            var planLimitations = PlanLimitations.CreateFromJson("");

            // Assert
            planLimitations.Should().NotBeNull();
            planLimitations.Limitations.Should().BeEmpty();
        }

        [Fact]
        public void CreateFromJson_WithWhitespaceString_ShouldCreateEmptyPlanLimitations()
        {
            // Act
            var planLimitations = PlanLimitations.CreateFromJson("   ");

            // Assert
            planLimitations.Should().NotBeNull();
            planLimitations.Limitations.Should().BeEmpty();
        }

        [Fact]
        public void CreateFromJson_WithInvalidJson_ShouldCreateEmptyPlanLimitations()
        {
            // Arrange
            var invalidJson = "{ invalid json }";

            // Act
            var planLimitations = PlanLimitations.CreateFromJson(invalidJson);

            // Assert
            planLimitations.Should().NotBeNull();
            planLimitations.Limitations.Should().BeEmpty();
        }

        [Fact]
        public void ToJson_WithLimitations_ShouldReturnValidJson()
        {
            // Arrange
            var limitations = new List<Limitation>
            {
                new Limitation("Limit1", LimitationType.Numeric, "Description1", 10, null),
                new Limitation("Limit2", LimitationType.String, "Description2", null, "Value2")
            };
            var planLimitations = PlanLimitations.Create(limitations);

            // Act
            var json = planLimitations.ToJson();

            // Assert
            json.Should().NotBeNullOrEmpty();
            var deserializedLimitations = PlanLimitations.CreateFromJson(json);
            deserializedLimitations.Limitations.Should().HaveCount(2);
            deserializedLimitations.Limitations[0].Name.Should().Be("Limit1");
            deserializedLimitations.Limitations[1].Name.Should().Be("Limit2");
        }

        [Fact]
        public void ToJson_WithEmptyLimitations_ShouldReturnEmptyArrayJson()
        {
            // Arrange
            var planLimitations = PlanLimitations.Create(new List<Limitation>());

            // Act
            var json = planLimitations.ToJson();

            // Assert
            json.Should().Be("[]");
        }

        [Fact]
        public void WithLimitation_WithValidLimitation_ShouldReturnNewInstanceWithLimitation()
        {
            // Arrange
            var planLimitations = PlanLimitations.Create(new List<Limitation>());
            var newLimitation = new Limitation("NewLimit", LimitationType.Numeric, "New Description", 20, null);

            // Act
            var result = planLimitations.WithLimitation(newLimitation);

            // Assert
            result.Limitations.Should().HaveCount(1);
            result.Limitations[0].Should().Be(newLimitation);
            planLimitations.Limitations.Should().HaveCount(0); // Original should be unchanged
        }

        [Fact]
        public void WithLimitation_WithNullLimitation_ShouldThrowArgumentNullException()
        {
            // Arrange
            var planLimitations = PlanLimitations.Create(new List<Limitation>());

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => planLimitations.WithLimitation(null!));
            exception.ParamName.Should().Be("limitation");
        }

        [Fact]
        public void WithoutLimitation_WithExistingLimitation_ShouldReturnNewInstanceWithoutLimitation()
        {
            // Arrange
            var limitations = new List<Limitation>
            {
                new Limitation("Limit1", LimitationType.Numeric, "Description1", 10, null),
                new Limitation("Limit2", LimitationType.String, "Description2", null, "Value2")
            };
            var planLimitations = PlanLimitations.Create(limitations);

            // Act
            var result = planLimitations.WithoutLimitation("Limit1");

            // Assert
            result.Limitations.Should().HaveCount(1);
            result.Limitations[0].Name.Should().Be("Limit2");
            planLimitations.Limitations.Should().HaveCount(2); // Original should be unchanged
        }

        [Fact]
        public void WithoutLimitation_WithNonExistingLimitation_ShouldReturnNewInstanceWithSameLimitations()
        {
            // Arrange
            var limitations = new List<Limitation>
            {
                new Limitation("Limit1", LimitationType.Numeric, "Description1", 10, null),
                new Limitation("Limit2", LimitationType.String, "Description2", null, "Value2")
            };
            var planLimitations = PlanLimitations.Create(limitations);

            // Act
            var result = planLimitations.WithoutLimitation("NonExistingLimit");

            // Assert
            result.Limitations.Should().HaveCount(2);
        }

        [Fact]
        public void WithoutLimitation_WithCaseInsensitiveName_ShouldReturnNewInstanceWithoutLimitation()
        {
            // Arrange
            var limitations = new List<Limitation>
            {
                new Limitation("Limit1", LimitationType.Numeric, "Description1", 10, null),
                new Limitation("Limit2", LimitationType.String, "Description2", null, "Value2")
            };
            var planLimitations = PlanLimitations.Create(limitations);

            // Act
            var result = planLimitations.WithoutLimitation("limit1");

            // Assert
            result.Limitations.Should().HaveCount(1);
            result.Limitations[0].Name.Should().Be("Limit2");
        }

        [Fact]
        public void HasLimitation_WithExistingLimitation_ShouldReturnTrue()
        {
            // Arrange
            var limitations = new List<Limitation>
            {
                new Limitation("Limit1", LimitationType.Numeric, "Description1", 10, null),
                new Limitation("Limit2", LimitationType.String, "Description2", null, "Value2")
            };
            var planLimitations = PlanLimitations.Create(limitations);

            // Act
            var hasLimitation = planLimitations.HasLimitation("Limit1");

            // Assert
            hasLimitation.Should().BeTrue();
        }

        [Fact]
        public void HasLimitation_WithNonExistingLimitation_ShouldReturnFalse()
        {
            // Arrange
            var limitations = new List<Limitation>
            {
                new Limitation("Limit1", LimitationType.Numeric, "Description1", 10, null),
                new Limitation("Limit2", LimitationType.String, "Description2", null, "Value2")
            };
            var planLimitations = PlanLimitations.Create(limitations);

            // Act
            var hasLimitation = planLimitations.HasLimitation("NonExistingLimit");

            // Assert
            hasLimitation.Should().BeFalse();
        }

        [Fact]
        public void HasLimitation_WithCaseInsensitiveName_ShouldReturnTrue()
        {
            // Arrange
            var limitations = new List<Limitation>
            {
                new Limitation("Limit1", LimitationType.Numeric, "Description1", 10, null),
                new Limitation("Limit2", LimitationType.String, "Description2", null, "Value2")
            };
            var planLimitations = PlanLimitations.Create(limitations);

            // Act
            var hasLimitation = planLimitations.HasLimitation("limit1");

            // Assert
            hasLimitation.Should().BeTrue();
        }

        [Fact]
        public void GetLimitation_WithExistingLimitation_ShouldReturnLimitation()
        {
            // Arrange
            var limitations = new List<Limitation>
            {
                new Limitation("Limit1", LimitationType.Numeric, "Description1", 10, null),
                new Limitation("Limit2", LimitationType.String, "Description2", null, "Value2")
            };
            var planLimitations = PlanLimitations.Create(limitations);

            // Act
            var limitation = planLimitations.GetLimitation("Limit1");

            // Assert
            limitation.Should().NotBeNull();
            limitation!.Name.Should().Be("Limit1");
            limitation.Description.Should().Be("Description1");
            limitation.NumericValue.Should().Be(10);
            limitation.StringValue.Should().BeNull();
            limitation.Type.Should().Be(LimitationType.Numeric);
        }

        [Fact]
        public void GetLimitation_WithNonExistingLimitation_ShouldReturnNull()
        {
            // Arrange
            var limitations = new List<Limitation>
            {
                new Limitation("Limit1", LimitationType.Numeric, "Description1", 10, null),
                new Limitation("Limit2", LimitationType.String, "Description2", null, "Value2")
            };
            var planLimitations = PlanLimitations.Create(limitations);

            // Act
            var limitation = planLimitations.GetLimitation("NonExistingLimit");

            // Assert
            limitation.Should().BeNull();
        }

        [Fact]
        public void GetLimitation_WithCaseInsensitiveName_ShouldReturnLimitation()
        {
            // Arrange
            var limitations = new List<Limitation>
            {
                new Limitation("Limit1", LimitationType.Numeric, "Description1", 10, null),
                new Limitation("Limit2", LimitationType.String, "Description2", null, "Value2")
            };
            var planLimitations = PlanLimitations.Create(limitations);

            // Act
            var limitation = planLimitations.GetLimitation("limit1");

            // Assert
            limitation.Should().NotBeNull();
            limitation!.Name.Should().Be("Limit1");
        }

        [Fact]
        public void GetNumericLimit_WithExistingNumericLimitation_ShouldReturnNumericValue()
        {
            // Arrange
            var limitations = new List<Limitation>
            {
                new Limitation("Limit1", LimitationType.Numeric, "Description1", 10, null),
                new Limitation("Limit2", LimitationType.String, "Description2", null, "Value2")
            };
            var planLimitations = PlanLimitations.Create(limitations);

            // Act
            var numericValue = planLimitations.GetNumericLimit("Limit1");

            // Assert
            numericValue.Should().Be(10);
        }

        [Fact]
        public void GetNumericLimit_WithNonExistingLimitation_ShouldReturnNull()
        {
            // Arrange
            var limitations = new List<Limitation>
            {
                new Limitation("Limit1", LimitationType.Numeric, "Description1", 10, null),
                new Limitation("Limit2", LimitationType.String, "Description2", null, "Value2")
            };
            var planLimitations = PlanLimitations.Create(limitations);

            // Act
            var numericValue = planLimitations.GetNumericLimit("NonExistingLimit");

            // Assert
            numericValue.Should().BeNull();
        }

        [Fact]
        public void GetNumericLimit_WithStringLimitation_ShouldReturnNull()
        {
            // Arrange
            var limitations = new List<Limitation>
            {
                new Limitation("Limit1", LimitationType.Numeric, "Description1", 10, null),
                new Limitation("Limit2", LimitationType.String, "Description2", null, "Value2")
            };
            var planLimitations = PlanLimitations.Create(limitations);

            // Act
            var numericValue = planLimitations.GetNumericLimit("Limit2");

            // Assert
            numericValue.Should().BeNull();
        }

        [Fact]
        public void GetNumericLimit_WithCaseInsensitiveName_ShouldReturnNumericValue()
        {
            // Arrange
            var limitations = new List<Limitation>
            {
                new Limitation("Limit1", LimitationType.Numeric, "Description1", 10, null),
                new Limitation("Limit2", LimitationType.String, "Description2", null, "Value2")
            };
            var planLimitations = PlanLimitations.Create(limitations);

            // Act
            var numericValue = planLimitations.GetNumericLimit("limit1");

            // Assert
            numericValue.Should().Be(10);
        }

        [Fact]
        public void Equals_WithSameLimitations_ShouldReturnTrue()
        {
            // Arrange
            var limitations1 = new List<Limitation>
            {
                new Limitation("Limit1", LimitationType.Numeric, "Description1", 10, null),
                new Limitation("Limit2", LimitationType.String, "Description2", null, "Value2")
            };
            var limitations2 = new List<Limitation>
            {
                new Limitation("Limit1", LimitationType.Numeric, "Description1", 10, null),
                new Limitation("Limit2", LimitationType.String, "Description2", null, "Value2")
            };

            var planLimitations1 = PlanLimitations.Create(limitations1);
            var planLimitations2 = PlanLimitations.Create(limitations2);

            // Act & Assert
            planLimitations1.Should().Be(planLimitations2);
            planLimitations1.Equals(planLimitations2).Should().BeTrue();
        }

        [Fact]
        public void Equals_WithDifferentLimitations_ShouldReturnFalse()
        {
            // Arrange
            var limitations1 = new List<Limitation>
            {
                new Limitation("Limit1", LimitationType.Numeric, "Description1", 10, null)
            };
            var limitations2 = new List<Limitation>
            {
                new Limitation("Limit2", LimitationType.String, "Description2", null, "Value2")
            };

            var planLimitations1 = PlanLimitations.Create(limitations1);
            var planLimitations2 = PlanLimitations.Create(limitations2);

            // Act & Assert
            planLimitations1.Should().NotBe(planLimitations2);
            planLimitations1.Equals(planLimitations2).Should().BeFalse();
        }

        [Fact]
        public void Equals_WithNull_ShouldReturnFalse()
        {
            // Arrange
            var limitations = new List<Limitation>
            {
                new Limitation("Limit1", LimitationType.Numeric, "Description1", 10, null)
            };
            var planLimitations = PlanLimitations.Create(limitations);

            // Act & Assert
            planLimitations.Equals(null).Should().BeFalse();
        }

        [Fact]
        public void ToString_ShouldReturnDescriptiveString()
        {
            // Arrange
            var limitations = new List<Limitation>
            {
                new Limitation("Limit1", LimitationType.Numeric, "Description1", 10, null),
                new Limitation("Limit2", LimitationType.String, "Description2", null, "Value2")
            };
            var planLimitations = PlanLimitations.Create(limitations);

            // Act
            var result = planLimitations.ToString();

            // Assert
            result.Should().Be("Limitations: 2 items");
        }
    }
}
