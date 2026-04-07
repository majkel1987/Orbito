using MediatR;
using Orbito.Application.Common.Models;
using Orbito.Application.Features.Payments.Queries.DTOs;

namespace Orbito.Application.Features.Payments.Queries;

/// <summary>
/// Query to get scheduled retries with filtering and pagination.
/// </summary>
public class GetScheduledRetriesQuery : IRequest<PaginatedList<RetryScheduleDto>>
{
    /// <summary>
    /// Client ID to filter retries (optional).
    /// </summary>
    public Guid? ClientId { get; set; }

    /// <summary>
    /// Status to filter retries (optional).
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Pagination parameters.
    /// </summary>
    public PaginationParams Pagination { get; set; } = new();
}
