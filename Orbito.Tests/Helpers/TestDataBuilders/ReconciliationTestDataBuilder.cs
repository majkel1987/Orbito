using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;

namespace Orbito.Tests.Helpers.TestDataBuilders;

/// <summary>
/// Builder for creating ReconciliationReport and PaymentDiscrepancy test data
/// </summary>
public class ReconciliationTestDataBuilder
{
    private TenantId _tenantId = TenantId.New();
    private DateTime _periodStart = DateTime.UtcNow.AddDays(-30);
    private DateTime _periodEnd = DateTime.UtcNow;
    private ReconciliationStatus _status = ReconciliationStatus.InProgress;
    private int _totalPayments = 0;
    private int _matchedPayments = 0;
    private int _mismatchedPayments = 0;
    private int _discrepanciesCount = 0;
    private int _autoResolvedCount = 0;
    private int _manualReviewCount = 0;
    private DateTime? _startedAt;
    private DateTime? _completedAt;
    private TimeSpan? _duration;
    private string? _errorMessage;

    public static ReconciliationTestDataBuilder Create() => new();

    public ReconciliationTestDataBuilder WithTenantId(TenantId tenantId)
    {
        _tenantId = tenantId;
        return this;
    }

    public ReconciliationTestDataBuilder WithPeriod(DateTime start, DateTime end)
    {
        _periodStart = start;
        _periodEnd = end;
        return this;
    }

    public ReconciliationTestDataBuilder WithStatus(ReconciliationStatus status)
    {
        _status = status;
        return this;
    }

    public ReconciliationTestDataBuilder WithStatistics(int total, int matched, int mismatched, int discrepancies)
    {
        _totalPayments = total;
        _matchedPayments = matched;
        _mismatchedPayments = mismatched;
        _discrepanciesCount = discrepancies;
        return this;
    }

    public ReconciliationTestDataBuilder WithResolutionCounts(int autoResolved, int manualReview)
    {
        _autoResolvedCount = autoResolved;
        _manualReviewCount = manualReview;
        return this;
    }

    public ReconciliationTestDataBuilder WithExecutionDetails(DateTime? startedAt, DateTime? completedAt, TimeSpan? duration)
    {
        _startedAt = startedAt;
        _completedAt = completedAt;
        _duration = duration;
        return this;
    }

    public ReconciliationTestDataBuilder WithErrorMessage(string errorMessage)
    {
        _errorMessage = errorMessage;
        return this;
    }

    public ReconciliationReport Build()
    {
        var report = ReconciliationReport.Create(_tenantId, _periodStart, _periodEnd);

        // Override properties using reflection for testing
        var statusProperty = typeof(ReconciliationReport).GetProperty("Status");
        statusProperty?.SetValue(report, _status);

        var totalPaymentsProperty = typeof(ReconciliationReport).GetProperty("TotalPayments");
        totalPaymentsProperty?.SetValue(report, _totalPayments);

        var matchedPaymentsProperty = typeof(ReconciliationReport).GetProperty("MatchedPayments");
        matchedPaymentsProperty?.SetValue(report, _matchedPayments);

        var mismatchedPaymentsProperty = typeof(ReconciliationReport).GetProperty("MismatchedPayments");
        mismatchedPaymentsProperty?.SetValue(report, _mismatchedPayments);

        var discrepanciesCountProperty = typeof(ReconciliationReport).GetProperty("DiscrepanciesCount");
        discrepanciesCountProperty?.SetValue(report, _discrepanciesCount);

        var autoResolvedCountProperty = typeof(ReconciliationReport).GetProperty("AutoResolvedCount");
        autoResolvedCountProperty?.SetValue(report, _autoResolvedCount);

        var manualReviewCountProperty = typeof(ReconciliationReport).GetProperty("ManualReviewCount");
        manualReviewCountProperty?.SetValue(report, _manualReviewCount);

        var startedAtProperty = typeof(ReconciliationReport).GetProperty("StartedAt");
        startedAtProperty?.SetValue(report, _startedAt);

        var completedAtProperty = typeof(ReconciliationReport).GetProperty("CompletedAt");
        completedAtProperty?.SetValue(report, _completedAt);

        var durationProperty = typeof(ReconciliationReport).GetProperty("Duration");
        durationProperty?.SetValue(report, _duration);

        var errorMessageProperty = typeof(ReconciliationReport).GetProperty("ErrorMessage");
        errorMessageProperty?.SetValue(report, _errorMessage);

        return report;
    }

