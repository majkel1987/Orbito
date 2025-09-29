using Orbito.Application.Common.Models.PaymentGateway;

namespace Orbito.Application.Common.Interfaces
{
    /// <summary>
    /// Abstrakcja dla payment gateway - umożliwia łatwe przełączanie między różnymi dostawcami płatności
    /// </summary>
    public interface IPaymentGateway
    {
        /// <summary>
        /// Przetwarza płatność przez payment gateway
        /// </summary>
        /// <param name="request">Dane płatności</param>
        /// <returns>Wynik przetwarzania płatności</returns>
        Task<PaymentResult> ProcessPaymentAsync(ProcessPaymentRequest request);

        /// <summary>
        /// Zwraca płatność przez payment gateway
        /// </summary>
        /// <param name="request">Dane zwrotu</param>
        /// <returns>Wynik zwrotu płatności</returns>
        Task<RefundResult> RefundPaymentAsync(RefundRequest request);

        /// <summary>
        /// Tworzy klienta w payment gateway
        /// </summary>
        /// <param name="request">Dane klienta</param>
        /// <returns>Wynik tworzenia klienta</returns>
        Task<CustomerResult> CreateCustomerAsync(CreateCustomerRequest request);

        /// <summary>
        /// Sprawdza status płatności w payment gateway
        /// </summary>
        /// <param name="externalPaymentId">Zewnętrzny ID płatności</param>
        /// <returns>Status płatności</returns>
        Task<PaymentStatusResult> GetPaymentStatusAsync(string externalPaymentId);

        /// <summary>
        /// Waliduje webhook od payment gateway
        /// </summary>
        /// <param name="payload">Dane webhook</param>
        /// <param name="signature">Podpis webhook</param>
        /// <returns>Wynik walidacji webhook z szczegółami</returns>
        Task<WebhookValidationResult> ValidateWebhookAsync(string payload, string signature);
    }
}
