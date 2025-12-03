using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Common;
using Orbito.Domain.Entities;
using Orbito.Domain.Errors;

namespace Orbito.Application.Providers.Commands.DeleteProvider
{
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
            // Get existing provider
            var provider = await _providerRepository.GetByIdAsync(request.Id, cancellationToken);
            if (provider == null)
            {
                return Result.Failure<Unit>(DomainErrors.Provider.NotFound);
            }

            // Check if provider can be deleted
            if (request.HardDelete && !provider.CanBeDeleted())
            {
                return Result.Failure<Unit>(DomainErrors.Provider.CannotDeleteWithActiveClients);
            }

            if (request.HardDelete)
            {
                // Hard delete - remove from database
                await _providerRepository.DeleteAsync(provider, cancellationToken);
            }
            else
            {
                // Soft delete - deactivate provider
                await _providerRepository.SoftDeleteAsync(provider, cancellationToken);
            }

            // SaveChanges - EF Core automatycznie utworzy transakcję i zastosuje retry strategy
            var saveResult = await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (!saveResult.IsSuccess)
            {
                _logger.LogError("Error deleting provider: {Error}", saveResult.ErrorMessage);
                var error = Error.Create(
                    "Provider.DeleteFailed",
                    saveResult.ErrorMessage ?? "Failed to save changes to database");
                return Result.Failure<Unit>(error);
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
}
