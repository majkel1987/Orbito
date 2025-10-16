using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Common;
using Orbito.Domain.Entities;
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
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            // Sprawdź czy użytkownik istnieje
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                return Result.Failure<CreateProviderResult>(DomainErrors.User.NotFound);
            }

            // Sprawdź czy użytkownik już ma providera
            if (user.Provider != null)
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                return Result.Failure<CreateProviderResult>(DomainErrors.Provider.UserAlreadyHasProvider);
            }

            // Sprawdź czy subdomain jest dostępny
            var existingProvider = await _providerRepository.GetBySubdomainSlugAsync(request.SubdomainSlug, cancellationToken);

            if (existingProvider != null)
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                return Result.Failure<CreateProviderResult>(DomainErrors.Provider.SubdomainAlreadyExists);
            }

            // Utwórz nowego providera
            var provider = Provider.Create(
                request.UserId,
                request.BusinessName,
                request.SubdomainSlug);

            // Ustaw dodatkowe właściwości
            if (!string.IsNullOrEmpty(request.Description))
                provider.Description = request.Description;

            if (!string.IsNullOrEmpty(request.Avatar))
                provider.Avatar = request.Avatar;

            if (!string.IsNullOrEmpty(request.CustomDomain))
                provider.CustomDomain = request.CustomDomain;

            // Zapisz providera
            await _providerRepository.AddAsync(provider, cancellationToken);

            // Zaktualizuj użytkownika - przypisz TenantId i rolę Provider
            user.TenantId = provider.TenantId;
            await _userManager.AddToRoleAsync(user, "Provider");

            // Zapisz zmiany
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);

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
            await _unitOfWork.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Błąd podczas tworzenia providera dla użytkownika {UserId}", request.UserId);
            return Result.Failure<CreateProviderResult>(DomainErrors.General.UnexpectedError);
        }
    }
}
