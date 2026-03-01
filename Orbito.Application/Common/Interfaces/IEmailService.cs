using Orbito.Domain.Common;

namespace Orbito.Application.Common.Interfaces;

public interface IEmailService
{
    Task<Result> SendClientInvitationAsync(
        string toEmail,
        string clientName,
        string providerName,
        string invitationLink,
        CancellationToken cancellationToken = default);
}
