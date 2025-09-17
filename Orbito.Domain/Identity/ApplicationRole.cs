using Microsoft.AspNetCore.Identity;
using Orbito.Domain.Entities;
using Orbito.Domain.ValueObjects;

namespace Orbito.Domain.Identity
{
    public class ApplicationRole : IdentityRole<Guid>
    {
        public TenantId? TenantId { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public Provider? Provider { get; set; }

        public ApplicationRole() : base() { }

        public ApplicationRole(string roleName, TenantId? tenantId = null) : base(roleName)
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

        public static ApplicationRole CreateForTenant(string name, TenantId tenantId, string? description = null)
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
