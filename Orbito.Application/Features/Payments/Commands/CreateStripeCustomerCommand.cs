using MediatR;
using Orbito.Domain.Common;

namespace Orbito.Application.Features.Payments.Commands;

/// <summary>
/// Command for creating a Stripe customer
/// </summary>
public record CreateStripeCustomerCommand : IRequest<Result<CreateStripeCustomerResult>>
{
    /// <summary>
    /// Client ID
    /// </summary>
    public Guid ClientId { get; init; }

    /// <summary>
    /// Client email address
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Client first name
    /// </summary>
    public string? FirstName { get; init; }

    /// <summary>
    /// Client last name
    /// </summary>
    public string? LastName { get; init; }

    /// <summary>
    /// Company name
    /// </summary>
    public string? CompanyName { get; init; }

    /// <summary>
    /// Client phone number
    /// </summary>
    public string? Phone { get; init; }
}

/// <summary>
/// Result of creating a Stripe customer
/// </summary>
public record CreateStripeCustomerResult
{
    /// <summary>
    /// External Stripe customer ID
    /// </summary>
    public required string StripeCustomerId { get; init; }

    /// <summary>
    /// Client email address
    /// </summary>
    public string? Email { get; init; }

    /// <summary>
    /// Client first name
    /// </summary>
    public string? FirstName { get; init; }

    /// <summary>
    /// Client last name
    /// </summary>
    public string? LastName { get; init; }
}
