using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orbito.Application.Common.Enums;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Common.Settings;
using System.Diagnostics;

namespace Orbito.Application.Common.Behaviours
{
    public class PerformanceBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<TRequest> _logger;
        private readonly PerformanceSettings _settings;
        private readonly IDateTime _dateTime;

        public PerformanceBehaviour(
            ILogger<TRequest> logger,
            IOptions<PerformanceSettings> settings,
            IDateTime dateTime)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
            _dateTime = dateTime ?? throw new ArgumentNullException(nameof(dateTime));
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            // Rozpoczynamy pomiar czasu
            var timer = Stopwatch.StartNew();
            var requestName = typeof(TRequest).Name;
            var startTime = _dateTime.Now;

            try
            {
                _logger.LogInformation(
                    "Starting request {RequestName} at {StartTime}",
                    requestName,
                    startTime);

                var response = await next();

                timer.Stop();
                var elapsedMilliseconds = timer.ElapsedMilliseconds;

                AnalyzePerformance(requestName, elapsedMilliseconds, request, startTime);

                return response;
            }
            catch (Exception ex)
            {
                timer.Stop();

                _logger.LogError(
                    ex,
                    "Error executing request {RequestName}. Execution time: {ElapsedMilliseconds}ms",
                    requestName,
                    timer.ElapsedMilliseconds);

                throw;
            }
        }

        private void AnalyzePerformance(
            string requestName,
            long elapsedMilliseconds,
            TRequest request,
            DateTime startTime)
        {
            var performanceLevel = DeterminePerformanceLevel(elapsedMilliseconds);
            var message = CreatePerformanceMessage(performanceLevel, requestName, elapsedMilliseconds);

            switch (performanceLevel)
            {
                case PerformanceLevel.Critical:
                    _logger.LogError(message + " {@Request}", request);
                    break;

                case PerformanceLevel.Warning:
                    _logger.LogWarning(message + " {@Request}", request);
                    break;

                case PerformanceLevel.Monitor:
                    _logger.LogInformation(message + " {@Request}", request);
                    break;

                default:
                    _logger.LogDebug(
                        "Request {RequestName} executed in {ElapsedMilliseconds}ms",
                        requestName,
                        elapsedMilliseconds);
                    break;
            }
        }

        private PerformanceLevel DeterminePerformanceLevel(long elapsedMilliseconds)
        {
            if (elapsedMilliseconds > _settings.CriticalThresholdMs)
                return PerformanceLevel.Critical;

            if (elapsedMilliseconds > _settings.WarningThresholdMs)
                return PerformanceLevel.Warning;

            if (elapsedMilliseconds > _settings.MonitorThresholdMs)
                return PerformanceLevel.Monitor;

            return PerformanceLevel.Normal;
        }

        private string CreatePerformanceMessage(
            PerformanceLevel level,
            string requestName,
            long elapsedMilliseconds)
        {
            return level switch
            {
                PerformanceLevel.Critical =>
                    $"CRITICAL PERFORMANCE ISSUE: {requestName} ({elapsedMilliseconds}ms)",
                PerformanceLevel.Warning =>
                    $"PERFORMANCE WARNING: {requestName} ({elapsedMilliseconds}ms)",
                PerformanceLevel.Monitor =>
                    $"PERFORMANCE MONITORING: {requestName} ({elapsedMilliseconds}ms)",
                _ => string.Empty
            };
        }
    }
}
