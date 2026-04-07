using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Orbito.Application.Common.Behaviours
{
    public class ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;
        private readonly ILogger<ValidationBehaviour<TRequest, TResponse>> _logger;

        public ValidationBehaviour(
            IEnumerable<IValidator<TRequest>> validators,
            ILogger<ValidationBehaviour<TRequest, TResponse>> logger)
        {
            _validators = validators ?? throw new ArgumentNullException(nameof(validators));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            if (!_validators.Any())
            {
                return await next();
            }

            var requestType = typeof(TRequest).Name;
            var timer = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation(
                    "Starting validation for request {RequestType}\nRequest details: {@Request}",
                    requestType,
                    request);

                var context = new ValidationContext<TRequest>(request);

                var validationResults = await Task.WhenAll(
                    _validators.Select(validator =>
                        validator.ValidateAsync(context, cancellationToken)));

                var failures = validationResults
                    .SelectMany(result => result.Errors)
                    .Where(failure => failure != null)
                    .GroupBy(
                        failure => failure.PropertyName,
                        failure => failure.ErrorMessage)
                    .ToDictionary(
                        group => group.Key,
                        group => group.ToArray());

                if (failures.Any())
                {
                    var errorDetails = FormatValidationErrors(failures);

                    _logger.LogWarning(
                        "Validation errors detected for request {RequestType}\n{ValidationErrors}",
                        requestType,
                        errorDetails);

                    throw new ValidationException(
                        "Validation errors occurred. See exception details.",
                        validationResults.SelectMany(r => r.Errors));
                }

                timer.Stop();
                _logger.LogInformation(
                    "Validation completed successfully for request {RequestType} (elapsed: {ElapsedMilliseconds}ms)",
                    requestType,
                    timer.ElapsedMilliseconds);

                return await next();
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error occurred during validation of request {RequestType}",
                    requestType);
                throw;
            }
        }

        private static string FormatValidationErrors(
            Dictionary<string, string[]> failures)
        {
            return string.Join("\n", failures.Select(failure =>
                $"Property: {failure.Key}\n" +
                $"Errors:\n{string.Join("\n", failure.Value.Select(error => $"- {error}"))}"));
        }
    }
}
