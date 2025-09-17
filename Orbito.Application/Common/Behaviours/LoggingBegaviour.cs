using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Orbito.Application.Common.Behaviours
{
    public class LoggingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> 
        where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<TRequest> _logger;

        public LoggingBehaviour(ILogger<TRequest> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<TResponse> Handle(
            TRequest request, 
            RequestHandlerDelegate<TResponse> next, 
            CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;
            var operationId = Guid.NewGuid().ToString();

            try
            {
                var timer = Stopwatch.StartNew();

                _logger.LogInformation(
                    $"Starting request {requestName} with operation id {operationId} at {DateTime.UtcNow}. Time: {timer.ElapsedMilliseconds}ms");

                var response = await next();

                timer.Stop();

                _logger.LogInformation(
                    $"Request {requestName} with operation id {operationId} completed successfully at {DateTime.UtcNow}. Time: {timer.ElapsedMilliseconds}ms");

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Request {requestName} with operation id {operationId} failed at {DateTime.UtcNow}");
                throw;
            }
        }
    }
}
