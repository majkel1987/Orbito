using FluentAssertions;
using Orbito.Application.Clients.Commands.CreateClient;
using Xunit;

namespace Orbito.Tests.Application.Clients.Commands.CreateClient
{
    public class CreateClientCommandValidatorTests
    {
        private readonly CreateClientCommandValidator _validator;

        public CreateClientCommandValidatorTests()
        {
            _validator = new CreateClientCommandValidator();
        }

        [Fact]
        public void Validate_WithUserId_ShouldBeValid()
        {
            // Arrange
            var command = new CreateClientCommand(
                UserId: Guid.NewGuid(),
                CompanyName: "Test Company",
                Phone: "+48123456789",
                DirectEmail: null,
                DirectFirstName: null,
                DirectLastName: null);

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_WithDirectEmail_ShouldBeValid()
        {
            // Arrange
            var command = new CreateClientCommand(
                UserId: null,
                CompanyName: "Test Company",
                Phone: "+48123456789",
                DirectEmail: "test@example.com",
                DirectFirstName: "John",
                DirectLastName: "Doe");

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_WithNeitherUserIdNorEmail_ShouldBeInvalid()
        {
            // Arrange
            var command = new CreateClientCommand(
                UserId: null,
                CompanyName: "Test Company",
                Phone: "+48123456789",
                DirectEmail: null,
                DirectFirstName: null,
                DirectLastName: null);

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Either UserId or DirectEmail must be provided"));
        }

        [Fact]
        public void Validate_WithBothUserIdAndEmail_ShouldBeInvalid()
        {
            // Arrange
            var command = new CreateClientCommand(
                UserId: Guid.NewGuid(),
                CompanyName: "Test Company",
                Phone: "+48123456789",
                DirectEmail: "test@example.com",
                DirectFirstName: "John",
                DirectLastName: "Doe");

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Cannot provide both UserId and DirectEmail"));
        }

        [Fact]
        public void Validate_WithDirectEmailButNoFirstName_ShouldBeInvalid()
        {
            // Arrange
            var command = new CreateClientCommand(
                UserId: null,
                CompanyName: "Test Company",
                Phone: "+48123456789",
                DirectEmail: "test@example.com",
                DirectFirstName: null,
                DirectLastName: "Doe");

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("nie może być puste"));
        }

        [Fact]
        public void Validate_WithDirectEmailButNoLastName_ShouldBeInvalid()
        {
            // Arrange
            var command = new CreateClientCommand(
                UserId: null,
                CompanyName: "Test Company",
                Phone: "+48123456789",
                DirectEmail: "test@example.com",
                DirectFirstName: "John",
                DirectLastName: null);

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("nie może być puste"));
        }

        [Fact]
        public void Validate_WithInvalidEmail_ShouldBeInvalid()
        {
            // Arrange
            var command = new CreateClientCommand(
                UserId: null,
                CompanyName: "Test Company",
                Phone: "+48123456789",
                DirectEmail: "invalid-email",
                DirectFirstName: "John",
                DirectLastName: "Doe");

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Valid email is required when UserId is not provided"));
        }

        [Fact]
        public void Validate_WithTooLongCompanyName_ShouldBeInvalid()
        {
            // Arrange
            var longCompanyName = new string('A', 201); // 201 characters
            var command = new CreateClientCommand(
                UserId: Guid.NewGuid(),
                CompanyName: longCompanyName,
                Phone: "+48123456789",
                DirectEmail: null,
                DirectFirstName: null,
                DirectLastName: null);

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Company name must not exceed 200 characters"));
        }

        [Fact]
        public void Validate_WithTooLongFirstName_ShouldBeInvalid()
        {
            // Arrange
            var longFirstName = new string('A', 101); // 101 characters
            var command = new CreateClientCommand(
                UserId: null,
                CompanyName: "Test Company",
                Phone: "+48123456789",
                DirectEmail: "test@example.com",
                DirectFirstName: longFirstName,
                DirectLastName: "Doe");

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("First name is required and must not exceed 100 characters when UserId is not provided"));
        }

        [Fact]
        public void Validate_WithTooLongLastName_ShouldBeInvalid()
        {
            // Arrange
            var longLastName = new string('A', 101); // 101 characters
            var command = new CreateClientCommand(
                UserId: null,
                CompanyName: "Test Company",
                Phone: "+48123456789",
                DirectEmail: "test@example.com",
                DirectFirstName: "John",
                DirectLastName: longLastName);

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Last name is required and must not exceed 100 characters when UserId is not provided"));
        }

        [Fact]
        public void Validate_WithInvalidPhone_ShouldBeInvalid()
        {
            // Arrange
            var command = new CreateClientCommand(
                UserId: Guid.NewGuid(),
                CompanyName: "Test Company",
                Phone: "invalid-phone",
                DirectEmail: null,
                DirectFirstName: null,
                DirectLastName: null);

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Phone number must be valid"));
        }

        [Fact]
        public void Validate_WithTooLongPhone_ShouldBeInvalid()
        {
            // Arrange
            var longPhone = new string('1', 21); // 21 characters
            var command = new CreateClientCommand(
                UserId: Guid.NewGuid(),
                CompanyName: "Test Company",
                Phone: longPhone,
                DirectEmail: null,
                DirectFirstName: null,
                DirectLastName: null);

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Phone number must be valid and not exceed 20 characters"));
        }

        [Fact]
        public void Validate_WithValidPhone_ShouldBeValid()
        {
            // Arrange
            var command = new CreateClientCommand(
                UserId: Guid.NewGuid(),
                CompanyName: "Test Company",
                Phone: "+48123456789",
                DirectEmail: null,
                DirectFirstName: null,
                DirectLastName: null);

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_WithEmptyStringValues_ShouldBeValid()
        {
            // Arrange
            var command = new CreateClientCommand(
                UserId: Guid.NewGuid(),
                CompanyName: "",
                Phone: "",
                DirectEmail: null,
                DirectFirstName: null,
                DirectLastName: null);

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
        }
    }
}
