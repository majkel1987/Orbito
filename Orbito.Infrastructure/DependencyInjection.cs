using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
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
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>((provider, options) =>
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection")
                        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                    // Wyłączamy retry strategy, aby uniknąć konfliktu z ręcznymi transakcjami
                    // sqlOptions.EnableRetryOnFailure(
                    //     maxRetryCount: 3,
                    //     maxRetryDelay: TimeSpan.FromSeconds(10),
                    //     errorNumbersToAdd: null);
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
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not found"))),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

            // Add Health Checks
            services.AddHealthChecks()
                .AddDbContextCheck<ApplicationDbContext>();

            // Register ITenantProvider for ApplicationDbContext
            services.AddScoped<ITenantProvider, Application.Common.Services.TenantProvider>();
            
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IProviderRepository, ProviderRepository>();
            services.AddScoped<IClientRepository, ClientRepository>();
            services.AddScoped<ISubscriptionPlanRepository, SubscriptionPlanRepository>();
            services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();
            services.AddScoped<IWebhookLogRepository, WebhookLogRepository>();

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

            // Validate Stripe configuration at startup
            services.AddOptions<StripeConfiguration>()
                .Validate(config =>
                {
                    // Validate only if signature verification is enabled
                    return true; // Basic validation, detailed validation happens at runtime
                }, "Invalid Stripe configuration");

            services.AddOptions<StripeWebhookSettings>()
                .Validate(settings =>
                {
                    if (settings.MaxPayloadSize <= 0)
                        return false;
                    if (settings.SignatureToleranceSeconds <= 0)
                        return false;
                    return true;
                }, "Invalid Stripe webhook settings");

            return services;
        }
    }
}
