using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orbito.Application.Features.ProviderSubscriptions.Commands.SendTrialExpirationNotifications;

namespace Orbito.Infrastructure.BackgroundJobs;

/// <summary>
/// Background job for sending trial expiration notifications to providers.
/// Runs every hour to check for trial subscriptions approaching expiration
/// and sends notification emails at 5 days, 3 days, and 24 hours before expiration.
/// </summary>
public class TrialExpirationNotificationJob : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<TrialExpirationNotificationJob> _logger;
    private readonly TimeSpan _initialDelay;
    private readonly TimeSpan _period;

    public TrialExpirationNotificationJob(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<TrialExpirationNotificationJob> logger,
        TimeSpan? initialDelay = null,
        TimeSpan? period = null)
    {
        _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _initialDelay = initialDelay ?? TimeSpan.FromMinutes(5); // Start 5 minutes after app startup
        _period = period ?? TimeSpan.FromHours(1); // Run every hour
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "TrialExpirationNotificationJob started. Initial delay: {InitialDelay}, Period: {Period}",
            _initialDelay,
            _period);

        // Initial delay
        await Task.Delay(_initialDelay, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SendNotificationsAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Graceful shutdown
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in TrialExpirationNotificationJob");
            }

            await Task.Delay(_period, stoppingToken);
        }

        _logger.LogInformation("TrialExpirationNotificationJob stopped");
    }

    private async Task SendNotificationsAsync(CancellationToken stoppingToken)
    {
        _logger.LogDebug("Running trial expiration notification check...");

        using var scope = _serviceScopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        cts.CancelAfter(TimeSpan.FromMinutes(30)); // 30 min timeout for 1 hour job

        try
        {
            var result = await mediator.Send(
                new SendTrialExpirationNotificationsCommand(),
                cts.Token);

            if (result.IsSuccess)
            {
                _logger.LogInformation(
                    "Trial expiration notification check completed. Sent {NotificationCount} notifications.",
                    result.Value);
            }
            else
            {
                _logger.LogWarning(
                    "Trial expiration notification check failed: {Error}",
                    result.Error);
            }
        }
        catch (OperationCanceledException) when (cts.IsCancellationRequested && !stoppingToken.IsCancellationRequested)
        {
            _logger.LogWarning("Trial expiration notification check timed out");
        }
    }
}
