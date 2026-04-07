using MediatR;
using Orbito.Domain.Common;
using Orbito.Domain.Enums;

namespace Orbito.Application.Features.Payments.Commands.UpdatePaymentFromWebhook;

/// <summary>
/// Command for updating payment from webhook data
/// </summary>
public record UpdatePaymentFromWebhookCommand : IRequest<Result>
{
    /// <summary>
    /// Payment ID to update
    /// </summary>
    public required Guid PaymentId { get; init; }

    /// <summary>
    /// Webhook event ID for idempotency
    /// </summary>
    public required string EventId { get; init; }

    /// <summary>
    /// Webhook event type
    /// </summary>
    public required string EventType { get; init; }

    /// <summary>
    /// Webhook payload data
    /// </summary>
    public required string Payload { get; init; }

    /// <summary>
    /// External payment ID from webhook
    /// </summary>
    public required string ExternalPaymentId { get; init; }

    /// <summary>
    /// New payment status from webhook
    /// </summary>
    public required PaymentStatus NewStatus { get; init; }

    /// <summary>
    /// Error message if payment failed
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Additional metadata from webhook
    /// </summary>
    public Dictionary<string, string> Metadata { get; init; } = new();
}
