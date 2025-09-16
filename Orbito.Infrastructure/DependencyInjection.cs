using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Orbito.Domain.Identity;
using Orbito.Infrastructure.Data;
using System.Text;

namespace Orbito.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>((provider, options) =>
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection")
                        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null);
                });

                options.ConfigureWarnings(warnings =>
                {
                    warnings.Ignore(RelationalEventId.PendingModelChangesWarning);
                });

                options.UseLoggerFactory(LoggerFactory.Create(builder =>
                    builder
                        .AddConsole()
                        .AddDebug()
                        .SetMinimumLevel(LogLevel.Information)));
            });

            // Add Identity after DbContext is registered
            services.AddIdentity<ApplicationUser, ApplicationRole>(options => {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;
            }).AddEntityFrameworkStores<ApplicationDbContext>();

             services.AddAuthentication(options => {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options => {
            options.TokenValidationParameters = new TokenValidationParameters
                {
                ValidateIssuer = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = configuration["Jwt:Audience"],
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"])),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
                };
            });

            services.AddMultiTenant<TenantInfo>()
                .WithHostStrategy()
                .WithHeaderStrategy("X-Tenant-ID")
                .WithRouteStrategy("tenant")
                .WithConfigurationStore();

            services.AddAuthorization();
            
            return services;
        }
    }
}
