using MediatR;

namespace Orbito.Domain.Common;

/// <summary>
/// Marker interface for domain events.
/// Implements MediatR INotification to enable event dispatching through the pipeline.
/// </summary>
public interface IDomainEvent : INotification
{
    /// <summary>
    /// Unique identifier for this event instance
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// UTC timestamp when this event occurred
    /// </summary>
    DateTime OccurredOn { get; }
}
