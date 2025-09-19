using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;

namespace Orbito.Application.Providers.Queries.GetAllProviders
{
    public class GetAllProvidersQueryHandler : IRequestHandler<GetAllProvidersQuery, GetAllProvidersResult>
    {
        private readonly IProviderRepository _providerRepository;
        private readonly ILogger<GetAllProvidersQueryHandler> _logger;

        public GetAllProvidersQueryHandler(
            IProviderRepository providerRepository,
            ILogger<GetAllProvidersQueryHandler> logger)
        {
            _providerRepository = providerRepository;
            _logger = logger;
        }

        public async Task<GetAllProvidersResult> Handle(GetAllProvidersQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Validate pagination parameters
                if (request.PageNumber < 1)
                    return GetAllProvidersResult.FailureResult("Page number must be greater than 0");

                if (request.PageSize < 1 || request.PageSize > 100)
                    return GetAllProvidersResult.FailureResult("Page size must be between 1 and 100");

                IEnumerable<Provider> providers;
                int totalCount;

                if (request.ActiveOnly)
                {
                    providers = await _providerRepository.GetActiveProvidersAsync(
                        request.PageNumber, request.PageSize, cancellationToken);
                    totalCount = await _providerRepository.GetActiveCountAsync(cancellationToken);
                }
                else
                {
                    providers = await _providerRepository.GetAllAsync(
                        request.PageNumber, request.PageSize, cancellationToken);
                    totalCount = await _providerRepository.GetTotalCountAsync(cancellationToken);
                }

                var providerDtos = providers.Select(MapToSummaryDto);

                _logger.LogInformation("Retrieved {Count} providers (Page {PageNumber}/{TotalPages})",
                    providers.Count(), request.PageNumber, (int)Math.Ceiling((double)totalCount / request.PageSize));

                return GetAllProvidersResult.SuccessResult(
                    providerDtos, totalCount, request.PageNumber, request.PageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving providers");
                return GetAllProvidersResult.FailureResult("Error retrieving providers");
            }
        }

        private static ProviderSummaryDto MapToSummaryDto(Provider provider)
        {
            return new ProviderSummaryDto
            {
                Id = provider.Id,
                TenantId = provider.TenantId.Value,
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
                UserFullName = provider.User != null ? $"{provider.User.FirstName} {provider.User.LastName}" : null
            };
        }
    }
}
