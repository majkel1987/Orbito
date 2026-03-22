using System;
using System.Collections.Generic;

namespace Orbito.Infrastructure.Models;

public partial class TeamMember
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid UserId { get; set; }

    public string Role { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public bool IsActive { get; set; }

    public DateTime InvitedAt { get; set; }

    public DateTime LastActiveAt { get; set; }

    public DateTime? AcceptedAt { get; set; }

    public DateTime? RemovedAt { get; set; }

    public string? InvitationToken { get; set; }

    public DateTime? InvitationExpiresAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public virtual Provider Tenant { get; set; } = null!;
}
