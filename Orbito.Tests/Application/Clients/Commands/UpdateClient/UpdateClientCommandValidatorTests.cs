using FluentAssertions;
using Orbito.Application.Clients.Commands.UpdateClient;
using Xunit;

namespace Orbito.Tests.Application.Clients.Commands.UpdateClient
{
    public class UpdateClientCommandValidatorTests
    {
        private readonly UpdateClientCommandValidator _validator;

        public UpdateClientCommandValidatorTests()
        {
            _validator = new UpdateClientCommandValidator();
        }

        [Fact]
        public void Validate_WithValidData_ShouldBeValid()
        {
            // Arrange
            var command = new UpdateClientCommand(
                Id: Guid.NewGuid(),
                CompanyName: "New Company",
                Phone: "+48123456789",
                DirectEmail: "new@example.com",
                DirectFirstName: "New",
                DirectLastName: "User");

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_WithEmptyId_ShouldBeInvalid()
        {
            // Arrange
            var command = new UpdateClientCommand(
                Id: Guid.Empty,
                CompanyName: "New Company",
                Phone: "+48123456789",
                DirectEmail: "new@example.com",
                DirectFirstName: "New",
                DirectLastName: "User");

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Client ID is required"));
        }

        [Fact]
        public void Validate_WithNoFieldsToUpdate_ShouldBeInvalid()
        {
            // Arrange
            var command = new UpdateClientCommand(
                Id: Guid.NewGuid(),
                CompanyName: null,
                Phone: null,
                DirectEmail: null,
                DirectFirstName: null,
                DirectLastName: null);

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("At least one field must be provided for update"));
        }

        [Fact]
        public void Validate_WithOnlyCompanyName_ShouldBeValid()
        {
            // Arrange
            var command = new UpdateClientCommand(
                Id: Guid.NewGuid(),
                CompanyName: "New Company",
                Phone: null,
                DirectEmail: null,
                DirectFirstName: null,
                DirectLastName: null);

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_WithOnlyPhone_ShouldBeValid()
        {
            // Arrange
            var command = new UpdateClientCommand(
                Id: Guid.NewGuid(),
                CompanyName: null,
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
        public void Validate_WithOnlyDirectEmail_ShouldBeValid()
        {
            // Arrange
            var command = new UpdateClientCommand(
                Id: Guid.NewGuid(),
                CompanyName: null,
                Phone: null,
                DirectEmail: "new@example.com",
                DirectFirstName: null,
                DirectLastName: null);

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_WithInvalidEmail_ShouldBeInvalid()
        {
            // Arrange
            var command = new UpdateClientCommand(
                Id: Guid.NewGuid(),
                CompanyName: "New Company",
                Phone: null,
                DirectEmail: "invalid-email",
                DirectFirstName: null,
                DirectLastName: null);

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("nie zawiera poprawnego adresu email"));
        }

        [Fact]
        public void Validate_WithTooLongEmail_ShouldBeInvalid()
        {
            // Arrange
            var longEmail = new string('a', 250) + "@example.com"; // 262 characters
            var command = new UpdateClientCommand(
                Id: Guid.NewGuid(),
                CompanyName: "New Company",
                Phone: null,
                DirectEmail: longEmail,
                DirectFirstName: null,
                DirectLastName: null);

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Email must be valid and not exceed 255 characters"));
        }

        [Fact]
        public void Validate_WithTooLongCompanyName_ShouldBeInvalid()
        {
            // Arrange
            var longCompanyName = new string('A', 201); // 201 characters
            var command = new UpdateClientCommand(
                Id: Guid.NewGuid(),
                CompanyName: longCompanyName,
                Phone: null,
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
            var command = new UpdateClientCommand(
                Id: Guid.NewGuid(),
                CompanyName: null,
                Phone: null,
                DirectEmail: null,
                DirectFirstName: longFirstName,
                DirectLastName: null);

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("First name must not exceed 100 characters"));
        }

        [Fact]
        public void Validate_WithTooLongLastName_ShouldBeInvalid()
        {
            // Arrange
            var longLastName = new string('A', 101); // 101 characters
            var command = new UpdateClientCommand(
                Id: Guid.NewGuid(),
                CompanyName: null,
                Phone: null,
                DirectEmail: null,
                DirectFirstName: null,
                DirectLastName: longLastName);

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Last name must not exceed 100 characters"));
        }

        [Fact]
        public void Validate_WithInvalidPhone_ShouldBeInvalid()
        {
            // Arrange
            var command = new UpdateClientCommand(
                Id: Guid.NewGuid(),
                CompanyName: "New Company",
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
            var command = new UpdateClientCommand(
                Id: Guid.NewGuid(),
                CompanyName: "New Company",
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
            var command = new UpdateClientCommand(
                Id: Guid.NewGuid(),
                CompanyName: "New Company",
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
            var command = new UpdateClientCommand(
                Id: Guid.NewGuid(),
                CompanyName: "Test Company", // Valid non-empty string
                Phone: "",
                DirectEmail: "",
                DirectFirstName: "",
                DirectLastName: "");

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
        }
    }
}
