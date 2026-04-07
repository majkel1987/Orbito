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

    Task<Result> SendPaymentConfirmationAsync(
        string toEmail,
        string clientName,
        string subscriptionName,
        decimal amount,
        string currency,
        string paymentId,
        DateTime paymentDate,
        CancellationToken cancellationToken = default);

    Task<Result> SendPaymentFailedAsync(
        string toEmail,
        string clientName,
        string subscriptionName,
        decimal amount,
        string currency,
        string failureReason,
        CancellationToken cancellationToken = default);

    Task<Result> SendTrialExpiringAsync(
        string toEmail,
        string providerName,
        int daysRemaining,
        string planName,
        string billingLink,
        CancellationToken cancellationToken = default);

    Task<Result> SendTrialExpiredAsync(
        string toEmail,
        string providerName,
        string planName,
        string billingLink,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send team member invitation email with link to accept the invitation.
    /// </summary>
    /// <param name="toEmail">Email address of the invited team member.</param>
    /// <param name="inviteeName">Name of the person being invited.</param>
    /// <param name="providerName">Name of the provider organization.</param>
    /// <param name="inviterName">Name of the person sending the invitation.</param>
    /// <param name="roleName">Role being assigned to the team member.</param>
    /// <param name="invitationLink">Link to accept the invitation.</param>
    /// <param name="personalMessage">Optional personal message from the inviter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<Result> SendTeamMemberInvitationAsync(
        string toEmail,
        string inviteeName,
        string providerName,
        string inviterName,
        string roleName,
        string invitationLink,
        string? personalMessage,
        CancellationToken cancellationToken = default);
}
