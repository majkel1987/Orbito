using MediatR;
using Orbito.Domain.Common;

namespace Orbito.Application.Features.Payments.Commands;

/// <summary>
/// Command to cancel a scheduled retry
/// </summary>
public record CancelRetryCommand : IRequest<Result<CancelRetryResponse>>
{
    /// <summary>
    /// ID of the retry schedule to cancel
    /// </summary>
    public required Guid ScheduleId { get; init; }

    /// <summary>
    /// ID of the client requesting the cancellation
    /// </summary>
    public required Guid ClientId { get; init; }
}

/// <summary>
/// Response for cancel retry command
/// </summary>
public record CancelRetryResponse
{
    /// <summary>
    /// ID of the cancelled retry schedule
    /// </summary>
    public required Guid ScheduleId { get; init; }
}
