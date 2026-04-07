using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Common.Models;
using Orbito.Application.Features.Payments.Queries.DTOs;
using Orbito.Domain.Enums;

namespace Orbito.Application.Features.Payments.Queries;

/// <summary>
/// Handler for get scheduled retries query.
/// SECURITY: Validates user context and ensures role-based access control.
/// </summary>
public class GetScheduledRetriesQueryHandler : IRequestHandler<GetScheduledRetriesQuery, PaginatedList<RetryScheduleDto>>
{
    private readonly IPaymentRetryRepository _retryRepository;
    private readonly IUserContextService _userContextService;
    private readonly ISecurityLimitService _securityLimitService;
    private readonly ILogger<GetScheduledRetriesQueryHandler> _logger;

    public GetScheduledRetriesQueryHandler(
        IPaymentRetryRepository retryRepository,
        IUserContextService userContextService,
        ISecurityLimitService securityLimitService,
        ILogger<GetScheduledRetriesQueryHandler> logger)
    {
        _retryRepository = retryRepository;
        _userContextService = userContextService;
        _securityLimitService = securityLimitService;
        _logger = logger;
    }

    public async Task<PaginatedList<RetryScheduleDto>> Handle(GetScheduledRetriesQuery request, CancellationToken cancellationToken)
    {
        // SECURITY: Validate pagination parameters to prevent DoS attacks
        var pagination = request.Pagination.ValidateWithService(_securityLimitService);

        _logger.LogDebug("Getting scheduled retries with filters: ClientId={ClientId}, Status={Status}, Page={Page}, PageSize={PageSize}",
            request.ClientId, request.Status, pagination.PageNumber, pagination.PageSize);

        // Get current user context for security
        var currentClientId = await _userContextService.GetCurrentClientIdAsync();
        var currentUserRole = _userContextService.GetCurrentUserRole();

        // SECURITY: Prevent cross-client data access
        // Providers can query any client in their tenant
        // Clients can only query their own data
        Guid? clientId;
        if (currentUserRole == UserRole.Provider.ToString() || currentUserRole == UserRole.PlatformAdmin.ToString())
        {
            // Providers/Admins can specify clientId or see all
            clientId = request.ClientId ?? currentClientId;
        }
        else
        {
            // Clients must use their own clientId
            if (request.ClientId.HasValue && request.ClientId != currentClientId)
            {
                _logger.LogWarning("Client {CurrentClientId} attempted to access retry schedules for client {RequestedClientId}",
                    currentClientId, request.ClientId);
                throw new UnauthorizedAccessException("You can only access your own retry schedules");
            }
            clientId = currentClientId;
        }

        if (!clientId.HasValue)
        {
            _logger.LogWarning("No client context available for scheduled retries query");
            return PaginatedList<RetryScheduleDto>.Empty(pagination.PageNumber, pagination.PageSize);
        }

        // Build query with filters
        var query = await _retryRepository.GetScheduledRetriesQueryAsync(
            clientId: clientId.Value,
            status: request.Status,
            cancellationToken: cancellationToken);

        // Apply pagination with read-only optimization
        var totalCount = await query.CountAsync(cancellationToken);

        // PERFORMANCE: First load entities with Include, then map in-memory to avoid N+1
        var schedules = await query
            .Include(rs => rs.Payment)
            .AsNoTracking()
            .OrderBy(rs => rs.NextAttemptAt)
            .ThenBy(rs => rs.AttemptNumber)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(cancellationToken);

        // Map to DTOs in-memory (Payment is already loaded via Include)
        var items = schedules.Select(rs => new RetryScheduleDto
        {
            Id = rs.Id,
            PaymentId = rs.PaymentId,
            ClientId = rs.Payment?.ClientId ?? Guid.Empty,
            Amount = rs.Payment?.Amount.Amount ?? 0,
            Currency = rs.Payment?.Amount.Currency.Code ?? string.Empty,
            NextAttemptAt = rs.NextAttemptAt,
            AttemptNumber = rs.AttemptNumber,
            MaxAttempts = rs.MaxAttempts,
            Status = rs.Status.ToString(),
            LastError = rs.LastError,
            CreatedAt = rs.CreatedAt,
            UpdatedAt = rs.UpdatedAt,
            IsOverdue = rs.NextAttemptAt < DateTime.UtcNow.AddMinutes(-_securityLimitService.RetryOverdueToleranceMinutes),
            CanRetry = rs.Status == RetryStatus.Scheduled &&
                      rs.AttemptNumber <= rs.MaxAttempts &&
                      rs.NextAttemptAt <= DateTime.UtcNow
        }).ToList();

        _logger.LogDebug("Retrieved {Count} scheduled retries out of {Total}", items.Count, totalCount);

        return new PaginatedList<RetryScheduleDto>(items, totalCount, pagination.PageNumber, pagination.PageSize);
    }
}
