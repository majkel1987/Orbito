using MediatR;
using Orbito.Application.Common.Interfaces;

namespace Orbito.Application.Features.Payments.Commands
{
    /// <summary>
    /// Komenda do tworzenia klienta Stripe
    /// </summary>
    public record CreateStripeCustomerCommand : IRequest<CreateStripeCustomerResult>
    {
        /// <summary>
        /// ID klienta
        /// </summary>
        public Guid ClientId { get; init; }

        /// <summary>
        /// Email klienta
        /// </summary>
        public string Email { get; init; } = string.Empty;

        /// <summary>
        /// Imię klienta
        /// </summary>
        public string? FirstName { get; init; }

        /// <summary>
        /// Nazwisko klienta
        /// </summary>
        public string? LastName { get; init; }

        /// <summary>
        /// Nazwa firmy
        /// </summary>
        public string? CompanyName { get; init; }

        /// <summary>
        /// Telefon klienta
        /// </summary>
        public string? Phone { get; init; }
    }

    /// <summary>
    /// Wynik tworzenia klienta Stripe
    /// </summary>
    public record CreateStripeCustomerResult
    {
        /// <summary>
        /// Czy operacja zakończyła się sukcesem
        /// </summary>
        public bool IsSuccess { get; init; }

        /// <summary>
        /// Komunikat błędu (jeśli wystąpił)
        /// </summary>
        public string? ErrorMessage { get; init; }

        /// <summary>
        /// Kod błędu (jeśli wystąpił)
        /// </summary>
        public string? ErrorCode { get; init; }

        /// <summary>
        /// Zewnętrzny ID klienta w Stripe
        /// </summary>
        public string? StripeCustomerId { get; init; }

        /// <summary>
        /// Email klienta
        /// </summary>
        public string? Email { get; init; }

        /// <summary>
        /// Imię klienta
        /// </summary>
        public string? FirstName { get; init; }

        /// <summary>
        /// Nazwisko klienta
        /// </summary>
        public string? LastName { get; init; }

        /// <summary>
        /// Konstruktor dla sukcesu
        /// </summary>
        public static CreateStripeCustomerResult Success(
            string stripeCustomerId,
            string? email = null,
            string? firstName = null,
            string? lastName = null)
        {
            return new CreateStripeCustomerResult
            {
                IsSuccess = true,
                StripeCustomerId = stripeCustomerId,
                Email = email,
                FirstName = firstName,
                LastName = lastName
            };
        }

        /// <summary>
        /// Konstruktor dla błędu
        /// </summary>
        public static CreateStripeCustomerResult Failure(string errorMessage, string? errorCode = null)
        {
            return new CreateStripeCustomerResult
            {
                IsSuccess = false,
                ErrorMessage = errorMessage,
                ErrorCode = errorCode
            };
        }
    }
}
