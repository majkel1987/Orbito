using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.DTOs;
using Orbito.Domain.Common;
using Orbito.Domain.Errors;

namespace Orbito.Application.Providers.Queries.GetProviderByUserId;

public class GetProviderByUserIdQueryHandler : IRequestHandler<GetProviderByUserIdQuery, Result<ProviderDto>>
{
    private readonly IProviderRepository _providerRepository;
    private readonly ILogger<GetProviderByUserIdQueryHandler> _logger;

    public GetProviderByUserIdQueryHandler(
        IProviderRepository providerRepository,
        ILogger<GetProviderByUserIdQueryHandler> logger)
    {
        _providerRepository = providerRepository;
        _logger = logger;
    }

    public async Task<Result<ProviderDto>> Handle(GetProviderByUserIdQuery request, CancellationToken cancellationToken)
    {
        var provider = await _providerRepository.GetByUserIdAsync(request.UserId, cancellationToken);

        if (provider == null)
        {
            _logger.LogWarning("Provider not found for user: {UserId}", request.UserId);
            return Result.Failure<ProviderDto>(DomainErrors.Provider.NotFound);
        }

        _logger.LogDebug("Provider retrieved for user: {UserId}", request.UserId);

        return Result.Success(ProviderMapper.ToDto(provider));
    }
}
