using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;

namespace Orbito.Application.BackgroundJobs
{
    public class ProcessRecurringPaymentsJob : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ProcessRecurringPaymentsJob> _logger;
        private readonly TimeSpan _period = TimeSpan.FromHours(1); // Run hourly

        public ProcessRecurringPaymentsJob(
            IServiceProvider serviceProvider,
            ILogger<ProcessRecurringPaymentsJob> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ProcessRecurringPaymentsJob started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessRecurringPayments(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing recurring payments");
                }

                await Task.Delay(_period, stoppingToken);
            }

            _logger.LogInformation("ProcessRecurringPaymentsJob stopped");
        }

        private async Task ProcessRecurringPayments(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var subscriptionService = scope.ServiceProvider.GetRequiredService<ISubscriptionService>();
            var dateTime = scope.ServiceProvider.GetRequiredService<IDateTime>();

            var currentDate = dateTime.UtcNow;

            _logger.LogInformation("Processing recurring payments for date {Date}", currentDate.Date);

            try
            {
                // Process recurring payments for today
                await subscriptionService.ProcessRecurringPaymentsAsync(currentDate, cancellationToken);

                // Process expired subscriptions
                await subscriptionService.ProcessExpiredSubscriptionsAsync(cancellationToken);

                _logger.LogInformation("Successfully processed recurring payments for date {Date}", currentDate.Date);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing recurring payments for date {Date}", currentDate.Date);
            }
        }
    }
}
