using Orbito.Application.DTOs;

namespace Orbito.Application.Features.Payments.Queries.GetPaymentsBySubscription
{
    public record GetPaymentsBySubscriptionResult
    {
        public bool Success { get; init; }
        public string? Message { get; init; }
        public IEnumerable<PaymentDto> Payments { get; init; } = [];
        public int TotalCount { get; init; }
        public int PageNumber { get; init; }
        public int PageSize { get; init; }
        public int TotalPages { get; init; }

        public static GetPaymentsBySubscriptionResult SuccessResult(
            IEnumerable<PaymentDto> payments,
            int totalCount,
            int pageNumber,
            int pageSize)
        {
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return new GetPaymentsBySubscriptionResult
            {
                Success = true,
                Payments = payments,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }

        public static GetPaymentsBySubscriptionResult FailureResult(string message)
        {
            return new GetPaymentsBySubscriptionResult
            {
                Success = false,
                Message = message
            };
        }
    }
}
