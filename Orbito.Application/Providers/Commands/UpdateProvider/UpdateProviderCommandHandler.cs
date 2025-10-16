using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;

namespace Orbito.Application.Providers.Commands.UpdateProvider
{
    public class UpdateProviderCommandHandler : IRequestHandler<UpdateProviderCommand, UpdateProviderResult>
    {
        private readonly IProviderRepository _providerRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateProviderCommandHandler> _logger;

        public UpdateProviderCommandHandler(
            IProviderRepository providerRepository,
            IUnitOfWork unitOfWork,
            ILogger<UpdateProviderCommandHandler> logger)
        {
            _providerRepository = providerRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<UpdateProviderResult> Handle(UpdateProviderCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Get existing provider
                var provider = await _providerRepository.GetByIdAsync(request.Id, cancellationToken);
                if (provider == null)
                {
                    return UpdateProviderResult.FailureResult("Provider not found");
                }

                // Validate subdomain availability if it's being changed
                if (!string.IsNullOrEmpty(request.SubdomainSlug) && 
                    request.SubdomainSlug != provider.SubdomainSlug)
                {
                    var isAvailable = await _providerRepository.IsSubdomainAvailableAsync(
                        request.SubdomainSlug, request.Id, cancellationToken);
                    
                    if (!isAvailable)
                    {
                        return UpdateProviderResult.FailureResult(
                            "Subdomain is already taken",
                            new List<string> { "Subdomain slug is not available" });
                    }
                }

                // Begin transaction
                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                try
                {
                    // Update business profile
                    if (!string.IsNullOrEmpty(request.BusinessName))
                    {
                        provider.UpdateBusinessProfile(
                            request.BusinessName, 
                            request.Description, 
                            request.Avatar);
                    }

                    // Update platform settings
                    if (!string.IsNullOrEmpty(request.SubdomainSlug))
                    {
                        provider.UpdatePlatformSettings(
                            request.SubdomainSlug, 
                            request.CustomDomain);
                    }

                    // Update in repository
                    await _providerRepository.UpdateAsync(provider, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    await _unitOfWork.CommitAsync(cancellationToken);

                    // Get updated provider with all relations
                    var updatedProvider = await _providerRepository.GetByIdAsync(request.Id, cancellationToken);
                    var providerDto = MapToDto(updatedProvider!);

                    _logger.LogInformation("Provider updated: {ProviderId}", request.Id);

                    return UpdateProviderResult.SuccessResult(providerDto);
                }
                catch (Exception)
                {
                    await _unitOfWork.RollbackAsync(cancellationToken);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating provider: {ProviderId}", request.Id);
                return UpdateProviderResult.FailureResult(
                    "Error updating provider",
                    new List<string> { ex.Message });
            }
        }

        private static ProviderDto MapToDto(Provider provider)
        {
            return new ProviderDto
            {
                Id = provider.Id,
                TenantId = provider.TenantId.Value,
                UserId = provider.UserId,
                BusinessName = provider.BusinessName,
                Description = provider.Description,
                Avatar = provider.Avatar,
                SubdomainSlug = provider.SubdomainSlug,
                CustomDomain = provider.CustomDomain,
                IsActive = provider.IsActive,
                CreatedAt = provider.CreatedAt,
                MonthlyRevenue = provider.MonthlyRevenue.Amount,
                Currency = provider.MonthlyRevenue.Currency,
                ActiveClientsCount = provider.ActiveClientsCount,
                PlansCount = provider.Plans.Count,
                SubscriptionsCount = provider.Subscriptions.Count,
                UserEmail = provider.User?.Email,
                UserFirstName = provider.User?.FirstName,
                UserLastName = provider.User?.LastName
            };
        }
    }
}
