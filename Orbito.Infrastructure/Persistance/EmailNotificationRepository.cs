using Microsoft.EntityFrameworkCore;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Infrastructure.Data;
using System.Linq.Expressions;

namespace Orbito.Infrastructure.Persistance;

/// <summary>
/// Repository implementation for email notifications
/// </summary>
public class EmailNotificationRepository : IEmailNotificationRepository
{
    private readonly ApplicationDbContext _context;

    public EmailNotificationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EmailNotification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.EmailNotifications
            .FirstOrDefaultAsync(en => en.Id == id, cancellationToken);
    }

    public async Task<EmailNotification?> FirstOrDefaultAsync(Expression<Func<EmailNotification, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _context.EmailNotifications
            .FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task<IEnumerable<EmailNotification>> AddRangeAsync(IEnumerable<EmailNotification> entities, CancellationToken cancellationToken = default)
    {
        await _context.EmailNotifications.AddRangeAsync(entities, cancellationToken);
        return entities;
    }


    public async Task<bool> ExistsAsync(Expression<Func<EmailNotification, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _context.EmailNotifications
            .AnyAsync(predicate, cancellationToken);
    }

    public async Task<int> CountAsync(Expression<Func<EmailNotification, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.EmailNotifications.AsQueryable();
        if (predicate != null)
        {
            query = query.Where(predicate);
        }
        return await query.CountAsync(cancellationToken);
    }

    public IQueryable<EmailNotification> Query()
    {
        return _context.EmailNotifications.AsQueryable();
    }

    public async Task<List<EmailNotification>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.EmailNotifications
            .OrderByDescending(en => en.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<EmailNotification> AddAsync(EmailNotification entity, CancellationToken cancellationToken = default)
    {
        await _context.EmailNotifications.AddAsync(entity, cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(EmailNotification entity, CancellationToken cancellationToken = default)
    {
        _context.EmailNotifications.Update(entity);
    }

    public async Task DeleteAsync(EmailNotification entity, CancellationToken cancellationToken = default)
    {
        _context.EmailNotifications.Remove(entity);
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
        return await _context.EmailNotifications
            .AnyAsync(en => en.Id == id, cancellationToken);
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.EmailNotifications.CountAsync(cancellationToken);
    }

    public async Task<List<EmailNotification>> GetPendingNotificationsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.EmailNotifications
            .Where(en => en.Status == EmailNotificationStatus.Pending)
            .Where(en => en.NextRetryAt == null || en.NextRetryAt <= DateTime.UtcNow)
            .OrderBy(en => en.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<EmailNotification>> GetFailedNotificationsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.EmailNotifications
            .Where(en => en.Status == EmailNotificationStatus.Failed)
            .OrderByDescending(en => en.ProcessedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<EmailNotification>> GetByTypeAsync(string type, CancellationToken cancellationToken = default)
    {
        return await _context.EmailNotifications
            .Where(en => en.Type == type)
            .OrderByDescending(en => en.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<EmailNotification>> GetByRelatedEntityAsync(Guid entityId, string entityType, CancellationToken cancellationToken = default)
    {
        return await _context.EmailNotifications
            .Where(en => en.RelatedEntityId == entityId && en.RelatedEntityType == entityType)
            .OrderByDescending(en => en.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<EmailNotification>> GetByStatusAsync(EmailNotificationStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.EmailNotifications
            .Where(en => en.Status == status)
            .OrderByDescending(en => en.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<EmailNotification>> GetReadyForRetryAsync(CancellationToken cancellationToken = default)
    {
        return await _context.EmailNotifications
            .Where(en => en.Status == EmailNotificationStatus.Pending)
            .Where(en => en.RetryCount < en.MaxRetries)
            .Where(en => en.NextRetryAt == null || en.NextRetryAt <= DateTime.UtcNow)
            .OrderBy(en => en.NextRetryAt ?? en.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
