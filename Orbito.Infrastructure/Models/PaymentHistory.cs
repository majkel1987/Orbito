using System;
using System.Collections.Generic;

namespace Orbito.Infrastructure.Models;

public partial class PaymentHistory
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid PaymentId { get; set; }

    public string Action { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime OccurredAt { get; set; }

    public string? Details { get; set; }

    public string? ErrorMessage { get; set; }

    public virtual Payment Payment { get; set; } = null!;
}
