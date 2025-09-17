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
                    "Rozpoczęcie walidacji żądania {RequestType}\nSzczegóły żądania: {@Request}",
                    requestType,
                    request);

                var context = new ValidationContext<TRequest>(request);

                // Uruchamiamy wszystkie walidatory równolegle
                var validationResults = await Task.WhenAll(
                    _validators.Select(validator =>
                        validator.ValidateAsync(context, cancellationToken)));

                // Zbieramy wszystkie błędy walidacji
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
                    // Formatujemy szczegółowy komunikat o błędach
                    var errorDetails = FormatValidationErrors(failures);

                    _logger.LogWarning(
                        "Wykryto błędy walidacji dla żądania {RequestType}\n{ValidationErrors}",
                        requestType,
                        errorDetails);

                    throw new ValidationException(
                        "Wykryto błędy walidacji. Sprawdź szczegóły w komunikacie.",
                        validationResults.SelectMany(r => r.Errors));
                }

                timer.Stop();
                _logger.LogInformation(
                    "Walidacja zakończona sukcesem dla żądania {RequestType} (czas: {ElapsedMilliseconds}ms)",
                    requestType,
                    timer.ElapsedMilliseconds);

                // Jeśli walidacja przeszła pomyślnie, przekazujemy żądanie dalej
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
                    "Wystąpił nieoczekiwany błąd podczas walidacji żądania {RequestType}",
                    requestType);
                throw;
            }
        }

        // Formatowanie błędów walidacji do czytelnej postaci.
        private static string FormatValidationErrors(
            Dictionary<string, string[]> failures)
        {
            return string.Join("\n", failures.Select(failure =>
                $"Właściwość: {failure.Key}\n" +
                $"Błędy:\n{string.Join("\n", failure.Value.Select(error => $"- {error}"))}"));
        }
    }
}
