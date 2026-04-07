using System.Text.RegularExpressions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Common;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.Errors;
using Orbito.Domain.Identity;
using Orbito.Domain.ValueObjects;

namespace Orbito.Application.Providers.Commands.CreateProvider;

public class CreateProviderCommandHandler : IRequestHandler<CreateProviderCommand, Result<CreateProviderResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProviderRepository _providerRepository;
    private readonly IPlatformPlanRepository _platformPlanRepository;
    private readonly IProviderSubscriptionRepository _providerSubscriptionRepository;
    private readonly IClientRepository _clientRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<CreateProviderCommandHandler> _logger;

    public CreateProviderCommandHandler(
        IUnitOfWork unitOfWork,
        IProviderRepository providerRepository,
        IPlatformPlanRepository platformPlanRepository,
        IProviderSubscriptionRepository providerSubscriptionRepository,
        IClientRepository clientRepository,
        UserManager<ApplicationUser> userManager,
        ILogger<CreateProviderCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _providerRepository = providerRepository;
        _platformPlanRepository = platformPlanRepository;
        _providerSubscriptionRepository = providerSubscriptionRepository;
        _clientRepository = clientRepository;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<Result<CreateProviderResult>> Handle(CreateProviderCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
        {
            return Result.Failure<CreateProviderResult>(DomainErrors.User.NotFound);
        }

        if (user.Provider != null)
        {
            return Result.Failure<CreateProviderResult>(DomainErrors.Provider.UserAlreadyHasProvider);
        }

        var sanitizedSubdomain = SanitizeSubdomain(request.SubdomainSlug);

        var existingProvider = await _providerRepository.GetBySubdomainSlugAsync(sanitizedSubdomain, cancellationToken);
        if (existingProvider != null)
        {
            return Result.Failure<CreateProviderResult>(DomainErrors.Provider.SubdomainAlreadyExists);
        }

        PlatformPlan? selectedPlan = null;
        if (request.SelectedPlatformPlanId.HasValue)
        {
            selectedPlan = await _platformPlanRepository.GetByIdAsync(request.SelectedPlatformPlanId.Value, cancellationToken);
            if (selectedPlan == null)
            {
                return Result.Failure<CreateProviderResult>(DomainErrors.ProviderSubscription.PlanNotFound);
            }
            if (!selectedPlan.IsActive)
            {
                return Result.Failure<CreateProviderResult>(DomainErrors.PlatformPlan.Inactive);
            }
        }

        var userRoles = await _userManager.GetRolesAsync(user);
        var needsProviderRole = !userRoles.Contains("Provider");

        var provider = Provider.Create(
            request.UserId,
            request.BusinessName,
            sanitizedSubdomain);

        if (!string.IsNullOrEmpty(request.Description))
            provider.SetDescription(request.Description);

        if (!string.IsNullOrEmpty(request.Avatar))
            provider.SetAvatar(request.Avatar);

        if (!string.IsNullOrEmpty(request.CustomDomain))
            provider.SetCustomDomain(request.CustomDomain);

        await _providerRepository.AddAsync(provider, cancellationToken);

        user.TenantId = provider.TenantId;

        if (needsProviderRole)
        {
            await _userManager.AddToRoleAsync(user, "Provider");
        }

        var ownerTeamMember = new TeamMember(
            provider.TenantId,
            request.UserId,
            TeamMemberRole.Owner,
            user.Email!,
            user.FirstName,
            user.LastName);

        await _unitOfWork.GetRepository<TeamMember>().AddAsync(ownerTeamMember, cancellationToken);

        if (selectedPlan != null)
        {
            var providerSubscription = ProviderSubscription.CreateTrial(
                provider.Id,
                selectedPlan.Id,
                selectedPlan.TrialDays);

            await _providerSubscriptionRepository.AddAsync(providerSubscription, cancellationToken);

            _logger.LogInformation(
                "Created trial subscription for Provider {ProviderId}: Plan={PlanName}, TrialDays={TrialDays}, TrialEndDate={TrialEndDate}",
                provider.Id, selectedPlan.Name, selectedPlan.TrialDays, providerSubscription.TrialEndDate);
        }

        var adminTenantId = await _providerRepository.GetPlatformAdminTenantIdAsync(cancellationToken);
        if (adminTenantId.HasValue)
        {
            var providerAsClient = Client.CreateDirect(
                TenantId.Create(adminTenantId.Value),
                request.Email,
                request.FirstName,
                request.LastName,
                request.BusinessName);

            providerAsClient.Activate();

            await _clientRepository.AddAsync(providerAsClient, cancellationToken);

            _logger.LogInformation(
                "Created Provider {ProviderId} as Client in PlatformAdmin tenant {AdminTenantId}",
                provider.Id, adminTenantId.Value);
        }
        else
        {
            _logger.LogWarning(
                "PlatformAdmin tenant not found - Provider {ProviderId} will not be added as Admin's client",
                provider.Id);
        }

        var saveResult = await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (!saveResult.IsSuccess)
        {
            _logger.LogError("Failed to save provider for user {UserId}", request.UserId);
            return Result.Failure<CreateProviderResult>(DomainErrors.General.UnexpectedError);
        }

        _logger.LogInformation("Provider created: {BusinessName} (TenantId: {TenantId})",
            provider.BusinessName, provider.TenantId.Value);

        var result = new CreateProviderResult(
            provider.Id,
            provider.TenantId.Value,
            provider.BusinessName,
            provider.SubdomainSlug,
            provider.IsActive);

        return Result.Success(result);
    }

    private static string SanitizeSubdomain(string subdomain)
    {
        if (string.IsNullOrWhiteSpace(subdomain))
            return string.Empty;

        // Remove any HTML/script tags and their content
        var sanitized = Regex.Replace(subdomain, @"<script[^>]*>.*?</script>", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        sanitized = Regex.Replace(sanitized, @"<[^>]+>", string.Empty, RegexOptions.IgnoreCase);

        // Remove common XSS keywords
        var dangerousWords = new[] { "alert", "script", "javascript", "onerror", "onload", "onclick", "eval", "expression" };
        foreach (var word in dangerousWords)
        {
            sanitized = Regex.Replace(sanitized, word, string.Empty, RegexOptions.IgnoreCase);
        }

        // Keep only lowercase letters, numbers, and hyphens
        sanitized = Regex.Replace(sanitized, @"[^a-z0-9-]", string.Empty, RegexOptions.IgnoreCase);

        // Remove consecutive hyphens
        sanitized = Regex.Replace(sanitized, @"-+", "-");

        // Remove leading and trailing hyphens
        sanitized = sanitized.Trim('-');

        return sanitized.ToLowerInvariant();
    }
}
