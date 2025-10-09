using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;
using Orbito.Infrastructure.Data;
using System.Linq.Expressions;

namespace Orbito.Infrastructure.Persistence;

/// <summary>
/// Repository for reconciliation report operations with tenant context
/// </summary>
public class ReconciliationRepository : IReconciliationRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<ReconciliationRepository> _logger;

    public ReconciliationRepository(
        ApplicationDbContext context,
        ITenantContext tenantContext,
        ILogger<ReconciliationRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Saves a reconciliation report with its discrepancies
    /// </summary>
    public async Task SaveReportAsync(ReconciliationReport report, CancellationToken cancellationToken = default)
    {
        if (report == null)
            throw new ArgumentNullException(nameof(report));

        // SECURITY: Verify tenant context matches report tenant
        if (_tenantContext.HasTenant && _tenantContext.CurrentTenantId != report.TenantId)
        {
            _logger.LogError(
                "SECURITY: Attempt to save report {ReportId} for tenant {ReportTenantId} but current tenant is {CurrentTenantId}",
                report.Id, report.TenantId.Value, _tenantContext.CurrentTenantId?.Value);
            throw new UnauthorizedAccessException("Cannot save report for different tenant");
        }

        try
        {
            var existingReport = await _context.ReconciliationReports
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == report.Id, cancellationToken);

            if (existingReport == null)
            {
                // New report - add with all discrepancies
                await _context.ReconciliationReports.AddAsync(report, cancellationToken);
                _logger.LogInformation("Creating new reconciliation report {ReportId} for tenant {TenantId} with {DiscrepancyCount} discrepancies",
                    report.Id, report.TenantId.Value, report.Discrepancies.Count);
            }
            else
            {
                // PERFORMANCE: Update report and bulk update/insert discrepancies
                _context.ReconciliationReports.Update(report);

                // Get existing discrepancy IDs
                var existingDiscrepancyIds = await _context.Set<PaymentDiscrepancy>()
                    .Where(d => d.ReconciliationReportId == report.Id)
                    .Select(d => d.Id)
                    .ToHashSetAsync(cancellationToken);

                var newDiscrepancies = report.Discrepancies
                    .Where(d => !existingDiscrepancyIds.Contains(d.Id))
                    .ToList();

                var updatedDiscrepancies = report.Discrepancies
                    .Where(d => existingDiscrepancyIds.Contains(d.Id))
                    .ToList();

                // Bulk insert new discrepancies
                if (newDiscrepancies.Any())
                {
                    await _context.Set<PaymentDiscrepancy>().AddRangeAsync(newDiscrepancies, cancellationToken);
                    _logger.LogDebug("Adding {Count} new discrepancies to report {ReportId}",
                        newDiscrepancies.Count, report.Id);
                }

                // Bulk update existing discrepancies
                if (updatedDiscrepancies.Any())
                {
                    _context.Set<PaymentDiscrepancy>().UpdateRange(updatedDiscrepancies);
                    _logger.LogDebug("Updating {Count} existing discrepancies for report {ReportId}",
                        updatedDiscrepancies.Count, report.Id);
                }

                _logger.LogInformation("Updating reconciliation report {ReportId} for tenant {TenantId}",
                    report.Id, report.TenantId.Value);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save reconciliation report {ReportId}: {ErrorMessage}",
                report.Id, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Gets the most recent reconciliation reports for a tenant
    /// </summary>
    public async Task<IEnumerable<ReconciliationReport>> GetRecentReportsAsync(
        TenantId tenantId,
        int count,
        CancellationToken cancellationToken = default)
    {
        if (tenantId == null)
            throw new ArgumentNullException(nameof(tenantId));

        if (count <= 0)
            throw new ArgumentException("Count must be greater than zero", nameof(count));

        // SECURITY: Verify tenant context
        if (_tenantContext.HasTenant && _tenantContext.CurrentTenantId != tenantId)
        {
            _logger.LogError(
                "SECURITY: Attempt to get reports for tenant {RequestedTenantId} but current tenant is {CurrentTenantId}",
                tenantId.Value, _tenantContext.CurrentTenantId?.Value);
            return Enumerable.Empty<ReconciliationReport>();
        }

        return await _context.ReconciliationReports
            .AsNoTracking()
            .Where(r => r.TenantId == tenantId)
            .OrderByDescending(r => r.RunDate)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets a reconciliation report by ID with discrepancies
    /// </summary>
    public async Task<ReconciliationReport?> GetReportWithDiscrepanciesAsync(
        Guid reportId,
        TenantId tenantId,
        CancellationToken cancellationToken = default)
    {
        if (tenantId == null)
            throw new ArgumentNullException(nameof(tenantId));

        // SECURITY: Verify tenant context
        if (_tenantContext.HasTenant && _tenantContext.CurrentTenantId != tenantId)
        {
            _logger.LogError(
                "SECURITY: Attempt to get report {ReportId} for tenant {RequestedTenantId} but current tenant is {CurrentTenantId}",
                reportId, tenantId.Value, _tenantContext.CurrentTenantId?.Value);
            return null;
        }

        return await _context.ReconciliationReports
            .AsNoTracking()
            .Include(r => r.Discrepancies)
            .Where(r => r.TenantId == tenantId)
            .FirstOrDefaultAsync(r => r.Id == reportId, cancellationToken);
    }

    /// <summary>
    /// Gets discrepancies by report ID
    /// </summary>
    public async Task<IEnumerable<PaymentDiscrepancy>> GetDiscrepanciesByReportIdAsync(
        Guid reportId,
        TenantId tenantId,
        CancellationToken cancellationToken = default)
    {
        if (tenantId == null)
            throw new ArgumentNullException(nameof(tenantId));

        // SECURITY: Verify tenant context
        if (_tenantContext.HasTenant && _tenantContext.CurrentTenantId != tenantId)
        {
            _logger.LogError(
                "SECURITY: Attempt to get discrepancies for report {ReportId} with tenant {RequestedTenantId} but current tenant is {CurrentTenantId}",
                reportId, tenantId.Value, _tenantContext.CurrentTenantId?.Value);
            return Enumerable.Empty<PaymentDiscrepancy>();
        }

        return await _context.Set<PaymentDiscrepancy>()
            .AsNoTracking()
            .Where(d => d.TenantId == tenantId)
            .Where(d => d.ReconciliationReportId == reportId)
            .OrderBy(d => d.DetectedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets unresolved discrepancies for a tenant
    /// </summary>
    public async Task<IEnumerable<PaymentDiscrepancy>> GetUnresolvedDiscrepanciesAsync(
        TenantId tenantId,
        CancellationToken cancellationToken = default)
    {
        if (tenantId == null)
            throw new ArgumentNullException(nameof(tenantId));

        // SECURITY: Verify tenant context
        if (_tenantContext.HasTenant && _tenantContext.CurrentTenantId != tenantId)
        {
            _logger.LogError(
                "SECURITY: Attempt to get unresolved discrepancies for tenant {RequestedTenantId} but current tenant is {CurrentTenantId}",
                tenantId.Value, _tenantContext.CurrentTenantId?.Value);
            return Enumerable.Empty<PaymentDiscrepancy>();
        }

        return await _context.Set<PaymentDiscrepancy>()
            .AsNoTracking()
            .Where(d => d.TenantId == tenantId)
            .Where(d => d.Resolution == DiscrepancyResolution.Pending ||
                       d.Resolution == DiscrepancyResolution.RequiresManualReview)
            .OrderBy(d => d.DetectedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Updates a discrepancy resolution
    /// </summary>
    public async Task UpdateDiscrepancyAsync(
        PaymentDiscrepancy discrepancy,
        CancellationToken cancellationToken = default)
    {
        if (discrepancy == null)
            throw new ArgumentNullException(nameof(discrepancy));

        // SECURITY: Verify tenant context matches discrepancy tenant
        if (_tenantContext.HasTenant && _tenantContext.CurrentTenantId != discrepancy.TenantId)
        {
            _logger.LogError(
                "SECURITY: Attempt to update discrepancy {DiscrepancyId} for tenant {DiscrepancyTenantId} but current tenant is {CurrentTenantId}",
                discrepancy.Id, discrepancy.TenantId.Value, _tenantContext.CurrentTenantId?.Value);
            throw new UnauthorizedAccessException("Cannot update discrepancy for different tenant");
        }

        try
        {
            _context.Set<PaymentDiscrepancy>().Update(discrepancy);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Updated discrepancy {DiscrepancyId} resolution to {Resolution}",
                discrepancy.Id, discrepancy.Resolution);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update discrepancy {DiscrepancyId}: {ErrorMessage}",
                discrepancy.Id, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Gets a discrepancy by ID
    /// </summary>
    public async Task<PaymentDiscrepancy?> GetDiscrepancyByIdAsync(
        Guid discrepancyId,
        TenantId tenantId,
        CancellationToken cancellationToken = default)
    {
        if (tenantId == null)
            throw new ArgumentNullException(nameof(tenantId));

        // SECURITY: Verify tenant context
        if (_tenantContext.HasTenant && _tenantContext.CurrentTenantId != tenantId)
        {
            _logger.LogError(
                "SECURITY: Attempt to get discrepancy {DiscrepancyId} for tenant {RequestedTenantId} but current tenant is {CurrentTenantId}",
                discrepancyId, tenantId.Value, _tenantContext.CurrentTenantId?.Value);
            return null;
        }

        return await _context.Set<PaymentDiscrepancy>()
            .AsNoTracking()
            .Where(d => d.TenantId == tenantId)
            .FirstOrDefaultAsync(d => d.Id == discrepancyId, cancellationToken);
    }

    #region IRepository<ReconciliationReport> implementation

    public async Task<ReconciliationReport?> FirstOrDefaultAsync(
        Expression<Func<ReconciliationReport, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        if (!_tenantContext.HasTenant)
        {
            _logger.LogError("SECURITY: No tenant context available");
            return null;
        }

        return await _context.ReconciliationReports
            .AsNoTracking()
            .Where(r => r.TenantId == _tenantContext.CurrentTenantId)
            .FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task<ReconciliationReport> AddAsync(ReconciliationReport entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        // SECURITY: Verify tenant context
        if (_tenantContext.HasTenant && _tenantContext.CurrentTenantId != entity.TenantId)
        {
            _logger.LogError(
                "SECURITY: Attempt to add report for tenant {EntityTenantId} but current tenant is {CurrentTenantId}",
                entity.TenantId.Value, _tenantContext.CurrentTenantId?.Value);
            throw new UnauthorizedAccessException("Cannot add report for different tenant");
        }

        await _context.ReconciliationReports.AddAsync(entity, cancellationToken);
        return entity;
    }

    public async Task<IEnumerable<ReconciliationReport>> AddRangeAsync(
        IEnumerable<ReconciliationReport> entities,
        CancellationToken cancellationToken = default)
    {
        var entitiesList = entities.ToList();

        // SECURITY: Verify all entities belong to current tenant
        if (_tenantContext.HasTenant)
        {
            var invalidEntities = entitiesList
                .Where(e => e.TenantId != _tenantContext.CurrentTenantId)
                .ToList();

            if (invalidEntities.Any())
            {
                _logger.LogError("SECURITY: Attempt to add reports for different tenants");
                throw new UnauthorizedAccessException("Cannot add reports for different tenants");
            }
        }

        await _context.ReconciliationReports.AddRangeAsync(entitiesList, cancellationToken);
        return entitiesList;
    }

    public async Task UpdateAsync(ReconciliationReport entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        // SECURITY: Verify tenant context
        if (_tenantContext.HasTenant && _tenantContext.CurrentTenantId != entity.TenantId)
        {
            _logger.LogError(
                "SECURITY: Attempt to update report for tenant {EntityTenantId} but current tenant is {CurrentTenantId}",
                entity.TenantId.Value, _tenantContext.CurrentTenantId?.Value);
            throw new UnauthorizedAccessException("Cannot update report for different tenant");
        }

        _context.ReconciliationReports.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(ReconciliationReport entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        // SECURITY: Verify tenant context
        if (_tenantContext.HasTenant && _tenantContext.CurrentTenantId != entity.TenantId)
        {
            _logger.LogError(
                "SECURITY: Attempt to delete report for tenant {EntityTenantId} but current tenant is {CurrentTenantId}",
                entity.TenantId.Value, _tenantContext.CurrentTenantId?.Value);
            throw new UnauthorizedAccessException("Cannot delete report for different tenant");
        }

        _context.ReconciliationReports.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        Expression<Func<ReconciliationReport, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        if (!_tenantContext.HasTenant)
        {
            _logger.LogError("SECURITY: No tenant context available");
            return false;
        }

        return await _context.ReconciliationReports
            .Where(r => r.TenantId == _tenantContext.CurrentTenantId)
            .AnyAsync(predicate, cancellationToken);
    }

    public async Task<int> CountAsync(
        Expression<Func<ReconciliationReport, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        if (!_tenantContext.HasTenant)
        {
            _logger.LogError("SECURITY: No tenant context available");
            return 0;
        }

        var query = _context.ReconciliationReports
            .Where(r => r.TenantId == _tenantContext.CurrentTenantId);

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        return await query.CountAsync(cancellationToken);
    }

    public IQueryable<ReconciliationReport> Query()
    {
        if (!_tenantContext.HasTenant)
        {
            _logger.LogError("SECURITY: No tenant context available");
            return Enumerable.Empty<ReconciliationReport>().AsQueryable();
        }

        return _context.ReconciliationReports
            .Where(r => r.TenantId == _tenantContext.CurrentTenantId);
    }

    #endregion
}
