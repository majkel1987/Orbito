using System;
using System.Collections.Generic;

namespace Orbito.Infrastructure.Models;

public partial class PaymentMethod
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid ClientId { get; set; }

    public string Type { get; set; } = null!;

    public string Token { get; set; } = null!;

    public string? LastFourDigits { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public bool IsDefault { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Client Client { get; set; } = null!;
}
