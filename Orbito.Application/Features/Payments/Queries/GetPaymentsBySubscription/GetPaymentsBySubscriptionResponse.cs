using Orbito.Application.DTOs;

namespace Orbito.Application.Features.Payments.Queries.GetPaymentsBySubscription
{
    /// <summary>
    /// Response for GetPaymentsBySubscription query
    /// </summary>
    public record GetPaymentsBySubscriptionResponse
    {
        public required IEnumerable<PaymentDto> Payments { get; init; }
        public required int TotalCount { get; init; }
        public required int PageNumber { get; init; }
        public required int PageSize { get; init; }
        public required int TotalPages { get; init; }
    }
}

