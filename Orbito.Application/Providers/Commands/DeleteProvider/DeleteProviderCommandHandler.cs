using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Common;
using Orbito.Domain.Errors;

namespace Orbito.Application.Providers.Commands.DeleteProvider;

public class DeleteProviderCommandHandler : IRequestHandler<DeleteProviderCommand, Result<Unit>>
{
    private readonly IProviderRepository _providerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteProviderCommandHandler> _logger;

    public DeleteProviderCommandHandler(
        IProviderRepository providerRepository,
        IUnitOfWork unitOfWork,
        ILogger<DeleteProviderCommandHandler> logger)
    {
        _providerRepository = providerRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Unit>> Handle(DeleteProviderCommand request, CancellationToken cancellationToken)
    {
        var provider = await _providerRepository.GetByIdAsync(request.Id, cancellationToken);
        if (provider == null)
        {
            return Result.Failure<Unit>(DomainErrors.Provider.NotFound);
        }

        if (request.HardDelete && !provider.CanBeDeleted())
        {
            return Result.Failure<Unit>(DomainErrors.Provider.CannotDeleteWithActiveClients);
        }

        if (request.HardDelete)
        {
            await _providerRepository.DeleteAsync(provider, cancellationToken);
        }
        else
        {
            await _providerRepository.SoftDeleteAsync(provider, cancellationToken);
        }

        var saveResult = await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (!saveResult.IsSuccess)
        {
            _logger.LogError("Failed to delete provider {ProviderId}: {Error}", request.Id, saveResult.ErrorMessage);
            return Result.Failure<Unit>(DomainErrors.General.UnexpectedError);
        }

        if (request.HardDelete)
        {
            _logger.LogWarning("Provider hard deleted: {ProviderId}", request.Id);
        }
        else
        {
            _logger.LogInformation("Provider soft deleted (deactivated): {ProviderId}", request.Id);
        }

        return Result.Success(Unit.Value);
    }
}
