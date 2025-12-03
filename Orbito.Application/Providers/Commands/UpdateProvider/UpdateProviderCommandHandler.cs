using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.DTOs;
using Orbito.Domain.Common;
using Orbito.Domain.Entities;
using Orbito.Domain.Errors;

namespace Orbito.Application.Providers.Commands.UpdateProvider
{
    public class UpdateProviderCommandHandler : IRequestHandler<UpdateProviderCommand, Result<ProviderDto>>
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

        public async Task<Result<ProviderDto>> Handle(UpdateProviderCommand request, CancellationToken cancellationToken)
        {
            // Get existing provider
            var provider = await _providerRepository.GetByIdAsync(request.Id, cancellationToken);
            if (provider == null)
            {
                return Result.Failure<ProviderDto>(DomainErrors.Provider.NotFound);
            }

            // Validate subdomain availability if it's being changed
            if (!string.IsNullOrEmpty(request.SubdomainSlug) &&
                request.SubdomainSlug != provider.SubdomainSlug)
            {
                var isAvailable = await _providerRepository.IsSubdomainAvailableAsync(
                    request.SubdomainSlug, request.Id, cancellationToken);

                if (!isAvailable)
                {
                    return Result.Failure<ProviderDto>(DomainErrors.Provider.SubdomainAlreadyExists);
                }
            }

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

            // SaveChanges - EF Core automatycznie utworzy transakcję i zastosuje retry strategy
            var saveResult = await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (!saveResult.IsSuccess)
            {
                _logger.LogError("Error saving provider updates: {Error}", saveResult.ErrorMessage);
                var error = Error.Create(
                    "Provider.SaveFailed",
                    saveResult.ErrorMessage ?? "Failed to save changes to database");
                return Result.Failure<ProviderDto>(error);
            }

            // Get updated provider with all relations
            var updatedProvider = await _providerRepository.GetByIdAsync(request.Id, cancellationToken);
            if (updatedProvider == null)
            {
                return Result.Failure<ProviderDto>(DomainErrors.Provider.NotFound);
            }

            var providerDto = MapToDto(updatedProvider);

            _logger.LogInformation("Provider updated: {ProviderId}", request.Id);

            return Result.Success(providerDto);
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
