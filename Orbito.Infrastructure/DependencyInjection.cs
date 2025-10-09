using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Orbito.Application.Common.Configuration;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Identity;
using Orbito.Infrastructure.BackgroundJobs;
using Orbito.Infrastructure.Data;
using Orbito.Infrastructure.PaymentGateways.Stripe;
using Orbito.Infrastructure.PaymentGateways.Stripe.EventHandlers;
using Orbito.Infrastructure.PaymentGateways.Stripe.Models;
using Orbito.Infrastructure.Persistance;
using Orbito.Infrastructure.Persistence;
using System;
using System.Text;

namespace Orbito.Infrastructure
{
    /// <summary>
    /// Extension methods for registering infrastructure services including database, authentication,
    /// repositories, payment gateways, and security configurations
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Registers infrastructure services with proper security, performance, and resilience configurations
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">Application configuration</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services, IConfiguration configuration)
        {
            // Configure Database with connection pooling and retry strategy
            services.AddDbContext<ApplicationDbContext>((provider, options) =>
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection")
                        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

                // PERFORMANCE: Add connection pool configuration
                var pooledConnectionString = connectionString +
                    ";Max Pool Size=200;Min Pool Size=10;Connection Timeout=30;";

                options.UseSqlServer(pooledConnectionString, sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);

                    // RESILIENCE: Enable retry strategy with exponential backoff
                    // Note: If using explicit transactions, ensure they are compatible with retry logic
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                });

                options.ConfigureWarnings(warnings =>
                {
                    warnings.Ignore(RelationalEventId.PendingModelChangesWarning);
                });

                // FIXED: Removed inline LoggerFactory creation (memory leak)
                // EF Core will automatically use ILoggerFactory from DI container
            });

            // Add Identity services
            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // Add JWT Authentication
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = configuration["Jwt:Audience"],
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                        configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not found"))),
                    ValidateLifetime = true,
                    // FIXED: ClockSkew set to 5 minutes (industry standard) instead of Zero
                    // Zero is too restrictive and can cause issues with clock synchronization
                    ClockSkew = TimeSpan.FromMinutes(5)
                };
            });

            // NOTE: Rate Limiting, CORS, and Response Compression are configured in Program.cs
            // These services require ASP.NET Core web application context and cannot be registered in Infrastructure layer
            // See Program.cs for configuration details

            // Add Health Checks
            services.AddHealthChecks()
                .AddDbContextCheck<ApplicationDbContext>();

            // REMOVED: ITenantProvider duplicate registration
            // Already registered in Application layer (Application/DependencyInjection.cs:37)
            // services.AddScoped<ITenantProvider, Application.Common.Services.TenantProvider>();

            // Add HttpContextAccessor for user context services
            services.AddHttpContextAccessor();
            
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IProviderRepository, ProviderRepository>();
            services.AddScoped<IClientRepository, ClientRepository>();
            services.AddScoped<ISubscriptionPlanRepository, SubscriptionPlanRepository>();
            services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();
            services.AddScoped<IWebhookLogRepository, WebhookLogRepository>();
            services.AddScoped<IEmailNotificationRepository, EmailNotificationRepository>();
            services.AddScoped<IPaymentRetryRepository, PaymentRetryRepository>();
            services.AddScoped<IReconciliationRepository, ReconciliationRepository>();
            services.AddScoped<IEmailSender, Services.EmailSender>();
            services.AddScoped<IUserContextService, Services.UserContextService>();
            services.AddScoped<IPaymentReconciliationService, Services.PaymentReconciliationService>();

            // Configure reconciliation settings
            services.Configure<ReconciliationSettings>(configuration.GetSection("ReconciliationSettings"));

            // Configure Stripe
            services.Configure<StripeConfiguration>(configuration.GetSection("Stripe"));
            services.Configure<StripeWebhookSettings>(configuration.GetSection("StripeWebhookSettings"));
            services.AddScoped<IPaymentGateway, StripePaymentGateway>();
            services.AddScoped<IPaymentWebhookProcessor, StripeWebhookProcessor>();
            services.AddScoped<StripeEventHandler>();

            // Add Background Jobs
            services.AddHostedService<ProcessDuePaymentsJob>();
            services.AddHostedService<CheckPendingPaymentsJob>();
            services.AddHostedService<PaymentStatusSyncJob>();
            services.AddHostedService<DailyReconciliationJob>();

            // FIXED: Validate Stripe configuration at startup with proper validation
            services.AddOptions<StripeConfiguration>()
                .Validate(config =>
                {
                    // Validate required Stripe keys
                    if (string.IsNullOrWhiteSpace(config.SecretKey))
                        return false;
                    if (string.IsNullOrWhiteSpace(config.PublishableKey))
                        return false;
                    if (string.IsNullOrWhiteSpace(config.WebhookSecret))
                        return false;

                    // Validate key formats (basic check)
                    if (!config.SecretKey.StartsWith("sk_"))
                        return false;
                    if (!config.PublishableKey.StartsWith("pk_"))
                        return false;

                    return true;
                }, "Invalid Stripe configuration: SecretKey (sk_*), PublishableKey (pk_*), and WebhookSecret are required");

            services.AddOptions<StripeWebhookSettings>()
                .Validate(settings =>
                {
                    if (settings.MaxPayloadSize <= 0)
                        return false;
                    if (settings.SignatureToleranceSeconds <= 0)
                        return false;
                    if (settings.SignatureToleranceSeconds > 600) // Max 10 minutes
                        return false;
                    return true;
                }, "Invalid Stripe webhook settings: MaxPayloadSize and SignatureToleranceSeconds must be positive, tolerance max 600s");

            return services;
        }
    }
}
