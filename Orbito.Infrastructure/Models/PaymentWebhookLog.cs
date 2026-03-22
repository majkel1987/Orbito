using System;
using System.Collections.Generic;

namespace Orbito.Infrastructure.Models;

public partial class PaymentWebhookLog
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string EventId { get; set; } = null!;

    public string Provider { get; set; } = null!;

    public string EventType { get; set; } = null!;

    public string Payload { get; set; } = null!;

    public DateTime? ProcessedAt { get; set; }

    public string Status { get; set; } = null!;

    public string? ErrorMessage { get; set; }

    public int Attempts { get; set; }

    public DateTime ReceivedAt { get; set; }

    public string? Metadata { get; set; }
}
