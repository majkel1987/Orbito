using System;
using System.Collections.Generic;

namespace Orbito.Infrastructure.Models;

public partial class Provider
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid? UserId { get; set; }

    public string BusinessName { get; set; } = null!;

    public string? Description { get; set; }

    public string? Avatar { get; set; }

    public string SubdomainSlug { get; set; } = null!;

    public string? CustomDomain { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public decimal MonthlyRevenueAmount { get; set; }

    public string MonthlyRevenueCurrency { get; set; } = null!;

    public int ActiveClientsCount { get; set; }

    public virtual ICollection<AspNetRole> AspNetRoles { get; set; } = new List<AspNetRole>();

    public virtual ICollection<Client> Clients { get; set; } = new List<Client>();

    public virtual ICollection<SubscriptionPlan> SubscriptionPlans { get; set; } = new List<SubscriptionPlan>();

    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();

    public virtual ICollection<TeamMember> TeamMembers { get; set; } = new List<TeamMember>();

    public virtual AspNetUser? User { get; set; }
}
