using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Enums;

namespace Orbito.Application.Features.Payments.Commands
{
    /// <summary>
    /// Handler for cancel retry command
    /// </summary>
    public class CancelRetryCommandHandler : IRequestHandler<CancelRetryCommand, CancelRetryResult>
    {
        private readonly IPaymentRetryRepository _retryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContextService _userContextService;
        private readonly ILogger<CancelRetryCommandHandler> _logger;

        public CancelRetryCommandHandler(
            IPaymentRetryRepository retryRepository,
            IUnitOfWork unitOfWork,
            IUserContextService userContextService,
            ILogger<CancelRetryCommandHandler> logger)
        {
            _retryRepository = retryRepository ?? throw new ArgumentNullException(nameof(retryRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _userContextService = userContextService ?? throw new ArgumentNullException(nameof(userContextService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<CancelRetryResult> Handle(CancelRetryCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Check for cancellation before starting
                cancellationToken.ThrowIfCancellationRequested();

                _logger.LogInformation("Processing cancel retry request for schedule {ScheduleId} by client {ClientId}",
                    request.ScheduleId, request.ClientId);

                // Get current user context for additional security
                var currentClientId = await _userContextService.GetCurrentClientIdAsync(cancellationToken);
                if (currentClientId != request.ClientId)
                {
                    _logger.LogWarning("Client {ClientId} attempted to cancel retry {ScheduleId} belonging to different client", 
                        currentClientId, request.ScheduleId);
                    return CancelRetryResult.FailureResult("You can only cancel your own retry schedules");
                }

                // Get the retry schedule with client verification
                var retrySchedule = await _retryRepository.GetByIdForClientAsync(request.ScheduleId, request.ClientId, cancellationToken);
                if (retrySchedule == null)
                {
                    _logger.LogWarning("Retry schedule {ScheduleId} not found or does not belong to client {ClientId}", 
                        request.ScheduleId, request.ClientId);
                    return CancelRetryResult.FailureResult("Retry schedule not found");
                }

                // Check if retry can be cancelled
                if (retrySchedule.Status == RetryStatus.Completed)
                {
                    _logger.LogWarning("Cannot cancel completed retry schedule {ScheduleId}", request.ScheduleId);
                    return CancelRetryResult.FailureResult("Cannot cancel a completed retry schedule");
                }

                if (retrySchedule.Status == RetryStatus.Cancelled)
                {
                    _logger.LogWarning("Retry schedule {ScheduleId} is already cancelled", request.ScheduleId);
                    return CancelRetryResult.FailureResult("Retry schedule is already cancelled");
                }

                // Cancel the retry schedule
                retrySchedule.Cancel();
                await _retryRepository.UpdateAsync(retrySchedule, cancellationToken);
                var saveResult = await _unitOfWork.SaveChangesAsync(cancellationToken);

                if (!saveResult.IsSuccess)
                {
                    _logger.LogError("Failed to save retry cancellation: {Error}", saveResult.ErrorMessage);
                    return CancelRetryResult.FailureResult(saveResult.ErrorMessage ?? "Failed to save changes");
                }

                _logger.LogInformation("Successfully cancelled retry schedule {ScheduleId}", request.ScheduleId);
                return CancelRetryResult.SuccessResult(request.ScheduleId);
            }
            catch (OperationCanceledException)
            {
                // Rethrow cancellation exceptions - they should not be caught
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling retry schedule {ScheduleId}", request.ScheduleId);
                return CancelRetryResult.FailureResult("An error occurred while cancelling the retry schedule");
            }
        }
    }
}
