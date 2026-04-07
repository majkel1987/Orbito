using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.DTOs;
using Orbito.Domain.Common;
using Orbito.Domain.Errors;

namespace Orbito.Application.Providers.Commands.UpdateProvider;

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
        var provider = await _providerRepository.GetByIdAsync(request.Id, cancellationToken);
        if (provider == null)
        {
            return Result.Failure<ProviderDto>(DomainErrors.Provider.NotFound);
        }

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

        if (!string.IsNullOrEmpty(request.BusinessName))
        {
            var profileResult = provider.UpdateBusinessProfile(
                request.BusinessName,
                request.Description,
                request.Avatar);
            if (profileResult.IsFailure)
                return Result.Failure<ProviderDto>(profileResult.Error);
        }

        if (!string.IsNullOrEmpty(request.SubdomainSlug))
        {
            var settingsResult = provider.UpdatePlatformSettings(
                request.SubdomainSlug,
                request.CustomDomain);
            if (settingsResult.IsFailure)
                return Result.Failure<ProviderDto>(settingsResult.Error);
        }

        await _providerRepository.UpdateAsync(provider, cancellationToken);

        var saveResult = await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (!saveResult.IsSuccess)
        {
            _logger.LogError("Error saving provider updates: {Error}", saveResult.ErrorMessage);
            return Result.Failure<ProviderDto>(DomainErrors.General.UnexpectedError);
        }

        var updatedProvider = await _providerRepository.GetByIdAsync(request.Id, cancellationToken);
        if (updatedProvider == null)
        {
            return Result.Failure<ProviderDto>(DomainErrors.Provider.NotFound);
        }

        _logger.LogInformation("Provider updated: {ProviderId}", request.Id);

        return Result.Success(ProviderMapper.ToDto(updatedProvider));
    }
}
