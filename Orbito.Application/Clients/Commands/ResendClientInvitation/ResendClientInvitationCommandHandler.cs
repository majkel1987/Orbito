using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Common;
using Orbito.Domain.Errors;
using Orbito.Domain.ValueObjects;

namespace Orbito.Application.Clients.Commands.ResendClientInvitation;

public class ResendClientInvitationCommandHandler : IRequestHandler<ResendClientInvitationCommand, Result>
{
    private readonly IClientRepository _clientRepository;
    private readonly IProviderRepository _providerRepository;
    private readonly ITenantContext _tenantContext;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ResendClientInvitationCommandHandler> _logger;

    public ResendClientInvitationCommandHandler(
        IClientRepository clientRepository,
        IProviderRepository providerRepository,
        ITenantContext tenantContext,
        IEmailService emailService,
        IConfiguration configuration,
        ILogger<ResendClientInvitationCommandHandler> logger)
    {
        _clientRepository = clientRepository;
        _providerRepository = providerRepository;
        _tenantContext = tenantContext;
        _emailService = emailService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Result> Handle(ResendClientInvitationCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.HasTenant)
            return Result.Failure(DomainErrors.Tenant.NoTenantContext);

        var tenantId = _tenantContext.CurrentTenantId!;

        var client = await _clientRepository.GetByIdAsync(request.ClientId, cancellationToken);
        if (client is null)
            return Result.Failure(DomainErrors.Client.NotFound);

        if (client.Status == Domain.Enums.ClientStatus.Active)
            return Result.Failure(DomainErrors.Client.AlreadyConfirmed);

        var provider = await _providerRepository.GetByIdAsync(tenantId.Value, cancellationToken);
        if (provider is null)
            return Result.Failure(DomainErrors.Provider.NotFound);

        var newToken = ClientInvitationToken.Create(TimeSpan.FromDays(7));

        var regenerateResult = client.RegenerateInvitationToken(newToken.Token, newToken.ExpiresAt);
        if (regenerateResult.IsFailure)
            return regenerateResult;

        // IMPORTANT: Save token FIRST, then send email
        // This ensures data consistency - the new token is persisted before email
        await _clientRepository.UpdateAsync(client, cancellationToken);

        var baseUrl = _configuration["App:FrontendBaseUrl"] ?? "http://localhost:3000";
        var invitationLink = $"{baseUrl}/portal/confirm?token={newToken.Token}";

        var emailResult = await _emailService.SendClientInvitationAsync(
            client.DirectEmail ?? string.Empty,
            client.FullName,
            provider.BusinessName,
            invitationLink,
            cancellationToken);

        if (emailResult.IsFailure)
        {
            // Token is saved but email failed - log warning but don't fail
            // The operation can be retried
            _logger.LogWarning(
                "Token regenerated for client {ClientId} but email send failed: {Error}. Retry the operation.",
                client.Id, emailResult.Error.Message);
        }

        _logger.LogInformation(
            "Resent invitation to client {ClientId} for tenant {TenantId}",
            client.Id, tenantId.Value);

        return Result.Success();
    }
}
