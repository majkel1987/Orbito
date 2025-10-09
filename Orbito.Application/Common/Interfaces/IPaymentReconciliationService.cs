using Orbito.Domain.Entities;
using Orbito.Domain.ValueObjects;

namespace Orbito.Application.Common.Interfaces;

/// <summary>
/// Service for reconciling payments between Orbito and Stripe
/// </summary>
public interface IPaymentReconciliationService
{
    /// <summary>
    /// Reconciles payments with Stripe for a specific period and tenant
    /// </summary>
    /// <param name="fromDate">Start date of reconciliation period</param>
    /// <param name="toDate">End date of reconciliation period</param>
    /// <param name="tenantId">Tenant ID to reconcile payments for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Reconciliation report with discrepancies</returns>
    Task<ReconciliationReport> ReconcileWithStripeAsync(
        DateTime fromDate,
        DateTime toDate,
        TenantId tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a detailed discrepancy report
    /// </summary>
    /// <param name="discrepancies">List of payment discrepancies</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Reconciliation report with statistics</returns>
    Task<ReconciliationReport> GenerateDiscrepancyReportAsync(
        List<PaymentDiscrepancy> discrepancies,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Automatically resolves discrepancies based on predefined rules
    /// </summary>
    /// <param name="report">Reconciliation report to process</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of discrepancies auto-resolved</returns>
    Task<int> AutoResolveDiscrepanciesAsync(
        ReconciliationReport report,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends reconciliation report notification
    /// </summary>
    /// <param name="report">Report to send notification about</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SendReconciliationReportAsync(
        ReconciliationReport report,
        CancellationToken cancellationToken = default);
}
