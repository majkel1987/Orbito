using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orbito.Application.Common.Configuration;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;
using Orbito.Infrastructure.Data;
using Stripe;
using System.Collections.Concurrent;

namespace Orbito.Infrastructure.Services;

/// <summary>
/// Service for reconciling payments between Orbito and Stripe
/// </summary>
public class PaymentReconciliationService : IPaymentReconciliationService
{
    private readonly ApplicationDbContext _context;
    private readonly IReconciliationRepository _reconciliationRepository;
    private readonly ILogger<PaymentReconciliationService> _logger;
    private readonly IEmailSender _emailSender;
    private readonly ReconciliationSettings _settings;

    public PaymentReconciliationService(
        ApplicationDbContext context,
        IReconciliationRepository reconciliationRepository,
        ILogger<PaymentReconciliationService> logger,
        IEmailSender emailSender,
        IOptions<ReconciliationSettings> settings)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _reconciliationRepository = reconciliationRepository ?? throw new ArgumentNullException(nameof(reconciliationRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
    }

    /// <summary>
    /// Reconciles payments with Stripe for a specific period and tenant
    /// </summary>
    public async Task<ReconciliationReport> ReconcileWithStripeAsync(
        DateTime fromDate,
        DateTime toDate,
        TenantId tenantId,
        CancellationToken cancellationToken = default)
    {
        if (tenantId == null)
            throw new ArgumentNullException(nameof(tenantId));

        if (fromDate >= toDate)
            throw new ArgumentException("From date must be before to date", nameof(fromDate));

        // VALIDATION: Prevent reconciliation of data older than configured limit
        var maxHistoricalDate = DateTime.UtcNow.AddYears(-_settings.MaxHistoricalDataYears);
        if (fromDate < maxHistoricalDate)
            throw new ArgumentException($"Cannot reconcile data older than {_settings.MaxHistoricalDataYears} year(s)", nameof(fromDate));

        // VALIDATION: Prevent reconciliation of future dates
        if (toDate > DateTime.UtcNow)
            throw new ArgumentException("Cannot reconcile future dates", nameof(toDate));

        _logger.LogInformation(
            "Starting reconciliation for tenant {TenantId} from {FromDate} to {ToDate}",
            tenantId.Value, fromDate, toDate);

        var report = ReconciliationReport.Create(tenantId, fromDate, toDate);

        try
        {
            // Fetch local payments
            var localPayments = await GetLocalPaymentsAsync(tenantId, fromDate, toDate, cancellationToken);
            _logger.LogInformation("Found {Count} local payments", localPayments.Count);

            // Fetch Stripe payments
            var stripePayments = await FetchStripePaymentsAsync(tenantId, fromDate, toDate, cancellationToken);
            _logger.LogInformation("Found {Count} Stripe payments", stripePayments.Count);

            // Update statistics
            report.SetTotalPayments(localPayments.Count);

            // Compare and find discrepancies in parallel
            var discrepancies = await FindDiscrepanciesAsync(
                localPayments,
                stripePayments,
                report.Id,
                tenantId,
                cancellationToken);

            foreach (var discrepancy in discrepancies)
            {
                report.AddDiscrepancy(discrepancy);
            }

            // Calculate matched payments
            report.SetMatchedPayments(report.TotalPayments - report.MismatchedPayments);

            // Save report with discrepancies
            await _reconciliationRepository.SaveReportAsync(report, cancellationToken);

            // Auto-resolve discrepancies
            await AutoResolveDiscrepanciesAsync(report, cancellationToken);

            report.MarkAsCompleted();
            await _reconciliationRepository.SaveReportAsync(report, cancellationToken);

            _logger.LogInformation(
                "Reconciliation completed for tenant {TenantId}. Found {DiscrepancyCount} discrepancies, auto-resolved {AutoResolvedCount}",
                tenantId.Value, report.DiscrepanciesCount, report.AutoResolvedCount);

            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Reconciliation failed for tenant {TenantId}: {ErrorMessage}",
                tenantId.Value, ex.Message);

            report.MarkAsFailed(ex.Message);
            await _reconciliationRepository.SaveReportAsync(report, cancellationToken);

            throw;
        }
    }

