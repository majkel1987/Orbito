using Orbito.Domain.Entities;
using Orbito.Domain.ValueObjects;

namespace Orbito.Application.Common.Interfaces;

/// <summary>
/// Repository for reconciliation report operations
/// </summary>
public interface IReconciliationRepository : IRepository<ReconciliationReport>
{
    /// <summary>
    /// Saves a reconciliation report with its discrepancies
    /// </summary>
    Task SaveReportAsync(ReconciliationReport report, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the most recent reconciliation reports for a tenant
    /// </summary>
    Task<IEnumerable<ReconciliationReport>> GetRecentReportsAsync(
        TenantId tenantId,
        int count,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a reconciliation report by ID with discrepancies
    /// </summary>
    Task<ReconciliationReport?> GetReportWithDiscrepanciesAsync(
        Guid reportId,
        TenantId tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets discrepancies by report ID
    /// </summary>
    Task<IEnumerable<PaymentDiscrepancy>> GetDiscrepanciesByReportIdAsync(
        Guid reportId,
        TenantId tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets unresolved discrepancies for a tenant
    /// </summary>
    Task<IEnumerable<PaymentDiscrepancy>> GetUnresolvedDiscrepanciesAsync(
        TenantId tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a discrepancy resolution
    /// </summary>
    Task UpdateDiscrepancyAsync(
        PaymentDiscrepancy discrepancy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a discrepancy by ID
    /// </summary>
    Task<PaymentDiscrepancy?> GetDiscrepancyByIdAsync(
        Guid discrepancyId,
        TenantId tenantId,
        CancellationToken cancellationToken = default);
}