    // Predefined scenarios for common test cases
    public static ReconciliationReport InProgressReport()
        => Create().Build();

    public static ReconciliationReport CompletedReport()
        => Create()
            .WithStatus(ReconciliationStatus.Completed)
            .WithStatistics(100, 95, 5, 5)
            .WithExecutionDetails(DateTime.UtcNow.AddMinutes(-10), DateTime.UtcNow, TimeSpan.FromMinutes(10))
            .Build();

    public static ReconciliationReport CompletedWithDiscrepanciesReport()
        => Create()
            .WithStatus(ReconciliationStatus.CompletedWithDiscrepancies)
            .WithStatistics(100, 90, 10, 10)
            .WithResolutionCounts(5, 5)
            .WithExecutionDetails(DateTime.UtcNow.AddMinutes(-15), DateTime.UtcNow, TimeSpan.FromMinutes(15))
            .Build();

    public static ReconciliationReport FailedReport()
        => Create()
            .WithStatus(ReconciliationStatus.Failed)
            .WithErrorMessage("Stripe API connection failed")
            .WithExecutionDetails(DateTime.UtcNow.AddMinutes(-5), DateTime.UtcNow, TimeSpan.FromMinutes(5))
            .Build();

    public static ReconciliationReport EmptyReport()
        => Create()
            .WithStatus(ReconciliationStatus.Completed)
            .WithStatistics(0, 0, 0, 0)
            .WithExecutionDetails(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow, TimeSpan.FromMinutes(1))
            .Build();
}

/// <summary>
/// Builder for creating PaymentDiscrepancy test data
/// </summary>
public class PaymentDiscrepancyTestDataBuilder
{
    private TenantId _tenantId = TenantId.New();
    private Guid _reconciliationReportId = Guid.NewGuid();
    private Guid? _paymentId = Guid.NewGuid();
    private string? _externalPaymentId = "ch_test_123";
    private DiscrepancyType _type = DiscrepancyType.StatusMismatch;
    private DiscrepancyResolution _resolution = DiscrepancyResolution.Pending;
    private PaymentStatus? _orbitStatus = PaymentStatus.Completed;
    private string? _stripeStatus = "succeeded";
    private decimal? _orbitAmount = 100.00m;
    private string? _orbitCurrency = "USD";
    private decimal? _stripeAmount = 100.00m;
    private string? _stripeCurrency = "USD";
    private string? _resolutionNotes;
    private DateTime? _resolvedAt;
    private string? _resolvedBy;
    private string? _additionalData;
    private DateTime _detectedAt = DateTime.UtcNow;

    public static PaymentDiscrepancyTestDataBuilder Create() => new();

    public PaymentDiscrepancyTestDataBuilder WithTenantId(TenantId tenantId)
    {
        _tenantId = tenantId;
        return this;
    }

    public PaymentDiscrepancyTestDataBuilder WithReconciliationReportId(Guid reportId)
    {
        _reconciliationReportId = reportId;
        return this;
    }

    public PaymentDiscrepancyTestDataBuilder WithPaymentId(Guid? paymentId)
    {
        _paymentId = paymentId;
        return this;
    }

    public PaymentDiscrepancyTestDataBuilder WithExternalPaymentId(string? externalPaymentId)
    {
        _externalPaymentId = externalPaymentId;
        return this;
    }

    public PaymentDiscrepancyTestDataBuilder WithType(DiscrepancyType type)
    {
        _type = type;
        return this;
    }

    public PaymentDiscrepancyTestDataBuilder WithResolution(DiscrepancyResolution resolution)
    {
        _resolution = resolution;
        return this;
    }

    public PaymentDiscrepancyTestDataBuilder WithStatusMismatch(PaymentStatus orbitStatus, string stripeStatus)
    {
        _type = DiscrepancyType.StatusMismatch;
        _orbitStatus = orbitStatus;
        _stripeStatus = stripeStatus;
        return this;
    }

