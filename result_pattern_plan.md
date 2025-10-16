Result Pattern w kontekście Twojego projektu Orbito
Dobra wiadomość!
Twoja aplikacja już częściowo wykorzystuje Result Pattern, szczególnie w Transaction Management. Widzę, że masz już implementację dla operacji transakcyjnych, ale warto ją rozszerzyć na całą aplikację dla większej spójności.
Czy warto implementować teraz?
TAK, zdecydowanie warto! Twoja aplikacja jest idealna do Result Pattern, ponieważ:

Już masz Clean Architecture - Result Pattern doskonale się w nią wpisuje
Używasz CQRS z MediatR - Handlers mogą zwracać Result<T> zamiast rzucać wyjątkami
Multi-tenancy wymaga precyzyjnej obsługi błędów - np. brak dostępu do tenanta, cross-tenant security
Masz już Value Objects - Result Pattern jest podobną koncepcją
Payment processing - tutaj Result Pattern jest niemal obowiązkowy (niepowodzenia płatności to normalna część biznesu, nie wyjątki)

Implementacja w Orbito

1. Utwórz Result w Domain Layer
   csharp// Orbito.Domain/Common/Result.cs
   namespace Orbito.Domain.Common;

public class Result
{
public bool IsSuccess { get; }
public bool IsFailure => !IsSuccess;
public Error Error { get; }

    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
            throw new InvalidOperationException();
        if (!isSuccess && error == Error.None)
            throw new InvalidOperationException();

        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);

    public static Result<TValue> Success<TValue>(TValue value) =>
        new(value, true, Error.None);

    public static Result<TValue> Failure<TValue>(Error error) =>
        new(default, false, error);

}

public class Result<TValue> : Result
{
private readonly TValue? \_value;

    protected internal Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access value of failed result");

    public static implicit operator Result<TValue>(TValue value) =>
        Success(value);

} 2. Utwórz Error Value Object
csharp// Orbito.Domain/Common/Error.cs
namespace Orbito.Domain.Common;

public sealed record Error
{
public static readonly Error None = new(string.Empty, string.Empty);
public static readonly Error NullValue = new("Error.NullValue", "Null value was provided");

    public string Code { get; }
    public string Message { get; }

    private Error(string code, string message)
    {
        Code = code;
        Message = message;
    }

    public static Error Create(string code, string message) => new(code, message);

    public static implicit operator string(Error error) => error.Code;

} 3. Zdefiniuj Domain Errors
csharp// Orbito.Domain/Errors/DomainErrors.cs
namespace Orbito.Domain.Errors;

public static class DomainErrors
{
public static class Tenant
{
public static Error NotFound => Error.Create(
"Tenant.NotFound",
"Tenant nie został znaleziony");

        public static Error SubdomainAlreadyExists => Error.Create(
            "Tenant.SubdomainAlreadyExists",
            "Subdomena jest już zajęta");

        public static Error NoTenantContext => Error.Create(
            "Tenant.NoTenantContext",
            "Kontekst tenanta nie jest dostępny");
    }

    public static class Payment
    {
        public static Error ProcessingFailed => Error.Create(
            "Payment.ProcessingFailed",
            "Przetwarzanie płatności nie powiodło się");

        public static Error InvalidAmount => Error.Create(
            "Payment.InvalidAmount",
            "Nieprawidłowa kwota płatności");

        public static Error DuplicateIdempotencyKey => Error.Create(
            "Payment.DuplicateIdempotencyKey",
            "Płatność z tym kluczem idempotencji już istnieje");
    }

    public static class Subscription
    {
        public static Error NotFound => Error.Create(
            "Subscription.NotFound",
            "Subskrypcja nie została znaleziona");

        public static Error CannotBeCancelled => Error.Create(
            "Subscription.CannotBeCancelled",
            "Subskrypcja nie może zostać anulowana w obecnym stanie");

        public static Error AlreadyActive => Error.Create(
            "Subscription.AlreadyActive",
            "Subskrypcja jest już aktywna");
    }

    public static class Client
    {
        public static Error NotFound => Error.Create(
            "Client.NotFound",
            "Klient nie został znaleziony");

