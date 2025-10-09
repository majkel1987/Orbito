using Orbito.Domain.Entities;

namespace Orbito.API.Extensions;

/// <summary>
/// Extension methods for mapping reconciliation entities to DTOs
/// </summary>
public static class ReconciliationExtensions
{
    /// <summary>
    /// Maps ReconciliationReport to ReconciliationReportDto
    /// </summary>
    public static ReconciliationReportDto ToDto(this ReconciliationReport report)
    {
        if (report == null)
            throw new ArgumentNullException(nameof(report));

        return new ReconciliationReportDto
        {
            Id = report.Id,
            TenantId = report.TenantId.Value,
            RunDate = report.RunDate,
            PeriodStart = report.PeriodStart,
            PeriodEnd = report.PeriodEnd,
            Status = report.Status.ToString(),
            TotalPayments = report.TotalPayments,
            MatchedPayments = report.MatchedPayments,
            MismatchedPayments = report.MismatchedPayments,
            DiscrepanciesCount = report.DiscrepanciesCount,
            AutoResolvedCount = report.AutoResolvedCount,
            ManualReviewCount = report.ManualReviewCount,
            Duration = report.Duration,
            ErrorMessage = report.ErrorMessage
        };
    }

    /// <summary>
    /// Maps PaymentDiscrepancy to PaymentDiscrepancyDto
    /// </summary>
    public static PaymentDiscrepancyDto ToDto(this PaymentDiscrepancy discrepancy)
    {
        if (discrepancy == null)
            throw new ArgumentNullException(nameof(discrepancy));

        return new PaymentDiscrepancyDto
        {
            Id = discrepancy.Id,
            ReconciliationReportId = discrepancy.ReconciliationReportId,
            PaymentId = discrepancy.PaymentId,
            ExternalPaymentId = discrepancy.ExternalPaymentId,
            Type = discrepancy.Type.ToString(),
            Resolution = discrepancy.Resolution.ToString(),
            OrbitoStatus = discrepancy.OrbitoStatus?.ToString(),
            StripeStatus = discrepancy.StripeStatus,
            OrbitoAmount = discrepancy.OrbitoAmount,
            OrbitoCurrency = discrepancy.OrbitoCurrency,
            StripeAmount = discrepancy.StripeAmount,
            StripeCurrency = discrepancy.StripeCurrency,
            ResolutionNotes = discrepancy.ResolutionNotes,
            ResolvedAt = discrepancy.ResolvedAt,
            ResolvedBy = discrepancy.ResolvedBy,
            AdditionalData = discrepancy.AdditionalData,
            DetectedAt = discrepancy.DetectedAt
        };
    }

    /// <summary>
    /// Maps collection of ReconciliationReport to DTOs
    /// </summary>
    public static IEnumerable<ReconciliationReportDto> ToDto(this IEnumerable<ReconciliationReport> reports)
    {
        return reports.Select(r => r.ToDto());
    }

    /// <summary>
    /// Maps collection of PaymentDiscrepancy to DTOs
    /// </summary>
    public static IEnumerable<PaymentDiscrepancyDto> ToDto(this IEnumerable<PaymentDiscrepancy> discrepancies)
    {
        return discrepancies.Select(d => d.ToDto());
    }
}

/// <summary>
/// DTO for reconciliation report
/// </summary>
public class ReconciliationReportDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public DateTime RunDate { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public string Status { get; set; } = string.Empty;
    public int TotalPayments { get; set; }
    public int MatchedPayments { get; set; }
    public int MismatchedPayments { get; set; }
    public int DiscrepanciesCount { get; set; }
    public int AutoResolvedCount { get; set; }
    public int ManualReviewCount { get; set; }
    public TimeSpan? Duration { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// DTO for payment discrepancy
/// </summary>
public class PaymentDiscrepancyDto
{
    public Guid Id { get; set; }
    public Guid ReconciliationReportId { get; set; }
    public Guid? PaymentId { get; set; }
    public string? ExternalPaymentId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Resolution { get; set; } = string.Empty;
    public string? OrbitoStatus { get; set; }
    public string? StripeStatus { get; set; }
    public decimal? OrbitoAmount { get; set; }
    public string? OrbitoCurrency { get; set; }
    public decimal? StripeAmount { get; set; }
    public string? StripeCurrency { get; set; }
    public string? ResolutionNotes { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolvedBy { get; set; }
    public string? AdditionalData { get; set; }
    public DateTime DetectedAt { get; set; }
}
