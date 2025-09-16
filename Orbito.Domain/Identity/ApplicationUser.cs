using Microsoft.AspNetCore.Identity;
using Orbito.Domain.Entities;

namespace Orbito.Domain.Identity
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public Guid? TenantId { get; set; }

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }

        public Provider? Provider { get; set; }        // Jeśli user jest providerem
        public Client? ClientProfile { get; set; }     // Jeśli user jest klientem

        public string FullName => $"{FirstName} {LastName}";

        public void UpdateLastLogin()
        {
            LastLoginAt = DateTime.UtcNow;
        }
    }
}
