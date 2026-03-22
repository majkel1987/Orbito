using System;
using System.Collections.Generic;

namespace Orbito.Infrastructure.Models;

public partial class Client
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid? UserId { get; set; }

    public string? CompanyName { get; set; }

    public string? Phone { get; set; }

    public string? DirectEmail { get; set; }

    public string? DirectFirstName { get; set; }

    public string? DirectLastName { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsActive { get; set; }

    public string? StripeCustomerId { get; set; }

    public virtual ICollection<PaymentMethod> PaymentMethods { get; set; } = new List<PaymentMethod>();

    public virtual ICollection<PaymentRetrySchedule> PaymentRetrySchedules { get; set; } = new List<PaymentRetrySchedule>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();

    public virtual Provider Tenant { get; set; } = null!;

    public virtual AspNetUser? User { get; set; }
}
