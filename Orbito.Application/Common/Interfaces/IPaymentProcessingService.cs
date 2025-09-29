using Orbito.Domain.ValueObjects;
using Orbito.Application.Common.Models.PaymentGateway;

namespace Orbito.Application.Common.Interfaces
{
    /// <summary>
    /// Interfejs serwisu do przetwarzania płatności
    /// </summary>
    public interface IPaymentProcessingService
    {
        /// <summary>
        /// Przetwarza płatność subskrypcji
        /// </summary>
        /// <param name="subscriptionId">ID subskrypcji</param>
        /// <param name="amount">Kwota płatności</param>
        /// <param name="paymentMethodId">ID metody płatności</param>
        /// <param name="description">Opis płatności</param>
        /// <param name="cancellationToken">Token anulowania</param>
        /// <returns>Wynik przetwarzania płatności</returns>
        Task<PaymentResult> ProcessSubscriptionPaymentAsync(
            Guid subscriptionId, 
            Money amount, 
            string paymentMethodId,
            string description,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obsługuje udaną płatność
        /// </summary>
        /// <param name="paymentId">ID płatności</param>
        /// <param name="cancellationToken">Token anulowania</param>
        Task HandlePaymentSuccessAsync(Guid paymentId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obsługuje nieudaną płatność
        /// </summary>
        /// <param name="paymentId">ID płatności</param>
        /// <param name="reason">Powód niepowodzenia</param>
        /// <param name="cancellationToken">Token anulowania</param>
        Task HandlePaymentFailureAsync(Guid paymentId, string reason, CancellationToken cancellationToken = default);

        /// <summary>
        /// Zwraca płatność
        /// </summary>
        /// <param name="paymentId">ID płatności</param>
        /// <param name="amount">Kwota zwrotu</param>
        /// <param name="reason">Powód zwrotu</param>
        /// <param name="cancellationToken">Token anulowania</param>
        /// <returns>Wynik zwrotu płatności</returns>
        Task<RefundResult> RefundPaymentAsync(
            Guid paymentId, 
            Money amount, 
            string reason,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Tworzy klienta w Stripe
        /// </summary>
        /// <param name="clientId">ID klienta</param>
        /// <param name="email">Email klienta</param>
        /// <param name="firstName">Imię klienta</param>
        /// <param name="lastName">Nazwisko klienta</param>
        /// <param name="companyName">Nazwa firmy</param>
        /// <param name="phone">Telefon klienta</param>
        /// <param name="cancellationToken">Token anulowania</param>
        /// <returns>Wynik tworzenia klienta</returns>
        Task<CustomerResult> CreateStripeCustomerAsync(
            Guid clientId,
            string email,
            string? firstName = null,
            string? lastName = null,
            string? companyName = null,
            string? phone = null,
            CancellationToken cancellationToken = default);
    }
}