    public PaymentDiscrepancyTestDataBuilder WithAmountMismatch(decimal orbitAmount, string orbitCurrency, decimal stripeAmount, string stripeCurrency)
    {
        _type = DiscrepancyType.AmountMismatch;
        _orbitAmount = orbitAmount;
        _orbitCurrency = orbitCurrency;
        _stripeAmount = stripeAmount;
        _stripeCurrency = stripeCurrency;
        return this;
    }

    public PaymentDiscrepancyTestDataBuilder WithResolutionDetails(string notes, DateTime? resolvedAt, string? resolvedBy)
    {
        _resolutionNotes = notes;
        _resolvedAt = resolvedAt;
        _resolvedBy = resolvedBy;
        return this;
    }

    public PaymentDiscrepancyTestDataBuilder WithAdditionalData(string additionalData)
    {
        _additionalData = additionalData;
        return this;
    }

    public PaymentDiscrepancyTestDataBuilder WithDetectedAt(DateTime detectedAt)
    {
        _detectedAt = detectedAt;
        return this;
    }

    public PaymentDiscrepancy Build()
    {
        return _type switch
        {
            DiscrepancyType.StatusMismatch => PaymentDiscrepancy.CreateStatusMismatch(
                _tenantId,
                _reconciliationReportId,
                _paymentId!.Value,
                _orbitStatus!.Value,
                _stripeStatus!,
                _externalPaymentId),

            DiscrepancyType.AmountMismatch => PaymentDiscrepancy.CreateAmountMismatch(
                _tenantId,
                _reconciliationReportId,
                _paymentId!.Value,
                _orbitAmount!.Value,
                _orbitCurrency!,
                _stripeAmount!.Value,
                _stripeCurrency!,
                _externalPaymentId),

            DiscrepancyType.MissingInStripe => PaymentDiscrepancy.CreateMissingPayment(
                _tenantId,
                _reconciliationReportId,
                DiscrepancyType.MissingInStripe,
                _paymentId,
                _externalPaymentId,
                _additionalData),

            DiscrepancyType.MissingInOrbito => PaymentDiscrepancy.CreateMissingPayment(
                _tenantId,
                _reconciliationReportId,
                DiscrepancyType.MissingInOrbito,
                _paymentId,
                _externalPaymentId,
                _additionalData),

            _ => throw new ArgumentException($"Unsupported discrepancy type: {_type}")
        };
    }

    // Predefined scenarios for common test cases
    public static PaymentDiscrepancy StatusMismatch()
        => Create()
            .WithStatusMismatch(PaymentStatus.Completed, "failed")
            .Build();

    public static PaymentDiscrepancy AmountMismatch()
        => Create()
            .WithAmountMismatch(100.00m, "USD", 99.00m, "USD")
            .Build();

    public static PaymentDiscrepancy MissingInStripe()
        => Create()
            .WithType(DiscrepancyType.MissingInStripe)
            .Build();

    public static PaymentDiscrepancy MissingInOrbito()
        => Create()
            .WithType(DiscrepancyType.MissingInOrbito)
            .WithExternalPaymentId("ch_stripe_only_123")
            .Build();

    public static PaymentDiscrepancy AutoResolvedDiscrepancy()
        => Create()
            .WithStatusMismatch(PaymentStatus.Completed, "succeeded")
            .WithResolution(DiscrepancyResolution.AutoResolved)
            .WithResolutionDetails("Status updated to match Stripe", DateTime.UtcNow, "System")
            .Build();

    public static PaymentDiscrepancy ManualReviewDiscrepancy()
        => Create()
            .WithAmountMismatch(100.00m, "USD", 50.00m, "USD")
            .WithResolution(DiscrepancyResolution.RequiresManualReview)
            .Build();

    public static PaymentDiscrepancy ManuallyResolvedDiscrepancy()
        => Create()
            .WithStatusMismatch(PaymentStatus.Failed, "succeeded")
            .WithResolution(DiscrepancyResolution.ManuallyResolved)
            .WithResolutionDetails("Manually verified and corrected", DateTime.UtcNow, "admin@example.com")
            .Build();

    public static PaymentDiscrepancy IgnoredDiscrepancy()
        => Create()
            .WithType(DiscrepancyType.MissingInStripe)
            .WithResolution(DiscrepancyResolution.Ignored)
            .WithResolutionDetails("Test payment, can be ignored", DateTime.UtcNow, "admin@example.com")
            .Build();
}
