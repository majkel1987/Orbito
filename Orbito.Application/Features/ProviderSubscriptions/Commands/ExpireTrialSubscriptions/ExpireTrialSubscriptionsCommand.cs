using MediatR;
using Orbito.Domain.Common;

namespace Orbito.Application.Features.ProviderSubscriptions.Commands.ExpireTrialSubscriptions;

/// <summary>
/// Command to expire all trial subscriptions that have passed their trial end date.
/// Changes status from Trial to Expired and sends notification emails.
/// </summary>
public record ExpireTrialSubscriptionsCommand : IRequest<Result<int>>;
