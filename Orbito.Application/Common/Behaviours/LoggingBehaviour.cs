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

            using (_logger.BeginScope(new Dictionary<string, object>
            {
                ["OperationId"] = operationId,
                ["RequestName"] = requestName
            }))
            {
                try
                {
                    var timer = Stopwatch.StartNew();

                    _logger.LogInformation(
                        "Starting request {RequestName} with operation id {OperationId} at {Timestamp}",
                        requestName,
                        operationId,
                        DateTime.UtcNow);

                    var response = await next();

                    timer.Stop();

                    _logger.LogInformation(
                        "Request {RequestName} with operation id {OperationId} completed successfully at {Timestamp}. Elapsed: {ElapsedMilliseconds}ms",
                        requestName,
                        operationId,
                        DateTime.UtcNow,
                        timer.ElapsedMilliseconds);

                    return response;
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Request {RequestName} with operation id {OperationId} failed at {Timestamp}",
                        requestName,
                        operationId,
                        DateTime.UtcNow);
                    throw;
                }
            }
        }
    }
}