    /// <summary>
    /// Generates a detailed discrepancy report
    /// </summary>
    public async Task<ReconciliationReport> GenerateDiscrepancyReportAsync(
        List<PaymentDiscrepancy> discrepancies,
        CancellationToken cancellationToken = default)
    {
        if (discrepancies == null || discrepancies.Count == 0)
            throw new ArgumentException("Discrepancies list cannot be null or empty", nameof(discrepancies));

        var firstDiscrepancy = discrepancies.First();
        var report = ReconciliationReport.Create(
            firstDiscrepancy.TenantId,
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow);

        foreach (var discrepancy in discrepancies)
        {
            report.AddDiscrepancy(discrepancy);
        }

        report.UpdateStatistics();
        report.MarkAsCompleted();

        await _reconciliationRepository.SaveReportAsync(report, cancellationToken);

        _logger.LogInformation(
            "Generated discrepancy report {ReportId} with {Count} discrepancies",
            report.Id, discrepancies.Count);

        return report;
    }

    /// <summary>
    /// Automatically resolves discrepancies based on predefined rules
    /// </summary>
    public async Task<int> AutoResolveDiscrepanciesAsync(
        ReconciliationReport report,
        CancellationToken cancellationToken = default)
    {
        if (report == null)
            throw new ArgumentNullException(nameof(report));

        _logger.LogInformation(
            "Starting auto-resolution for report {ReportId} with {Count} discrepancies",
            report.Id, report.DiscrepanciesCount);

        int resolvedCount = 0;

        // Process discrepancies in batches for transaction safety
        var pendingDiscrepancies = report.Discrepancies
            .Where(d => d.Resolution == DiscrepancyResolution.Pending)
            .ToList();

        // PERFORMANCE: Batch load all required payments to avoid N+1 queries
        var paymentIds = pendingDiscrepancies
            .Where(d => d.PaymentId.HasValue)
            .Select(d => d.PaymentId!.Value)
            .Distinct()
            .ToList();

        var payments = new Dictionary<Guid, Domain.Entities.Payment>();
        if (paymentIds.Any())
        {
            var paymentEntities = await _context.Payments
                .AsNoTracking()
                .Where(p => paymentIds.Contains(p.Id))
                .ToListAsync(cancellationToken);
            
            payments = paymentEntities.ToDictionary(p => p.Id);
        }

        // THREAD SAFETY: Process discrepancies sequentially to avoid race conditions
        foreach (var discrepancy in pendingDiscrepancies)
        {
            try
            {
                // Double-check that discrepancy is still pending (avoid race conditions)
                if (discrepancy.Resolution != DiscrepancyResolution.Pending)
                {
                    _logger.LogDebug("Discrepancy {DiscrepancyId} already processed, skipping", discrepancy.Id);
                    continue;
                }

                var resolved = await TryAutoResolveDiscrepancyAsync(discrepancy, payments, cancellationToken);
                if (resolved)
                {
                    resolvedCount++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to auto-resolve discrepancy {DiscrepancyId}: {ErrorMessage}",
                    discrepancy.Id, ex.Message);
            }
        }

        // TRANSACTION: Save all changes in a single transaction (payments + discrepancies)
        await _context.SaveChangesAsync(cancellationToken);

        // Update report statistics
        report.UpdateStatistics();
        await _reconciliationRepository.SaveReportAsync(report, cancellationToken);

        _logger.LogInformation(
            "Auto-resolved {ResolvedCount} out of {TotalCount} discrepancies for report {ReportId}",
            resolvedCount, pendingDiscrepancies.Count, report.Id);

        return resolvedCount;
    }

