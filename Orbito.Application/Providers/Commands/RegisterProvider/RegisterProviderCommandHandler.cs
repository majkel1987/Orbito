using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Providers.Commands.CreateProvider;
using Orbito.Domain.Common;
using Orbito.Domain.Errors;
using Orbito.Domain.Identity;

namespace Orbito.Application.Providers.Commands.RegisterProvider;

public class RegisterProviderCommandHandler : IRequestHandler<RegisterProviderCommand, Result<RegisterProviderResult>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMediator _mediator;
    private readonly ILogger<RegisterProviderCommandHandler> _logger;

    public RegisterProviderCommandHandler(
        UserManager<ApplicationUser> userManager,
        IMediator mediator,
        ILogger<RegisterProviderCommandHandler> logger)
    {
        _userManager = userManager;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<Result<RegisterProviderResult>> Handle(RegisterProviderCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return Result.Failure<RegisterProviderResult>(DomainErrors.User.EmailAlreadyExists);
        }

        var subdomainCheck = await _mediator.Send(new CheckSubdomainAvailabilityQuery(request.SubdomainSlug), cancellationToken);
        if (!subdomainCheck.IsAvailable)
        {
            return Result.Failure<RegisterProviderResult>(DomainErrors.Provider.SubdomainAlreadyExists);
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            TenantId = null,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var createUserResult = await _userManager.CreateAsync(user, request.Password);
        if (!createUserResult.Succeeded)
        {
            var errorMessages = string.Join("; ", createUserResult.Errors.Select(e => e.Description));
            _logger.LogWarning("Failed to create user for provider registration: {Errors}", errorMessages);
            return Result.Failure<RegisterProviderResult>(DomainErrors.User.CreationFailed);
        }

        var createProviderCommand = new CreateProviderCommand(
            user.Id,
            request.BusinessName,
            request.SubdomainSlug,
            request.Email,
            request.FirstName,
            request.LastName,
            request.SelectedPlatformPlanId,
            request.Description,
            request.Avatar,
            request.CustomDomain);

        var createProviderResult = await _mediator.Send(createProviderCommand, cancellationToken);

        if (createProviderResult.IsFailure)
        {
            await _userManager.DeleteAsync(user);
            _logger.LogWarning("Provider creation failed, user rolled back: {Email}", request.Email);
            return Result.Failure<RegisterProviderResult>(createProviderResult.Error);
        }

        _logger.LogInformation("Provider registered: {Email} (ProviderId: {ProviderId})",
            request.Email, createProviderResult.Value.ProviderId);

        return Result.Success(new RegisterProviderResult(
            user.Id,
            createProviderResult.Value.ProviderId,
            createProviderResult.Value.TenantId,
            request.BusinessName,
            request.SubdomainSlug));
    }
}

/// <summary>
/// Query to check if a subdomain is available for registration.
/// </summary>
public record CheckSubdomainAvailabilityQuery(string SubdomainSlug) : IRequest<SubdomainAvailabilityResult>;

/// <summary>
/// Result of subdomain availability check.
/// </summary>
public record SubdomainAvailabilityResult(bool IsAvailable, string? Message = null);

public class CheckSubdomainAvailabilityQueryHandler : IRequestHandler<CheckSubdomainAvailabilityQuery, SubdomainAvailabilityResult>
{
    private readonly IProviderRepository _providerRepository;
    private readonly ILogger<CheckSubdomainAvailabilityQueryHandler> _logger;

    public CheckSubdomainAvailabilityQueryHandler(
        IProviderRepository providerRepository,
        ILogger<CheckSubdomainAvailabilityQueryHandler> logger)
    {
        _providerRepository = providerRepository;
        _logger = logger;
    }

    public async Task<SubdomainAvailabilityResult> Handle(CheckSubdomainAvailabilityQuery request, CancellationToken cancellationToken)
    {
        var existingProvider = await _providerRepository.GetBySubdomainSlugAsync(request.SubdomainSlug, cancellationToken);

        if (existingProvider != null)
        {
            return new SubdomainAvailabilityResult(false, "Subdomain is already in use");
        }

        return new SubdomainAvailabilityResult(true, "Subdomain is available");
    }
}
