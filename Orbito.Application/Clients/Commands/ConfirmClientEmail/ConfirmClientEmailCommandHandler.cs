using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Common;
using Orbito.Domain.Enums;
using Orbito.Domain.Errors;
using Orbito.Domain.Identity;
using Orbito.Domain.ValueObjects;

namespace Orbito.Application.Clients.Commands.ConfirmClientEmail;

/// <summary>
/// Handles client email confirmation via invitation token.
/// This is a PUBLIC endpoint (no auth required) - client confirms by token from email.
/// Security: Token-based validation only; tenant context is NOT required as this is a public operation.
/// The token itself provides the security - it's cryptographically random and expires.
/// </summary>
public class ConfirmClientEmailCommandHandler : IRequestHandler<ConfirmClientEmailCommand, Result>
{
    private readonly IClientRepository _clientRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ConfirmClientEmailCommandHandler> _logger;

    public ConfirmClientEmailCommandHandler(
        IClientRepository clientRepository,
        UserManager<ApplicationUser> userManager,
        ILogger<ConfirmClientEmailCommandHandler> logger)
    {
        _clientRepository = clientRepository;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<Result> Handle(ConfirmClientEmailCommand request, CancellationToken cancellationToken)
    {
        // Note: This is intentionally a PUBLIC endpoint (no tenant context required).
        // Security is provided by the cryptographically random token.
        // Logging token hash for security audit (never log full token).
        var tokenHash = request.Token.Length > 8 ? request.Token[..8] + "..." : "***";
        _logger.LogInformation("Processing email confirmation for token {TokenHash}", tokenHash);

        var client = await _clientRepository.GetByInvitationTokenAsync(request.Token, cancellationToken);
        if (client is null)
            return Result.Failure(DomainErrors.Client.InvalidToken);

        var confirmResult = client.ConfirmEmail();
        if (confirmResult.IsFailure)
            return confirmResult;

        var clientEmail = client.DirectEmail ?? string.Empty;
        var firstName = client.DirectFirstName ?? string.Empty;
        var lastName = client.DirectLastName ?? string.Empty;

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = clientEmail,
            Email = clientEmail,
            EmailConfirmed = true,
            FirstName = firstName,
            LastName = lastName,
            TenantId = client.TenantId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            _logger.LogWarning("Failed to create Identity account for client {ClientId}: {Errors}", client.Id, errors);
            return Result.Failure("Identity.CreateFailed", errors);
        }

        var roleResult = await _userManager.AddToRoleAsync(user, UserRole.Client.ToString());
        if (!roleResult.Succeeded)
        {
            var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
            _logger.LogWarning("Failed to assign Client role for user {UserId}: {Errors}", user.Id, errors);
            await _userManager.DeleteAsync(user);
            return Result.Failure("Identity.RoleAssignFailed", errors);
        }

        client.SetUserId(user.Id);

        await _clientRepository.UpdateAsync(client, cancellationToken);

        _logger.LogInformation(
            "Client {ClientId} confirmed email and created Identity account {UserId}",
            client.Id, user.Id);

        return Result.Success();
    }
}
