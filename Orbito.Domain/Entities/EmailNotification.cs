using Orbito.Domain.Enums;
using Orbito.Domain.Interfaces;
using Orbito.Domain.ValueObjects;

namespace Orbito.Domain.Entities;

/// <summary>
/// Email notification entity for outbox pattern
/// </summary>
public class EmailNotification : IMustHaveTenant
{
    public Guid Id { get; set; }
    public TenantId TenantId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Type { get; set; } = string.Empty;
    public string RecipientEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public int RetryCount { get; set; } = 0;
    public int MaxRetries { get; set; } = 3;
    public DateTime? ProcessedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? NextRetryAt { get; set; }
    public Guid? RelatedEntityId { get; set; } // PaymentId, SubscriptionId, etc.
    public string? RelatedEntityType { get; set; } // "Payment", "Subscription", etc.

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
            Status = "Pending",
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
        Status = "Processed";
        ProcessedAt = DateTime.UtcNow;
        ErrorMessage = null;
    }

    /// <summary>
    /// Marks notification as failed
    /// </summary>
    public void MarkAsFailed(string errorMessage)
    {
        Status = "Failed";
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
        Status = "Pending";
        
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
        return Status == "Pending" && 
               RetryCount < MaxRetries && 
               (NextRetryAt == null || NextRetryAt <= DateTime.UtcNow);
    }
}
