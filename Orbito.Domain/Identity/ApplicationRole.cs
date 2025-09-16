using Microsoft.AspNetCore.Identity;
using Orbito.Domain.Entities;

namespace Orbito.Domain.Identity
{
    public class ApplicationRole : IdentityRole<Guid>
    {
        public Guid? TenantId { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public Provider? Provider { get; set; }

        public ApplicationRole() : base() { }

        public ApplicationRole(string roleName, Guid? tenantId = null) : base(roleName)
        {
            Id = Guid.NewGuid();
            TenantId = tenantId;
        }

        // Factory methods
        public static ApplicationRole CreateGlobal(string name, string? description = null)
        {
            return new ApplicationRole(name)
            {
                Description = description,
                TenantId = null
            };
        }

        public static ApplicationRole CreateForTenant(string name, Guid tenantId, string? description = null)
        {
            return new ApplicationRole(name, tenantId)
            {
                Description = description
            };
        }

        public ApplicationRole SetId(Guid id)
        {
            Id = id;
            return this;
        }
    }
}
