using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orbito.Application.Common.Behaviours;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Common.Services;
using Orbito.Application.Common.Settings;
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
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            
            // Add pipeline behaviors
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));
            
            // Add common services
            services.AddScoped<IDateTime, DateTimeService>();

            return services;
        }
    }
}
