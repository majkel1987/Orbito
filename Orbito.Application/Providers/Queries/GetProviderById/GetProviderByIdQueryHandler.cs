using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;

namespace Orbito.Application.Providers.Queries.GetProviderById
{
    public class GetProviderByIdQueryHandler : IRequestHandler<GetProviderByIdQuery, GetProviderByIdResult>
    {
        private readonly IProviderRepository _providerRepository;
        private readonly ILogger<GetProviderByIdQueryHandler> _logger;

        public GetProviderByIdQueryHandler(
            IProviderRepository providerRepository,
            ILogger<GetProviderByIdQueryHandler> logger)
        {
            _providerRepository = providerRepository;
            _logger = logger;
        }

        public async Task<GetProviderByIdResult> Handle(GetProviderByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var provider = await _providerRepository.GetByIdAsync(request.Id, cancellationToken);

                if (provider == null)
                {
                    _logger.LogWarning("Provider not found: {ProviderId}", request.Id);
                    return GetProviderByIdResult.NotFoundResult();
                }

                var providerDto = MapToDto(provider);

                _logger.LogInformation("Provider retrieved: {ProviderId}", request.Id);

                return GetProviderByIdResult.SuccessResult(providerDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving provider: {ProviderId}", request.Id);
                return GetProviderByIdResult.NotFoundResult("Error retrieving provider");
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
