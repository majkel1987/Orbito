using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orbito.Application.Common.Behaviours;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Common.Options;
using Orbito.Application.Common.Services;
using Orbito.Application.Services;
using Orbito.Application.Common.Settings;
using Orbito.Application.Common.Configuration;
using System.Reflection;

namespace Orbito.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(
            this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMediatR(mediatRConfig =>
            {
                mediatRConfig.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            });

            services.Configure<PerformanceSettings>(configuration.GetSection("PerformanceSettings"));
            services.Configure<PaymentRetryOptions>(configuration.GetSection(PaymentRetryOptions.SectionName));
            services.Configure<IdempotencySettings>(configuration.GetSection("IdempotencySettings"));
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            
            // Add pipeline behaviors
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));
            
            // Add common services
            services.AddScoped<IDateTime, DateTimeService>();
            services.AddScoped<ITenantContext, TenantContext>();
            services.AddScoped<ITenantProvider, TenantProvider>();
            services.AddScoped<IAdminSetupService, AdminSetupService>();
            services.AddScoped<IProviderService, ProviderService>();
            services.AddScoped<ISubscriptionPlanService, SubscriptionPlanService>();
            services.AddScoped<ISubscriptionService, SubscriptionService>();
            services.AddScoped<IPaymentProcessingService, PaymentProcessingService>();
            services.AddScoped<IPaymentNotificationService, PaymentNotificationService>();
            services.AddScoped<IPaymentRetryService, PaymentRetryService>();
            services.AddScoped<IPaymentMetricsService, PaymentMetricsService>();
            services.AddSingleton<ISecurityLimitService, SecurityLimitService>();
            
        // Add caching service
        services.AddScoped<ICacheService, Common.Services.MemoryCacheService>();

            return services;
        }
    }
}
