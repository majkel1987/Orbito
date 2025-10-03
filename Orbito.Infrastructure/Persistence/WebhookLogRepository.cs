using Microsoft.EntityFrameworkCore;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Infrastructure.Data;

namespace Orbito.Infrastructure.Persistence
{
    /// <summary>
    /// Repository implementation for webhook log operations
    /// </summary>
    public class WebhookLogRepository : IWebhookLogRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantContext _tenantContext;

        public WebhookLogRepository(ApplicationDbContext context, ITenantContext tenantContext)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        }

        /// <summary>
        /// Gets webhook log by event ID (tenant-filtered)
        /// </summary>
        public async Task<PaymentWebhookLog?> GetByEventIdAsync(string eventId, CancellationToken cancellationToken = default)
        {
            if (!_tenantContext.HasTenant)
            {
                return null;
            }

            var tenantId = _tenantContext.CurrentTenantId;
            return await _context.PaymentWebhookLogs
                .Where(w => w.TenantId == tenantId)
                .FirstOrDefaultAsync(w => w.EventId == eventId, cancellationToken);
        }

        /// <summary>
        /// Checks if webhook has already processed the payment with given event type (tenant-filtered)
        /// </summary>
        public async Task<PaymentWebhookLog?> GetByPaymentAndEventAsync(Guid paymentId, string eventType, CancellationToken cancellationToken = default)
        {
            if (!_tenantContext.HasTenant)
            {
                return null;
            }

            var tenantId = _tenantContext.CurrentTenantId;

            // Query PaymentWebhookLog and join with Payments to find matching records
            var webhookLog = await _context.PaymentWebhookLogs
                .Where(w => w.TenantId == tenantId && w.EventType == eventType)
                .Where(w => w.Status == WebhookStatus.Processed)
                .OrderByDescending(w => w.ProcessedAt)
                .FirstOrDefaultAsync(cancellationToken);

            return webhookLog;
        }

        /// <summary>
        /// Gets webhook logs by provider (tenant-filtered)
        /// </summary>
        public async Task<IEnumerable<PaymentWebhookLog>> GetByProviderAsync(
            string provider,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            if (!_tenantContext.HasTenant)
            {
                return Enumerable.Empty<PaymentWebhookLog>();
            }

            var tenantId = _tenantContext.CurrentTenantId;
            return await _context.PaymentWebhookLogs
                .Where(w => w.TenantId == tenantId && w.Provider == provider)
                .OrderByDescending(w => w.ProcessedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Gets webhook logs by event type (tenant-filtered)
        /// </summary>
        public async Task<IEnumerable<PaymentWebhookLog>> GetByEventTypeAsync(
            string eventType,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            if (!_tenantContext.HasTenant)
            {
                return Enumerable.Empty<PaymentWebhookLog>();
            }

            var tenantId = _tenantContext.CurrentTenantId;
            return await _context.PaymentWebhookLogs
                .Where(w => w.TenantId == tenantId && w.EventType == eventType)
                .OrderByDescending(w => w.ProcessedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Gets failed webhook logs that can be retried (tenant-filtered)
        /// </summary>
        public async Task<IEnumerable<PaymentWebhookLog>> GetFailedWebhooksAsync(
            int maxAttempts = 3,
            CancellationToken cancellationToken = default)
        {
            if (!_tenantContext.HasTenant)
            {
                return Enumerable.Empty<PaymentWebhookLog>();
            }

            var tenantId = _tenantContext.CurrentTenantId;
            return await _context.PaymentWebhookLogs
                .Where(w => w.TenantId == tenantId && w.Status == WebhookStatus.Failed && w.Attempts < maxAttempts)
                .OrderBy(w => w.ReceivedAt)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Adds a new webhook log
        /// </summary>
        public async Task<PaymentWebhookLog> AddAsync(PaymentWebhookLog webhookLog, CancellationToken cancellationToken = default)
        {
            _context.PaymentWebhookLogs.Add(webhookLog);
            await _context.SaveChangesAsync(cancellationToken);
            return webhookLog;
        }

        /// <summary>
        /// Updates an existing webhook log
        /// </summary>
        public async Task UpdateAsync(PaymentWebhookLog webhookLog, CancellationToken cancellationToken = default)
        {
            _context.PaymentWebhookLogs.Update(webhookLog);
            await _context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Saves changes to the database
        /// </summary>
        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Gets webhook statistics (tenant-filtered)
        /// SECURITY: Only returns statistics for the current tenant
        /// </summary>
        public async Task<WebhookStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default)
        {
            if (!_tenantContext.HasTenant)
            {
                return new WebhookStatistics
                {
                    TotalWebhooks = 0,
                    ProcessedWebhooks = 0,
                    FailedWebhooks = 0,
                    PendingWebhooks = 0,
                    WebhooksByProvider = new Dictionary<string, int>(),
                    WebhooksByEventType = new Dictionary<string, int>(),
                    LastUpdated = DateTime.UtcNow
                };
            }

            var tenantId = _tenantContext.CurrentTenantId;
            var webhooks = await _context.PaymentWebhookLogs
                .Where(w => w.TenantId == tenantId)
                .ToListAsync(cancellationToken);

            var statistics = new WebhookStatistics
            {
                TotalWebhooks = webhooks.Count,
                ProcessedWebhooks = webhooks.Count(w => w.Status == WebhookStatus.Processed),
                FailedWebhooks = webhooks.Count(w => w.Status == WebhookStatus.Failed),
                PendingWebhooks = webhooks.Count(w => w.Status == WebhookStatus.Pending),
                WebhooksByProvider = webhooks
                    .GroupBy(w => w.Provider)
                    .ToDictionary(g => g.Key, g => g.Count()),
                WebhooksByEventType = webhooks
                    .GroupBy(w => w.EventType)
                    .ToDictionary(g => g.Key, g => g.Count()),
                LastUpdated = DateTime.UtcNow
            };

            return statistics;
        }
    }
}
