using MediatR;
using Orbito.Domain.Common;

namespace Orbito.Application.Features.ProviderSubscriptions.Commands.SendTrialExpirationNotifications;

/// <summary>
/// Command to send trial expiration notification emails to providers.
/// Executed by background job every hour.
/// </summary>
public record SendTrialExpirationNotificationsCommand : IRequest<Result<int>>;
