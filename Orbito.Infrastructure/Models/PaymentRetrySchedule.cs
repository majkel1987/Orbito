using System;
using System.Collections.Generic;

namespace Orbito.Infrastructure.Models;

public partial class PaymentRetrySchedule
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid ClientId { get; set; }

    public Guid PaymentId { get; set; }

    public DateTime NextAttemptAt { get; set; }

    public int AttemptNumber { get; set; }

    public int MaxAttempts { get; set; }

    public string Status { get; set; } = null!;

    public string? LastError { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Client Client { get; set; } = null!;

    public virtual Payment Payment { get; set; } = null!;
}
