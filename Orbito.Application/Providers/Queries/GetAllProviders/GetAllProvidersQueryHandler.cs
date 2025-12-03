using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Common.Models;
using Orbito.Application.DTOs;
using Orbito.Domain.Common;
using Orbito.Domain.Entities;

namespace Orbito.Application.Providers.Queries.GetAllProviders
{
    public class GetAllProvidersQueryHandler : IRequestHandler<GetAllProvidersQuery, Result<PaginatedList<ProviderDto>>>
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

        public async Task<Result<PaginatedList<ProviderDto>>> Handle(GetAllProvidersQuery request, CancellationToken cancellationToken)
        {
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

            var providerDtos = providers.Select(MapToDto).ToList();

            var paginatedList = new PaginatedList<ProviderDto>(
                providerDtos,
                totalCount,
                request.PageNumber,
                request.PageSize);

            _logger.LogInformation("Retrieved {Count} providers (Page {PageNumber}/{TotalPages})",
                providerDtos.Count, request.PageNumber, paginatedList.TotalPages);

            return Result.Success(paginatedList);
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
