using Orbito.Application.DTOs;

namespace Orbito.Application.Features.Payments.Queries.GetAllPayments
{
    /// <summary>
    /// Response for GetAllPayments query
    /// </summary>
    public record GetAllPaymentsResponse
    {
        public required IEnumerable<PaymentDto> Payments { get; init; }
        public required int TotalCount { get; init; }
        public required int PageNumber { get; init; }
        public required int PageSize { get; init; }
        public required int TotalPages { get; init; }
    }
}
