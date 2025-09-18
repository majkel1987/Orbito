using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;
using Orbito.Domain.Identity;
using Orbito.Domain.ValueObjects;

namespace Orbito.Application.Providers.Commands.CreateProvider
{
    public class CreateProviderCommandHandler : IRequestHandler<CreateProviderCommand, CreateProviderResult>
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

        public async Task<CreateProviderResult> Handle(CreateProviderCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                // Sprawdź czy użytkownik istnieje
                var user = await _userManager.FindByIdAsync(request.UserId.ToString());
                if (user == null)
                {
                    throw new InvalidOperationException($"Użytkownik o ID {request.UserId} nie istnieje");
                }

                // Sprawdź czy użytkownik już ma providera
                if (user.Provider != null)
                {
                    throw new InvalidOperationException($"Użytkownik {user.Email} już ma przypisanego providera");
                }

                // Sprawdź czy subdomain jest dostępny
                var existingProvider = await _providerRepository.GetBySubdomainSlugAsync(request.SubdomainSlug, cancellationToken);
                
                if (existingProvider != null)
                {
                    throw new InvalidOperationException($"Subdomain '{request.SubdomainSlug}' jest już zajęty");
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
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                _logger.LogInformation("Provider utworzony: {BusinessName} (TenantId: {TenantId})", 
                    provider.BusinessName, provider.TenantId.Value);

                return new CreateProviderResult(
                    provider.Id,
                    provider.TenantId,
                    provider.BusinessName,
                    provider.SubdomainSlug,
                    provider.IsActive);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                _logger.LogError(ex, "Błąd podczas tworzenia providera dla użytkownika {UserId}", request.UserId);
                throw;
            }
        }
    }
}
