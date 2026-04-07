using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Common;
using Orbito.Domain.Entities;
using Orbito.Domain.Errors;
using Orbito.Domain.ValueObjects;

namespace Orbito.Application.Clients.Commands.InviteClient;

public class InviteClientCommandHandler : IRequestHandler<InviteClientCommand, Result<Guid>>
{
    private readonly IClientRepository _clientRepository;
    private readonly IProviderRepository _providerRepository;
    private readonly ITenantContext _tenantContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<InviteClientCommandHandler> _logger;

    public InviteClientCommandHandler(
        IClientRepository clientRepository,
        IProviderRepository providerRepository,
        ITenantContext tenantContext,
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        IConfiguration configuration,
        ILogger<InviteClientCommandHandler> logger)
    {
        _clientRepository = clientRepository;
        _providerRepository = providerRepository;
        _tenantContext = tenantContext;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(InviteClientCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.HasTenant)
            return Result.Failure<Guid>(DomainErrors.Tenant.NoTenantContext);

        var tenantId = _tenantContext.CurrentTenantId!;

        var provider = await _providerRepository.GetByIdAsync(tenantId.Value, cancellationToken);
        if (provider == null)
            return Result.Failure<Guid>(DomainErrors.Provider.NotFound);

        var clientExists = await _clientRepository.ExistsAsync(request.Email, cancellationToken);
        if (clientExists)
            return Result.Failure<Guid>(DomainErrors.Client.EmailAlreadyExists);

        var invitationToken = ClientInvitationToken.Create(TimeSpan.FromDays(7));

        var client = Client.CreateInvited(
            tenantId,
            request.Email,
            request.FirstName,
            request.LastName,
            invitationToken.Token,
            invitationToken.ExpiresAt,
            request.CompanyName);

        var setProviderResult = client.SetProvider(provider);
        if (setProviderResult.IsFailure)
            return Result.Failure<Guid>(setProviderResult.Error);

        // IMPORTANT: Save client FIRST, then send email
        // This ensures data consistency - if email fails, client exists and we can retry
        // If we sent email first and DB failed, client would have link but no record
        await _clientRepository.AddAsync(client, cancellationToken);

        var baseUrl = _configuration["App:FrontendBaseUrl"] ?? "http://localhost:3000";
        var invitationLink = $"{baseUrl}/portal/confirm?token={invitationToken.Token}";

        _logger.LogInformation(
            "Sending invitation email to {Email} for tenant {TenantId}",
            request.Email,
            tenantId.Value);

        var emailResult = await _emailService.SendClientInvitationAsync(
            request.Email,
            $"{request.FirstName} {request.LastName}".Trim(),
            provider.BusinessName,
            invitationLink,
            cancellationToken);

        if (emailResult.IsFailure)
        {
            // Client is saved but email failed - log warning but don't fail the operation
            // Admin can use ResendClientInvitation to retry
            _logger.LogWarning(
                "Client {ClientId} created but failed to send invitation email to {Email}: {Error}. Use ResendClientInvitation to retry.",
                client.Id,
                request.Email,
                emailResult.Error.Message);
        }

        return Result.Success(client.Id);
    }
}