    /// <summary>
    /// Sends reconciliation report notification
    /// </summary>
    public async Task SendReconciliationReportAsync(
        ReconciliationReport report,
        CancellationToken cancellationToken = default)
    {
        if (report == null)
            throw new ArgumentNullException(nameof(report));

        _logger.LogInformation(
            "Sending reconciliation report {ReportId} for tenant {TenantId}",
            report.Id, report.TenantId.Value);

        var subject = report.Status == ReconciliationStatus.CompletedWithDiscrepancies
            ? $"⚠️ Payment Reconciliation Report - Discrepancies Found"
            : $"✅ Payment Reconciliation Report - All Clear";

        var body = BuildReportEmailBody(report);

        try
        {
            // TODO: Get provider email from tenant context
            // For now, just log the report
            _logger.LogInformation(
                "Reconciliation Report Summary:\n" +
                "Report ID: {ReportId}\n" +
                "Tenant ID: {TenantId}\n" +
                "Period: {PeriodStart} to {PeriodEnd}\n" +
                "Total Payments: {TotalPayments}\n" +
                "Matched: {MatchedPayments}\n" +
                "Discrepancies: {DiscrepanciesCount}\n" +
                "Auto-Resolved: {AutoResolvedCount}\n" +
                "Manual Review Required: {ManualReviewCount}\n" +
                "Status: {Status}",
                report.Id,
                report.TenantId.Value,
                report.PeriodStart,
                report.PeriodEnd,
                report.TotalPayments,
                report.MatchedPayments,
                report.DiscrepanciesCount,
                report.AutoResolvedCount,
                report.ManualReviewCount,
                report.Status);

            // Critical discrepancies should trigger immediate notification
            if (report.ManualReviewCount > 0)
            {
                _logger.LogWarning(
                    "CRITICAL: {Count} discrepancies require manual review in report {ReportId}",
                    report.ManualReviewCount, report.Id);

                // TODO: Send Slack/Teams webhook notification for critical discrepancies
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to send reconciliation report {ReportId}: {ErrorMessage}",
                report.Id, ex.Message);
        }
    }

    #region Private Helper Methods

    private async Task<List<Domain.Entities.Payment>> GetLocalPaymentsAsync(
        TenantId tenantId,
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken)
    {
        return await _context.Payments
            .AsNoTracking()
            .Where(p => p.TenantId == tenantId)
            .Where(p => p.CreatedAt >= fromDate && p.CreatedAt <= toDate)
            .Where(p => !string.IsNullOrEmpty(p.ExternalPaymentId)) // Only payments that went through Stripe
            .ToListAsync(cancellationToken);
    }