        public static Error EmailAlreadyExists => Error.Create(
            "Client.EmailAlreadyExists",
            "Klient z tym adresem email już istnieje");
    }

} 4. Zaktualizuj CQRS Handlers
csharp// Przykład: Orbito.Application/Subscriptions/Commands/CancelSubscription/CancelSubscriptionCommandHandler.cs
public class CancelSubscriptionCommandHandler
: IRequestHandler<CancelSubscriptionCommand, Result<SubscriptionDto>>
{
private readonly IUnitOfWork \_unitOfWork;
private readonly ITenantProvider \_tenantProvider;

    public CancelSubscriptionCommandHandler(
        IUnitOfWork unitOfWork,
        ITenantProvider tenantProvider)
    {
        _unitOfWork = unitOfWork;
        _tenantProvider = tenantProvider;
    }

    public async Task<Result<SubscriptionDto>> Handle(
        CancelSubscriptionCommand request,
        CancellationToken cancellationToken)
    {
        // Walidacja tenanta
        if (!_tenantProvider.HasTenant())
            return Result.Failure<SubscriptionDto>(DomainErrors.Tenant.NoTenantContext);

        // Pobranie subskrypcji
        var subscription = await _unitOfWork.SubscriptionRepository
            .GetByIdAsync(request.SubscriptionId, cancellationToken);

        if (subscription is null)
            return Result.Failure<SubscriptionDto>(DomainErrors.Subscription.NotFound);

        // Biznesowa walidacja
        if (!subscription.CanBeCancelled())
            return Result.Failure<SubscriptionDto>(DomainErrors.Subscription.CannotBeCancelled);

        // Operacja domenowa
        subscription.Cancel();

        await _unitOfWork.SubscriptionRepository.UpdateAsync(subscription, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = subscription.ToDto();
        return Result.Success(dto);
    }

} 5. Zaktualizuj BaseController
csharp// Orbito.API/Controllers/BaseController.cs
public abstract class BaseController : ControllerBase
{
protected IActionResult HandleResult<T>(Result<T> result)
{
if (result.IsSuccess)
return Ok(result.Value);

        return result.Error.Code switch
        {
            var code when code.Contains("NotFound") => NotFound(new ErrorResponse
            {
                Error = result.Error.Code,
                Message = result.Error.Message
            }),
            var code when code.Contains("AlreadyExists") => Conflict(new ErrorResponse
            {
                Error = result.Error.Code,
                Message = result.Error.Message
            }),
            var code when code.Contains("Invalid") => BadRequest(new ErrorResponse
            {
                Error = result.Error.Code,
                Message = result.Error.Message
            }),
            _ => BadRequest(new ErrorResponse
            {
                Error = result.Error.Code,
                Message = result.Error.Message
            })
        };
    }

    protected IActionResult HandleResult(Result result)
    {
        if (result.IsSuccess)
            return Ok();

        return result.Error.Code switch
        {
            var code when code.Contains("NotFound") => NotFound(new ErrorResponse
            {
                Error = result.Error.Code,
                Message = result.Error.Message
            }),
            var code when code.Contains("AlreadyExists") => Conflict(new ErrorResponse
            {
                Error = result.Error.Code,
                Message = result.Error.Message
            }),
            _ => BadRequest(new ErrorResponse
            {
                Error = result.Error.Code,
                Message = result.Error.Message
            })
        };
    }

} 6. Przykład użycia w Controllerze
csharp// Orbito.API/Controllers/SubscriptionsController.cs
[Route("api/[controller]")]
[ApiController]
public class SubscriptionsController : BaseController
{
private readonly ISender \_sender;

    public SubscriptionsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelSubscription(Guid id)
    {
        var command = new CancelSubscriptionCommand { SubscriptionId = id };
        var result = await _sender.Send(command);

        return HandleResult(result);
    }

}
Plan migracji
Krok 1: Podstawowa infrastruktura (1-2 dni)

Utwórz Result i Error w Domain layer
Utwórz DomainErrors z podstawowymi błędami
Zaktualizuj BaseController o metody HandleResult

Krok 2: Krytyczne miejsca (2-3 dni)

Payment processing - tutaj priorytet!
Subscription management
Multi-tenant operations

Krok 3: Stopniowa migracja (ongoing)

CRUD operations dla Provider
Client management
Pozostałe handlery

Korzyści dla Orbito

Bezpieczeństwo - Cross-tenant errors będą obsłużone jawnie
Payment processing - Niepowodzenia płatności to część biznesu, nie wyjątki
API responses - Spójne odpowiedzi błędów (już masz ErrorResponse)
Testowanie - Łatwiejsze mockowanie i testowanie scenariuszy błędów
Performance - Brak kosztownego stack unwinding przy wyjątkach
Clean Architecture - Result Pattern doskonale pasuje do DDD
