using MediatR;
using Orbito.Domain.Common;

namespace Orbito.Application.Features.Payments.Commands.ProcessWebhookEvent;

/// <summary>
/// Command for processing webhook events
/// </summary>
public record ProcessWebhookEventCommand : IRequest<Result>
{
    /// <summary>
    /// Event type from webhook
    /// </summary>
    public required string EventType { get; init; }

    /// <summary>
    /// Event ID from webhook
    /// </summary>
    public required string EventId { get; init; }

    /// <summary>
    /// Raw webhook payload
    /// </summary>
    public required string Payload { get; init; }

    /// <summary>
    /// Webhook signature for verification
    /// </summary>
    public required string Signature { get; init; }

    /// <summary>
    /// Payment gateway provider
    /// </summary>
    public required string Provider { get; init; }
}
