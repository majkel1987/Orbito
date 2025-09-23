using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;

namespace Orbito.Application.BackgroundJobs
{
    public class CheckExpiringSubscriptionsJob : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CheckExpiringSubscriptionsJob> _logger;
        private readonly TimeSpan _period = TimeSpan.FromHours(24); // Run daily

        public CheckExpiringSubscriptionsJob(
            IServiceProvider serviceProvider,
            ILogger<CheckExpiringSubscriptionsJob> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("CheckExpiringSubscriptionsJob started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckExpiringSubscriptions(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while checking expiring subscriptions");
                }

                await Task.Delay(_period, stoppingToken);
            }

            _logger.LogInformation("CheckExpiringSubscriptionsJob stopped");
        }

        private async Task CheckExpiringSubscriptions(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var subscriptionService = scope.ServiceProvider.GetRequiredService<ISubscriptionService>();

            _logger.LogInformation("Checking for expiring subscriptions");

            try
            {
                // Get subscriptions expiring in the next 7 days
                var expiringSubscriptions = await subscriptionService.GetExpiringSubscriptionsAsync(7, cancellationToken);

                foreach (var subscription in expiringSubscriptions)
                {
                    _logger.LogInformation("Subscription {SubscriptionId} for client {ClientId} is expiring on {ExpirationDate}", 
                        subscription.Id, subscription.ClientId, subscription.NextBillingDate);

                    // Here you would typically send notifications to clients
                    // For now, we'll just log the information
                    await SendExpirationNotification(subscription, cancellationToken);
                }

                _logger.LogInformation("Found {Count} expiring subscriptions", expiringSubscriptions.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while checking expiring subscriptions");
            }
        }

        private async Task SendExpirationNotification(Domain.Entities.Subscription subscription, CancellationToken cancellationToken)
        {
            // TODO: Implement notification logic (email, SMS, etc.)
            _logger.LogInformation("Sending expiration notification for subscription {SubscriptionId}", subscription.Id);
            
            // Simulate async work
            await Task.Delay(100, cancellationToken);
        }
    }
}
