using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.ValueObjects;

namespace Orbito.Infrastructure.Data
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../Orbito.API"))
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null);
            });

            // Design-time services (for migrations)
            var designTimeTenantProvider = new DesignTimeTenantProvider();
            var designTimeUserContextService = new DesignTimeUserContextService();

            return new ApplicationDbContext(
                optionsBuilder.Options, 
                designTimeTenantProvider, 
                designTimeUserContextService,
                Microsoft.Extensions.Logging.Abstractions.NullLogger<ApplicationDbContext>.Instance);
        }

        /// <summary>
        /// Design-time user context service for EF Core migrations.
        /// Returns null for all user operations during migrations.
        /// </summary>
        private class DesignTimeUserContextService : IUserContextService
        {
            public Guid? GetCurrentUserId()
            {
                return null; // No user context during design-time operations
            }

            public Task<Guid?> GetCurrentClientIdAsync(CancellationToken cancellationToken = default)
            {
                return Task.FromResult<Guid?>(null); // No client context during design-time operations
            }

            public string? GetCurrentUserEmail()
            {
                return null; // No user context during design-time operations
            }

            public string? GetCurrentUserRole()
            {
                return null; // No user context during design-time operations
            }

            public bool IsAuthenticated()
            {
                return false; // No authentication during design-time operations
            }
        }

        /// <summary>
        /// Design-time tenant provider for EF Core migrations.
        /// Returns a default tenant ID that will be ignored by query filters during migrations.
        /// </summary>
        private class DesignTimeTenantProvider : ITenantProvider
        {
            public TenantId? GetCurrentTenantId()
            {
                // Return null for design-time operations (migrations)
                // Query filters will be disabled during migrations
                return null;
            }

            public Guid GetCurrentTenantIdAsGuid()
            {
                // Return Guid.Empty for design-time operations (migrations)
                // Query filters will be disabled during migrations
                return Guid.Empty;
            }

            public bool HasTenant()
            {
                // No tenant context during design-time operations
                return false;
            }

            public void SetTenantOverride(Guid tenantId)
            {
                // Not supported during design-time operations
                throw new NotSupportedException("Tenant override is not supported during design-time operations (migrations).");
            }

            public void ClearTenantOverride()
            {
                // Not supported during design-time operations
                throw new NotSupportedException("Tenant override is not supported during design-time operations (migrations).");
            }
        }
    }
}
