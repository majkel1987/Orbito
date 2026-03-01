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

namespace Orbito.Application.Providers.Commands.CreateProvider;

public class CreateProviderCommandHandler : IRequestHandler<CreateProviderCommand, Result<CreateProviderResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProviderRepository _providerRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<CreateProviderCommandHandler> _logger;

    public CreateProviderCommandHandler(
        IUnitOfWork unitOfWork,
        IProviderRepository providerRepository,
        UserManager<ApplicationUser> userManager,
        ILogger<CreateProviderCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _providerRepository = providerRepository;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<Result<CreateProviderResult>> Handle(CreateProviderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Wykonaj walidacje
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
            {
                return Result.Failure<CreateProviderResult>(DomainErrors.User.NotFound);
            }

            if (user.Provider != null)
            {
                return Result.Failure<CreateProviderResult>(DomainErrors.Provider.UserAlreadyHasProvider);
            }

            // Sanitize subdomain slug - remove any non-alphanumeric characters except hyphens
            var sanitizedSubdomain = SanitizeSubdomain(request.SubdomainSlug);

            var existingProvider = await _providerRepository.GetBySubdomainSlugAsync(sanitizedSubdomain, cancellationToken);
            if (existingProvider != null)
            {
                return Result.Failure<CreateProviderResult>(DomainErrors.Provider.SubdomainAlreadyExists);
            }

            // Sprawdź czy użytkownik już ma rolę Provider
            var userRoles = await _userManager.GetRolesAsync(user);
            var needsProviderRole = !userRoles.Contains("Provider");

            // Utwórz nowego providera
            var provider = Provider.Create(
                request.UserId,
                request.BusinessName,
                sanitizedSubdomain);

            // Ustaw dodatkowe właściwości
            if (!string.IsNullOrEmpty(request.Description))
                provider.Description = request.Description;

            if (!string.IsNullOrEmpty(request.Avatar))
                provider.Avatar = request.Avatar;

            if (!string.IsNullOrEmpty(request.CustomDomain))
                provider.CustomDomain = request.CustomDomain;

            // Dodaj providera do kontekstu
            await _providerRepository.AddAsync(provider, cancellationToken);

            // Zaktualizuj użytkownika - przypisz TenantId
            user.TenantId = provider.TenantId;

            // Dodaj rolę Provider tylko jeśli użytkownik jeszcze jej nie ma
            if (needsProviderRole)
            {
                await _userManager.AddToRoleAsync(user, "Provider");
            }

            // Utwórz TeamMember z rolą Owner - śledź w kontekście bez osobnego SaveChanges
            var ownerTeamMember = new TeamMember(
                provider.TenantId,
                request.UserId,
                TeamMemberRole.Owner,
                user.Email!,
                user.FirstName,
                user.LastName);

            await _unitOfWork.GetRepository<TeamMember>().AddAsync(ownerTeamMember, cancellationToken);

            // SaveChanges - zapisuje Provider + TeamMember atomowo w jednej transakcji
            var saveResult = await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (!saveResult.IsSuccess)
            {
                _logger.LogError("Błąd podczas zapisywania providera");
                return Result.Failure<CreateProviderResult>(DomainErrors.General.UnexpectedError);
            }

            _logger.LogInformation("Provider utworzony: {BusinessName} (TenantId: {TenantId})",
                provider.BusinessName, provider.TenantId.Value);

            var result = new CreateProviderResult(
                provider.Id,
                provider.TenantId,
                provider.BusinessName,
                provider.SubdomainSlug,
                provider.IsActive);

            return Result.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas tworzenia providera dla użytkownika {UserId}", request.UserId);
            return Result.Failure<CreateProviderResult>(DomainErrors.General.UnexpectedError);
        }
    }

    private static string SanitizeSubdomain(string subdomain)
    {
        if (string.IsNullOrWhiteSpace(subdomain))
            return string.Empty;

        // Remove any HTML/script tags and their content (including script tags)
        var sanitized = Regex.Replace(subdomain, @"<script[^>]*>.*?</script>", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        sanitized = Regex.Replace(sanitized, @"<[^>]+>", string.Empty, RegexOptions.IgnoreCase);
        
        // Remove common XSS keywords and dangerous words
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
