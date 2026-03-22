using System;
using System.Collections.Generic;

namespace Orbito.Infrastructure.Models;

public partial class EmailNotification
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public DateTime CreatedAt { get; set; }

    public string Type { get; set; } = null!;

    public string RecipientEmail { get; set; } = null!;

    public string Subject { get; set; } = null!;

    public string Body { get; set; } = null!;

    public string Status { get; set; } = null!;

    public int RetryCount { get; set; }

    public int MaxRetries { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime? NextRetryAt { get; set; }

    public Guid? RelatedEntityId { get; set; }

    public string? RelatedEntityType { get; set; }
}
