using Orbito.Domain.Entities;

namespace Orbito.Application.Common.Interfaces;

/// <summary>
/// Repository interface for email notifications
/// </summary>
public interface IEmailNotificationRepository : IRepository<EmailNotification>
{
    /// <summary>
    /// Gets pending email notifications ready for processing
    /// </summary>
    Task<List<EmailNotification>> GetPendingNotificationsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets failed email notifications for cleanup
    /// </summary>
    Task<List<EmailNotification>> GetFailedNotificationsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets email notifications by type
    /// </summary>
    Task<List<EmailNotification>> GetByTypeAsync(string type, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets email notifications by related entity
    /// </summary>
    Task<List<EmailNotification>> GetByRelatedEntityAsync(Guid entityId, string entityType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets email notifications by status
    /// </summary>
    Task<List<EmailNotification>> GetByStatusAsync(string status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets email notifications ready for retry
    /// </summary>
    Task<List<EmailNotification>> GetReadyForRetryAsync(CancellationToken cancellationToken = default);
}
