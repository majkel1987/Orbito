using Orbito.Domain.Enums;
using Orbito.Domain.Interfaces;
using Orbito.Domain.ValueObjects;

namespace Orbito.Domain.Entities;

/// <summary>
/// Email notification entity for outbox pattern
/// </summary>
public class EmailNotification : IMustHaveTenant
{
    public Guid Id { get; private set; }
    public TenantId TenantId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string Type { get; private set; } = string.Empty;
    public string RecipientEmail { get; private set; } = string.Empty;
    public string Subject { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public EmailNotificationStatus Status { get; private set; } = EmailNotificationStatus.Pending;
    public int RetryCount { get; private set; }
    public int MaxRetries { get; private set; } = 3;
    public DateTime? ProcessedAt { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTime? NextRetryAt { get; private set; }
    public Guid? RelatedEntityId { get; private set; } // PaymentId, SubscriptionId, etc.
    public string? RelatedEntityType { get; private set; } // "Payment", "Subscription", etc.

    /// <summary>
    /// Creates a new email notification
    /// </summary>
    public static EmailNotification Create(
        TenantId tenantId,
        string type,
        string recipientEmail,
        string subject,
        string body,
        Guid? relatedEntityId = null,
        string? relatedEntityType = null)
    {
        return new EmailNotification
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Type = type,
            RecipientEmail = recipientEmail,
            Subject = subject,
            Body = body,
            Status = EmailNotificationStatus.Pending,
            RetryCount = 0,
            MaxRetries = 3,
            CreatedAt = DateTime.UtcNow,
            RelatedEntityId = relatedEntityId,
            RelatedEntityType = relatedEntityType
        };
    }

    /// <summary>
    /// Marks notification as processed
    /// </summary>
    public void MarkAsProcessed()
    {
        Status = EmailNotificationStatus.Processed;
        ProcessedAt = DateTime.UtcNow;
        ErrorMessage = null;
    }

    /// <summary>
    /// Marks notification as failed
    /// </summary>
    public void MarkAsFailed(string errorMessage)
    {
        Status = EmailNotificationStatus.Failed;
        ErrorMessage = errorMessage;
        ProcessedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Schedules retry with exponential backoff
    /// </summary>
    public void ScheduleRetry()
    {
        if (RetryCount >= MaxRetries)
        {
            MarkAsFailed("Max retries exceeded");
            return;
        }

        RetryCount++;
        Status = EmailNotificationStatus.Pending;

        // Exponential backoff: 5min, 15min, 1h
        var delayMinutes = RetryCount switch
        {
            1 => 5,
            2 => 15,
            3 => 60,
            _ => 60
        };

        NextRetryAt = DateTime.UtcNow.AddMinutes(delayMinutes);
    }

    /// <summary>
    /// Checks if notification can be retried
    /// </summary>
    public bool CanRetry()
    {
        return Status == EmailNotificationStatus.Pending &&
               RetryCount < MaxRetries &&
               (NextRetryAt == null || NextRetryAt <= DateTime.UtcNow);
    }
}
