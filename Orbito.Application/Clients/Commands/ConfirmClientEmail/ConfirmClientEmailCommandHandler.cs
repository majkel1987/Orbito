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

        client.UserId = user.Id;

        await _clientRepository.UpdateAsync(client, cancellationToken);

        _logger.LogInformation(
            "Client {ClientId} confirmed email and created Identity account {UserId}",
            client.Id, user.Id);

        return Result.Success();
    }
}