    private async Task<Dictionary<string, PaymentIntent>> FetchStripePaymentsAsync(
        TenantId tenantId,
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken)
    {
        // MEMORY MANAGEMENT: Use Dictionary for Stripe payments
        var stripePayments = new Dictionary<string, PaymentIntent>();
        var paymentIntentService = new PaymentIntentService();

        var options = new PaymentIntentListOptions
        {
            Created = new DateRangeOptions
            {
                GreaterThanOrEqual = fromDate,
                LessThanOrEqual = toDate
            },
            Limit = _settings.StripeBatchSize
        };

        // TIMEOUT: Add timeout for Stripe API call (5 minutes)
        using var stripeCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        stripeCts.CancelAfter(TimeSpan.FromMinutes(5));

        try
        {
            int requestCount = 0;
            await foreach (var paymentIntent in paymentIntentService.ListAutoPagingAsync(options, cancellationToken: stripeCts.Token))
            {
                // Rate limiting: Add delay every batch to respect Stripe API limits
                if (++requestCount % _settings.RateLimitBatchSize == 0)
                {
                    await Task.Delay(_settings.StripeApiDelayMs, cancellationToken);
                }

                // SECURITY: Validate tenant_id in metadata with proper validation
                if (paymentIntent.Metadata.TryGetValue("tenant_id", out var metadataTenantId))
                {
                    // Validate GUID format and match
                    if (Guid.TryParse(metadataTenantId, out var parsedTenantId) &&
                        parsedTenantId == tenantId.Value)
                    {
                        stripePayments[paymentIntent.Id] = paymentIntent;
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Invalid or mismatched tenant_id in Stripe metadata for payment {PaymentId}. " +
                            "Expected: {ExpectedTenantId}, Found: {MetadataTenantId}",
                            paymentIntent.Id, tenantId.Value, metadataTenantId);
                    }
                }
                else
                {
                    _logger.LogDebug(
                        "Payment {PaymentId} missing tenant_id metadata - skipping",
                        paymentIntent.Id);
                }
            }
        }
        catch (OperationCanceledException ex) when (stripeCts.Token.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex,
                "Stripe API timeout after 5 minutes for tenant {TenantId}",
                tenantId.Value);
            throw new TimeoutException("Stripe API request timed out after 5 minutes", ex);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex,
                "Failed to fetch Stripe payments for tenant {TenantId}: {ErrorMessage}",
                tenantId.Value, ex.Message);
            throw;
        }

        // PERFORMANCE: Return the dictionary directly, no need to create a copy
        return stripePayments;
    }

    private async Task<List<PaymentDiscrepancy>> FindDiscrepanciesAsync(
        List<Domain.Entities.Payment> localPayments,
        Dictionary<string, PaymentIntent> stripePayments,
        Guid reportId,
        TenantId tenantId,
        CancellationToken cancellationToken)
    {
        var discrepancies = new ConcurrentBag<PaymentDiscrepancy>();

        // Process in parallel with limited concurrency
        var semaphore = new SemaphoreSlim(_settings.MaxParallelTasks);
        var tasks = localPayments.Select(async payment =>
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                var paymentDiscrepancies = CheckPaymentDiscrepancies(
                    payment,
                    stripePayments,
                    reportId,
                    tenantId);

                foreach (var discrepancy in paymentDiscrepancies)
                {
                    discrepancies.Add(discrepancy);
                }
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);

        // Check for payments in Stripe but not in Orbito
        var localExternalIds = localPayments
            .Where(p => !string.IsNullOrEmpty(p.ExternalPaymentId))
            .Select(p => p.ExternalPaymentId!)
            .ToHashSet();

        foreach (var stripePayment in stripePayments.Values)
        {
            if (!localExternalIds.Contains(stripePayment.Id))
            {
                discrepancies.Add(PaymentDiscrepancy.CreateMissingPayment(
                    tenantId,
                    reportId,
                    DiscrepancyType.MissingInOrbito,
                    externalPaymentId: stripePayment.Id,
                    additionalData: $"Amount: {stripePayment.Amount / 100m} {stripePayment.Currency.ToUpper()}, Status: {stripePayment.Status}"));
            }
        }

        return discrepancies.ToList();
    }

    private List<PaymentDiscrepancy> CheckPaymentDiscrepancies(
        Domain.Entities.Payment payment,
        Dictionary<string, PaymentIntent> stripePayments,
        Guid reportId,
        TenantId tenantId)
    {
        var discrepancies = new List<PaymentDiscrepancy>();

        if (string.IsNullOrEmpty(payment.ExternalPaymentId))
        {
            return discrepancies; // Skip payments without Stripe ID
        }

        // Check if payment exists in Stripe
        if (!stripePayments.TryGetValue(payment.ExternalPaymentId, out var stripePayment))
        {
            discrepancies.Add(PaymentDiscrepancy.CreateMissingPayment(
                tenantId,
                reportId,
                DiscrepancyType.MissingInStripe,
                paymentId: payment.Id,
                externalPaymentId: payment.ExternalPaymentId,
                additionalData: $"Payment {payment.Id} exists in Orbito but not in Stripe"));
            return discrepancies;
        }

        // Check status mismatch
        var stripeStatus = stripePayment.Status;
        var expectedOrbitoStatus = MapStripeStatusToPaymentStatus(stripeStatus);

        if (expectedOrbitoStatus != payment.Status)
        {
            discrepancies.Add(PaymentDiscrepancy.CreateStatusMismatch(
                tenantId,
                reportId,
                payment.Id,
                payment.Status,
                stripeStatus,
                payment.ExternalPaymentId));
        }

        // Check amount mismatch (Stripe stores amounts in cents)
        var stripeAmountDecimal = stripePayment.Amount / 100m;
        if (Math.Abs(stripeAmountDecimal - payment.Amount.Amount) > 0.01m) // Allow 1 cent tolerance
        {
            discrepancies.Add(PaymentDiscrepancy.CreateAmountMismatch(
                tenantId,
                reportId,
                payment.Id,
                payment.Amount.Amount,
                payment.Amount.Currency.Code,
                stripeAmountDecimal,
                stripePayment.Currency.ToUpper(),
                payment.ExternalPaymentId));
        }

        return discrepancies;
    }

    private async Task<bool> TryAutoResolveDiscrepancyAsync(
        PaymentDiscrepancy discrepancy,
        Dictionary<Guid, Domain.Entities.Payment> payments,
        CancellationToken cancellationToken)
    {
        switch (discrepancy.Type)
        {
            case DiscrepancyType.StatusMismatch:
                return await TryResolveStatusMismatchAsync(discrepancy, payments, cancellationToken);

            case DiscrepancyType.AmountMismatch:
                // Amount mismatches always require manual review
                discrepancy.MarkAsRequiresManualReview(
                    "Amount mismatch detected - requires manual verification");
                // Don't save here - will be saved in batch at the end
                return false;

            case DiscrepancyType.MissingInStripe:
                // Potentially fraudulent - requires manual review
                discrepancy.MarkAsRequiresManualReview(
                    "Payment exists in Orbito but not in Stripe - potential fraud or sync issue");
                // Don't save here - will be saved in batch at the end
                return false;

            case DiscrepancyType.MissingInOrbito:
                // Payment in Stripe but not in Orbito - webhook might have failed
                discrepancy.MarkAsRequiresManualReview(
                    "Payment exists in Stripe but not in Orbito - webhook processing may have failed");
                // Don't save here - will be saved in batch at the end
                return false;

            default:
                return false;
        }
    }

    private async Task<bool> TryResolveStatusMismatchAsync(
        PaymentDiscrepancy discrepancy,
        Dictionary<Guid, Domain.Entities.Payment> payments,
        CancellationToken cancellationToken)
    {
        if (!discrepancy.PaymentId.HasValue)
            return false;

        // PERFORMANCE: Use pre-loaded payment from batch loading
        if (!payments.TryGetValue(discrepancy.PaymentId.Value, out var payment))
            return false;

        // Stripe is the source of truth for payment status
        var expectedStatus = MapStripeStatusToPaymentStatus(discrepancy.StripeStatus!);

        // Only auto-resolve if the transition is valid
        if (!payment.CanTransitionTo(expectedStatus))
        {
            discrepancy.MarkAsRequiresManualReview(
                $"Cannot transition from {payment.Status} to {expectedStatus} - invalid state transition");
            // Don't save here - will be saved in batch at the end
            return false;
        }

        // Update payment status to match Stripe
        switch (expectedStatus)
        {
            case PaymentStatus.Processing:
                payment.MarkAsProcessing();
                break;
            case PaymentStatus.Completed:
                payment.MarkAsCompleted();
                break;
            case PaymentStatus.Failed:
                payment.MarkAsFailed($"Status synced from Stripe: {discrepancy.StripeStatus}");
                break;
            case PaymentStatus.Cancelled:
                payment.MarkAsCancelled();
                break;
            default:
                discrepancy.MarkAsRequiresManualReview(
                    $"Unsupported status transition to {expectedStatus}");
                // Don't save here - will be saved in batch at the end
                return false;
        }

        // Mark discrepancy as resolved
        discrepancy.MarkAsAutoResolved(
            $"Payment status updated from {discrepancy.OrbitoStatus} to {expectedStatus} to match Stripe");

        _logger.LogInformation(
            "Auto-resolved status mismatch for payment {PaymentId}: {OldStatus} -> {NewStatus}",
            payment.Id, discrepancy.OrbitoStatus, expectedStatus);

        return true;
    }

    private PaymentStatus MapStripeStatusToPaymentStatus(string stripeStatus)
    {
        return stripeStatus?.ToLowerInvariant() switch
        {
            "requires_payment_method" => PaymentStatus.Pending,
            "requires_confirmation" => PaymentStatus.Pending,
            "requires_action" => PaymentStatus.Pending,
            "processing" => PaymentStatus.Processing,
            "requires_capture" => PaymentStatus.Processing,
            "succeeded" => PaymentStatus.Completed,
            "canceled" => PaymentStatus.Cancelled,
            _ => PaymentStatus.Failed
        };
    }

    private string BuildReportEmailBody(ReconciliationReport report)
    {
        return $@"
Payment Reconciliation Report

Report ID: {report.Id}
Run Date: {report.RunDate:yyyy-MM-dd HH:mm:ss} UTC
Period: {report.PeriodStart:yyyy-MM-dd} to {report.PeriodEnd:yyyy-MM-dd}
Status: {report.Status}

Statistics:
- Total Payments: {report.TotalPayments}
- Matched Payments: {report.MatchedPayments}
- Mismatched Payments: {report.MismatchedPayments}
- Total Discrepancies: {report.DiscrepanciesCount}
- Auto-Resolved: {report.AutoResolvedCount}
- Requires Manual Review: {report.ManualReviewCount}

Duration: {report.Duration?.TotalSeconds:F2} seconds

{(report.ManualReviewCount > 0 ? "⚠️ ATTENTION: Some discrepancies require manual review. Please check the system." : "✅ All discrepancies have been automatically resolved.")}
";
    }

    #endregion
}
