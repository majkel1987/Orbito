using Microsoft.EntityFrameworkCore;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Infrastructure.Data;
using System.Linq.Expressions;

namespace Orbito.Infrastructure.Persistance;

/// <summary>
/// Repository implementation for email notifications with tenant isolation
/// </summary>
public class EmailNotificationRepository : IEmailNotificationRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;

    public EmailNotificationRepository(ApplicationDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
    }

    public async Task<EmailNotification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!_tenantContext.HasTenant)
            return null;

        var tenantId = _tenantContext.CurrentTenantId;
        return await _context.EmailNotifications
            .Where(en => en.TenantId == tenantId)
            .FirstOrDefaultAsync(en => en.Id == id, cancellationToken);
    }

    public async Task<EmailNotification?> FirstOrDefaultAsync(Expression<Func<EmailNotification, bool>> predicate, CancellationToken cancellationToken = default)
    {
        if (!_tenantContext.HasTenant)
            return null;

        var tenantId = _tenantContext.CurrentTenantId;
        return await _context.EmailNotifications
            .Where(en => en.TenantId == tenantId)
            .FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task<IEnumerable<EmailNotification>> AddRangeAsync(IEnumerable<EmailNotification> entities, CancellationToken cancellationToken = default)
    {
        await _context.EmailNotifications.AddRangeAsync(entities, cancellationToken);
        return entities;
    }


    public async Task<bool> ExistsAsync(Expression<Func<EmailNotification, bool>> predicate, CancellationToken cancellationToken = default)
    {
        if (!_tenantContext.HasTenant)
            return false;

        var tenantId = _tenantContext.CurrentTenantId;
        return await _context.EmailNotifications
            .Where(en => en.TenantId == tenantId)
            .AnyAsync(predicate, cancellationToken);
    }

    public async Task<int> CountAsync(Expression<Func<EmailNotification, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        if (!_tenantContext.HasTenant)
            return 0;

        var tenantId = _tenantContext.CurrentTenantId;
        var query = _context.EmailNotifications
            .Where(en => en.TenantId == tenantId)
            .AsQueryable();

        if (predicate != null)
        {
            query = query.Where(predicate);
        }
        return await query.CountAsync(cancellationToken);
    }

    public IQueryable<EmailNotification> Query()
    {
        if (!_tenantContext.HasTenant)
            return Enumerable.Empty<EmailNotification>().AsQueryable();

        var tenantId = _tenantContext.CurrentTenantId;
        return _context.EmailNotifications
            .Where(en => en.TenantId == tenantId)
            .AsQueryable();
    }

    public async Task<List<EmailNotification>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        if (!_tenantContext.HasTenant)
            return new List<EmailNotification>();

        var tenantId = _tenantContext.CurrentTenantId;
        return await _context.EmailNotifications
            .Where(en => en.TenantId == tenantId)
            .OrderByDescending(en => en.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<EmailNotification> AddAsync(EmailNotification entity, CancellationToken cancellationToken = default)
    {
        await _context.EmailNotifications.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(EmailNotification entity, CancellationToken cancellationToken = default)
    {
        _context.EmailNotifications.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(EmailNotification entity, CancellationToken cancellationToken = default)
    {
        _context.EmailNotifications.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null)
        {
            await DeleteAsync(entity, cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!_tenantContext.HasTenant)
            return false;

        var tenantId = _tenantContext.CurrentTenantId;
        return await _context.EmailNotifications
            .Where(en => en.TenantId == tenantId)
            .AnyAsync(en => en.Id == id, cancellationToken);
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        if (!_tenantContext.HasTenant)
            return 0;

        var tenantId = _tenantContext.CurrentTenantId;
        return await _context.EmailNotifications
            .Where(en => en.TenantId == tenantId)
            .CountAsync(cancellationToken);
    }

    public async Task<List<EmailNotification>> GetPendingNotificationsAsync(CancellationToken cancellationToken = default)
    {
        if (!_tenantContext.HasTenant)
            return new List<EmailNotification>();

        var tenantId = _tenantContext.CurrentTenantId;
        return await _context.EmailNotifications
            .Where(en => en.TenantId == tenantId)
            .Where(en => en.Status == EmailNotificationStatus.Pending)
            .Where(en => en.NextRetryAt == null || en.NextRetryAt <= DateTime.UtcNow)
            .OrderBy(en => en.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<EmailNotification>> GetFailedNotificationsAsync(CancellationToken cancellationToken = default)
    {
        if (!_tenantContext.HasTenant)
            return new List<EmailNotification>();

        var tenantId = _tenantContext.CurrentTenantId;
        return await _context.EmailNotifications
            .Where(en => en.TenantId == tenantId)
            .Where(en => en.Status == EmailNotificationStatus.Failed)
            .OrderByDescending(en => en.ProcessedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<EmailNotification>> GetByTypeAsync(string type, CancellationToken cancellationToken = default)
    {
        if (!_tenantContext.HasTenant)
            return new List<EmailNotification>();

        var tenantId = _tenantContext.CurrentTenantId;
        return await _context.EmailNotifications
            .Where(en => en.TenantId == tenantId)
            .Where(en => en.Type == type)
            .OrderByDescending(en => en.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<EmailNotification>> GetByRelatedEntityAsync(Guid entityId, string entityType, CancellationToken cancellationToken = default)
    {
        if (!_tenantContext.HasTenant)
            return new List<EmailNotification>();

        var tenantId = _tenantContext.CurrentTenantId;
        return await _context.EmailNotifications
            .Where(en => en.TenantId == tenantId)
            .Where(en => en.RelatedEntityId == entityId && en.RelatedEntityType == entityType)
            .OrderByDescending(en => en.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<EmailNotification>> GetByStatusAsync(EmailNotificationStatus status, CancellationToken cancellationToken = default)
    {
        if (!_tenantContext.HasTenant)
            return new List<EmailNotification>();

        var tenantId = _tenantContext.CurrentTenantId;
        return await _context.EmailNotifications
            .Where(en => en.TenantId == tenantId)
            .Where(en => en.Status == status)
            .OrderByDescending(en => en.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<EmailNotification>> GetReadyForRetryAsync(CancellationToken cancellationToken = default)
    {
        if (!_tenantContext.HasTenant)
            return new List<EmailNotification>();

        var tenantId = _tenantContext.CurrentTenantId;
        return await _context.EmailNotifications
            .Where(en => en.TenantId == tenantId)
            .Where(en => en.Status == EmailNotificationStatus.Pending)
            .Where(en => en.RetryCount < en.MaxRetries)
            .Where(en => en.NextRetryAt == null || en.NextRetryAt <= DateTime.UtcNow)
            .OrderBy(en => en.NextRetryAt ?? en.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
