using MediatR;
using Orbito.Application.Common.Models;
using Orbito.Application.Features.Payments.Queries.DTOs;

namespace Orbito.Application.Features.Payments.Queries;

/// <summary>
/// Query to get failed payments that can be retried.
/// </summary>
public class GetFailedPaymentsForRetryQuery : IRequest<PaginatedList<FailedPaymentDto>>
{
    /// <summary>
    /// Client ID to filter payments (optional).
    /// </summary>
    public Guid? ClientId { get; set; }

    /// <summary>
    /// Pagination parameters.
    /// </summary>
    public PaginationParams Pagination { get; set; } = new();
}
