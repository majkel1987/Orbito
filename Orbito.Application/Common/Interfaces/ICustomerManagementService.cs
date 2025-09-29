using Orbito.Application.Common.Models;
using Orbito.Application.Common.Models.PaymentGateway;

namespace Orbito.Application.Common.Interfaces
{
    /// <summary>
    /// Interface for customer management (removed Stripe-specific naming)
    /// </summary>
    public interface ICustomerManagementService
    {
        /// <summary>
        /// Create a customer in the payment gateway
        /// </summary>
        /// <param name="clientId">Client ID</param>
        /// <param name="email">Customer email</param>
        /// <param name="firstName">First name</param>
        /// <param name="lastName">Last name</param>
        /// <param name="companyName">Company name</param>
        /// <param name="phone">Phone number</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Customer creation result</returns>
        Task<Result<CustomerResult>> CreateCustomerAsync(
            Guid clientId,
            string email,
            string? firstName = null,
            string? lastName = null,
            string? companyName = null,
            string? phone = null,
            CancellationToken cancellationToken = default);
    }
}
