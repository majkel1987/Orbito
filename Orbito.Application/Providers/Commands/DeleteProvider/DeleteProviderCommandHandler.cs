using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;

namespace Orbito.Application.Providers.Commands.DeleteProvider
{
    public class DeleteProviderCommandHandler : IRequestHandler<DeleteProviderCommand, DeleteProviderResult>
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

        public async Task<DeleteProviderResult> Handle(DeleteProviderCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Get existing provider
                var provider = await _providerRepository.GetByIdAsync(request.Id, cancellationToken);
                if (provider == null)
                {
                    return DeleteProviderResult.FailureResult("Provider not found");
                }

                // Check if provider can be deleted
                if (request.HardDelete && !provider.CanBeDeleted())
                {
                    return DeleteProviderResult.FailureResult(
                        "Provider cannot be permanently deleted because it has active clients or subscriptions",
                        new List<string> 
                        { 
                            "Provider has active clients or subscriptions",
                            "Consider soft delete instead"
                        });
                }

                // Begin transaction
                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                try
                {
                    if (request.HardDelete)
                    {
                        // Hard delete - remove from database
                        await _providerRepository.DeleteAsync(provider, cancellationToken);
                        await _unitOfWork.SaveChangesAsync(cancellationToken);
                        await _unitOfWork.CommitTransactionAsync(cancellationToken);

                        _logger.LogWarning("Provider hard deleted: {ProviderId}", request.Id);

                        return DeleteProviderResult.SuccessResult(
                            true, 
                            "Provider permanently deleted");
                    }
                    else
                    {
                        // Soft delete - deactivate provider
                        await _providerRepository.SoftDeleteAsync(provider, cancellationToken);
                        await _unitOfWork.SaveChangesAsync(cancellationToken);
                        await _unitOfWork.CommitTransactionAsync(cancellationToken);

                        _logger.LogInformation("Provider soft deleted (deactivated): {ProviderId}", request.Id);

                        return DeleteProviderResult.SuccessResult(
                            false, 
                            "Provider deactivated successfully");
                    }
                }
                catch (Exception)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting provider: {ProviderId}", request.Id);
                return DeleteProviderResult.FailureResult(
                    "Error deleting provider",
                    new List<string> { ex.Message });
            }
        }
    }
}
