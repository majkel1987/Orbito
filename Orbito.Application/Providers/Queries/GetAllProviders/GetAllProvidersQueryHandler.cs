using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Common.Models;
using Orbito.Application.DTOs;

namespace Orbito.Application.Providers.Queries.GetAllProviders;

public class GetAllProvidersQueryHandler : IRequestHandler<GetAllProvidersQuery, Orbito.Domain.Common.Result<PaginatedList<ProviderDto>>>
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

    public async Task<Orbito.Domain.Common.Result<PaginatedList<ProviderDto>>> Handle(GetAllProvidersQuery request, CancellationToken cancellationToken)
    {
        var (providers, totalCount) = request.ActiveOnly
            ? (await _providerRepository.GetActiveProvidersAsync(request.PageNumber, request.PageSize, cancellationToken),
               await _providerRepository.GetActiveCountAsync(cancellationToken))
            : (await _providerRepository.GetAllAsync(request.PageNumber, request.PageSize, cancellationToken),
               await _providerRepository.GetTotalCountAsync(cancellationToken));

        var providerDtos = ProviderMapper.ToDtos(providers);

        var paginatedList = new PaginatedList<ProviderDto>(
            providerDtos.ToList(),
            totalCount,
            request.PageNumber,
            request.PageSize);

        _logger.LogDebug("Retrieved {Count} providers (Page {PageNumber}/{TotalPages})",
            providerDtos.Count, request.PageNumber, paginatedList.TotalPages);

        return Orbito.Domain.Common.Result.Success(paginatedList);
    }
}
