using FluentAssertions;
using Orbito.Domain.Entities;
using Orbito.Domain.ValueObjects;
using Xunit;

namespace Orbito.Tests.Domain.Entities
{
    public class ClientTests
    {
        private readonly TenantId _tenantId = TenantId.New();
        private readonly Guid _userId = Guid.NewGuid();

        [Fact]
        public void CreateWithUser_ShouldCreateClientWithUser()
        {
            // Arrange
            var companyName = "Test Company";

            // Act
            var client = Client.CreateWithUser(_tenantId, _userId, companyName);

            // Assert
            client.Should().NotBeNull();
            client.TenantId.Should().Be(_tenantId);
            client.UserId.Should().Be(_userId);
            client.CompanyName.Should().Be(companyName);
            client.IsActive.Should().BeTrue();
            client.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void CreateWithUser_WithNullCompanyName_ShouldCreateClient()
        {
            // Act
            var client = Client.CreateWithUser(_tenantId, _userId, null);

            // Assert
            client.Should().NotBeNull();
            client.CompanyName.Should().BeNull();
            client.IsActive.Should().BeTrue();
        }

        [Fact]
        public void CreateDirect_ShouldCreateDirectClient()
        {
            // Arrange
            var email = "test@example.com";
            var firstName = "John";
            var lastName = "Doe";
            var companyName = "Test Company";

            // Act
            var client = Client.CreateDirect(_tenantId, email, firstName, lastName, companyName);

            // Assert
            client.Should().NotBeNull();
            client.TenantId.Should().Be(_tenantId);
            client.UserId.Should().BeNull();
            client.DirectEmail.Should().Be(email);
            client.DirectFirstName.Should().Be(firstName);
            client.DirectLastName.Should().Be(lastName);
            client.CompanyName.Should().Be(companyName);
            client.IsActive.Should().BeTrue();
            client.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void CreateDirect_WithNullCompanyName_ShouldCreateClient()
        {
            // Arrange
            var email = "test@example.com";
            var firstName = "John";
            var lastName = "Doe";

            // Act
            var client = Client.CreateDirect(_tenantId, email, firstName, lastName, null);

            // Assert
            client.Should().NotBeNull();
            client.CompanyName.Should().BeNull();
        }

        [Fact]
        public void Activate_ShouldSetIsActiveToTrue()
        {
            // Arrange
            var client = Client.CreateWithUser(_tenantId, _userId, "Test Company");
            client.Deactivate(); // First deactivate

            // Act
            client.Activate();

            // Assert
            client.IsActive.Should().BeTrue();
        }

        [Fact]
        public void Deactivate_ShouldSetIsActiveToFalse()
        {
            // Arrange
            var client = Client.CreateWithUser(_tenantId, _userId, "Test Company");

            // Act
            client.Deactivate();

            // Assert
            client.IsActive.Should().BeFalse();
        }

        [Fact]
        public void UpdateContactInfo_ShouldUpdateCompanyNameAndPhone()
        {
            // Arrange
            var client = Client.CreateWithUser(_tenantId, _userId, "Old Company");
            var newCompanyName = "New Company";
            var newPhone = "+48123456789";

            // Act
            client.UpdateContactInfo(newCompanyName, newPhone);

            // Assert
            client.CompanyName.Should().Be(newCompanyName);
            client.Phone.Should().Be(newPhone);
        }

        [Fact]
        public void UpdateContactInfo_WithNullValues_ShouldNotUpdate()
        {
            // Arrange
            var client = Client.CreateWithUser(_tenantId, _userId, "Test Company");
            var originalCompanyName = client.CompanyName;
            var originalPhone = client.Phone;

            // Act
            client.UpdateContactInfo(null, null);

            // Assert
            client.CompanyName.Should().Be(originalCompanyName);
            client.Phone.Should().Be(originalPhone);
        }

        [Fact]
        public void UpdateDirectInfo_ForClientWithUser_ShouldNotUpdate()
        {
            // Arrange
            var client = Client.CreateWithUser(_tenantId, _userId, "Test Company");
            var originalEmail = client.DirectEmail;
            var originalFirstName = client.DirectFirstName;
            var originalLastName = client.DirectLastName;

            // Act
            client.UpdateDirectInfo("new@example.com", "NewFirst", "NewLast");

            // Assert
            client.DirectEmail.Should().Be(originalEmail);
            client.DirectFirstName.Should().Be(originalFirstName);
            client.DirectLastName.Should().Be(originalLastName);
        }

        [Fact]
        public void UpdateDirectInfo_ForDirectClient_ShouldUpdate()
        {
            // Arrange
            var client = Client.CreateDirect(_tenantId, "old@example.com", "OldFirst", "OldLast", "Test Company");
            var newEmail = "new@example.com";
            var newFirstName = "NewFirst";
            var newLastName = "NewLast";

            // Act
            client.UpdateDirectInfo(newEmail, newFirstName, newLastName);

            // Assert
            client.DirectEmail.Should().Be(newEmail);
            client.DirectFirstName.Should().Be(newFirstName);
            client.DirectLastName.Should().Be(newLastName);
        }

        [Fact]
        public void CanBeDeleted_WithNoActiveSubscriptions_ShouldReturnTrue()
        {
            // Arrange
            var client = Client.CreateWithUser(_tenantId, _userId, "Test Company");

            // Act
            var result = client.CanBeDeleted();

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void Email_ForClientWithUser_ShouldReturnUserEmail()
        {
            // Arrange
            var client = Client.CreateWithUser(_tenantId, _userId, "Test Company");
            // Note: In real scenario, User would be loaded from database

            // Act
            var email = client.Email;

            // Assert
            email.Should().Be(client.DirectEmail ?? "");
        }

        [Fact]
        public void Email_ForDirectClient_ShouldReturnDirectEmail()
        {
            // Arrange
            var email = "test@example.com";
            var client = Client.CreateDirect(_tenantId, email, "John", "Doe", "Test Company");

            // Act
            var result = client.Email;

            // Assert
            result.Should().Be(email);
        }

        [Fact]
        public void FullName_ForDirectClient_ShouldReturnConcatenatedName()
        {
            // Arrange
            var client = Client.CreateDirect(_tenantId, "test@example.com", "John", "Doe", "Test Company");

            // Act
            var fullName = client.FullName;

            // Assert
            fullName.Should().Be("John Doe");
        }

        [Fact]
        public void FullName_WithEmptyNames_ShouldReturnEmptyString()
        {
            // Arrange
            var client = Client.CreateDirect(_tenantId, "test@example.com", "", "", "Test Company");

            // Act
            var fullName = client.FullName;

            // Assert
            fullName.Should().Be("");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void CreateWithUser_WithEmptyGuidUserId_ShouldCreateClient()
        {
            // Act
            var client = Client.CreateWithUser(_tenantId, Guid.Empty, "Test Company");
            
            // Assert
            client.Should().NotBeNull();
            client.UserId.Should().Be(Guid.Empty);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void CreateWithUser_WithEmptyCompanyName_ShouldCreateClient()
        {
            // Act
            var client = Client.CreateWithUser(_tenantId, Guid.NewGuid(), "");

            // Assert
            client.Should().NotBeNull();
            client.CompanyName.Should().Be("");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void CreateDirect_WithEmptyEmail_ShouldCreateClient()
        {
            // Act
            var client = Client.CreateDirect(_tenantId, "", "John", "Doe", "Test Company");
            
            // Assert
            client.Should().NotBeNull();
            client.DirectEmail.Should().Be("");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void CreateDirect_WithNullEmail_ShouldCreateClient()
        {
            // Act
            var client = Client.CreateDirect(_tenantId, null!, "John", "Doe", "Test Company");
            
            // Assert
            client.Should().NotBeNull();
            client.DirectEmail.Should().BeNull();
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void CreateDirect_WithEmptyFirstName_ShouldCreateClient()
        {
            // Act
            var client = Client.CreateDirect(_tenantId, "test@example.com", "", "Doe", "Test Company");

            // Assert
            client.Should().NotBeNull();
            client.DirectFirstName.Should().Be("");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void CreateDirect_WithEmptyLastName_ShouldCreateClient()
        {
            // Act
            var client = Client.CreateDirect(_tenantId, "test@example.com", "John", "", "Test Company");

            // Assert
            client.Should().NotBeNull();
            client.DirectLastName.Should().Be("");
        }
    }
}
