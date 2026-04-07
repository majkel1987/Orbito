using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.DTOs;
using Orbito.Domain.Common;
using Orbito.Domain.Errors;

namespace Orbito.Application.Providers.Queries.GetProviderById;

public class GetProviderByIdQueryHandler : IRequestHandler<GetProviderByIdQuery, Result<ProviderDto>>
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

    public async Task<Result<ProviderDto>> Handle(GetProviderByIdQuery request, CancellationToken cancellationToken)
    {
        var provider = await _providerRepository.GetByIdAsync(request.Id, cancellationToken);

        if (provider == null)
        {
            _logger.LogWarning("Provider not found: {ProviderId}", request.Id);
            return Result.Failure<ProviderDto>(DomainErrors.Provider.NotFound);
        }

        _logger.LogDebug("Provider retrieved: {ProviderId}", request.Id);

        return Result.Success(ProviderMapper.ToDto(provider));
    }
}
