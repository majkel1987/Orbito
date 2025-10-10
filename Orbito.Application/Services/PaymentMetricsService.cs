using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Constants;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Common.Models;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;

namespace Orbito.Application.Services;

/// <summary>
/// Service for advanced payment metrics and statistics
/// </summary>
public class PaymentMetricsService : IPaymentMetricsService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<PaymentMetricsService> _logger;
    private readonly ICacheService _cacheService;

    // Cache TTL constants - using centralized constants
    private static readonly TimeSpan ShortCacheTtl = CacheConstants.ShortCacheTtl;
    private static readonly TimeSpan MediumCacheTtl = CacheConstants.MediumCacheTtl;
    private static readonly TimeSpan LongCacheTtl = CacheConstants.LongCacheTtl;

    public PaymentMetricsService(
        IPaymentRepository paymentRepository,
        ITenantContext tenantContext,
        ILogger<PaymentMetricsService> logger,
        ICacheService cacheService)
    {
        _paymentRepository = paymentRepository;
        _tenantContext = tenantContext;
        _logger = logger;
        _cacheService = cacheService;
    }

    /// <summary>
    /// Gets payment success rate for a specific date range and optional provider
    /// </summary>
    public async Task<decimal> GetPaymentSuccessRateAsync(DateRange range, Guid? providerId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_tenantContext.HasTenant)
            {
                _logger.LogWarning("Cannot calculate success rate: Tenant context is required");
                return 0;
            }

            var tenantId = _tenantContext.CurrentTenantId!;
            var cacheKey = $"{CacheConstants.KeyPrefixes.PaymentSuccessRate}_{tenantId}_{range.StartDate:yyyyMMdd}_{range.EndDate:yyyyMMdd}_{providerId}";

            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                // SECURITY & PERFORMANCE: Use secure method with SQL-level filtering
                var payments = await _paymentRepository.GetPaymentsForMetricsAsync(
                    tenantId,
                    range.StartDate,
                    range.EndDate,
                    providerId,
                    cancellationToken);

                // Optimize query - only get status counts, not full entities
                var statusCounts = await payments
                    .GroupBy(p => p.Status)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToListAsync(cancellationToken);

                if (!statusCounts.Any())
                {
                    _logger.LogInformation("No payments found for success rate calculation in period {StartDate} to {EndDate}",
                        range.StartDate, range.EndDate);
                    return 0m;
                }

                var totalPayments = statusCounts.Sum(s => s.Count);
                var successfulPayments = statusCounts.FirstOrDefault(s => s.Status == PaymentStatus.Completed)?.Count ?? 0;

                var successRate = totalPayments > 0 ? (decimal)successfulPayments / totalPayments * 100 : 0;

                _logger.LogInformation("Calculated success rate: {SuccessRate}% for {TotalPayments} payments in period {StartDate} to {EndDate}",
                    successRate, totalPayments, range.StartDate, range.EndDate);

                return Math.Round(successRate, 2);
            }, ShortCacheTtl, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating payment success rate for period {StartDate} to {EndDate}",
                range.StartDate, range.EndDate);
            return 0;
        }
    }

    /// <summary>
    /// Gets average payment processing time for a specific date range
    /// </summary>
    public async Task<decimal> GetAverageProcessingTimeAsync(DateRange range, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_tenantContext.HasTenant)
            {
                _logger.LogWarning("Cannot calculate processing time: Tenant context is required");
                return 0;
            }

            var tenantId = _tenantContext.CurrentTenantId!;

            // SECURITY & PERFORMANCE: Use secure method with SQL-level filtering
            var payments = await _paymentRepository.GetPaymentsForMetricsAsync(
                tenantId,
                range.StartDate,
                range.EndDate,
                null,
                cancellationToken);

            var completedPayments = await payments
                .Where(p => p.Status == PaymentStatus.Completed && p.ProcessedAt.HasValue)
                .Select(p => new { p.CreatedAt, p.ProcessedAt })
                .ToListAsync(cancellationToken);

            if (!completedPayments.Any())
            {
                _logger.LogInformation("No completed payments found for processing time calculation in period {StartDate} to {EndDate}", 
                    range.StartDate, range.EndDate);
                return 0;
            }

            var totalProcessingTime = completedPayments
                .Sum(p => (p.ProcessedAt!.Value - p.CreatedAt).TotalSeconds);

            var averageProcessingTime = totalProcessingTime / completedPayments.Count;

            _logger.LogInformation("Calculated average processing time: {AverageTime} seconds for {PaymentCount} payments in period {StartDate} to {EndDate}",
                averageProcessingTime, completedPayments.Count, range.StartDate, range.EndDate);

            return Math.Round((decimal)averageProcessingTime, 2);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating average processing time for period {StartDate} to {EndDate}", 
                range.StartDate, range.EndDate);
            return 0;
        }
    }

    /// <summary>
    /// Gets breakdown of failure reasons for a specific date range and optional provider
    /// </summary>
    public async Task<Dictionary<string, int>> GetFailureReasonsBreakdownAsync(DateRange range, Guid? providerId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_tenantContext.HasTenant)
            {
                _logger.LogWarning("Cannot get failure reasons: Tenant context is required");
                return new Dictionary<string, int>();
            }

            var tenantId = _tenantContext.CurrentTenantId!;

            // SECURITY & PERFORMANCE: Use secure method with SQL-level filtering
            var payments = await _paymentRepository.GetPaymentsForMetricsAsync(
                tenantId,
                range.StartDate,
                range.EndDate,
                providerId,
                cancellationToken);

            var failedPayments = await payments
                .Where(p => p.Status == PaymentStatus.Failed && !string.IsNullOrEmpty(p.FailureReason))
                .Select(p => p.FailureReason)
                .ToListAsync(cancellationToken);

            var failureReasons = failedPayments
                .GroupBy(fr => fr!)
                .ToDictionary(g => g.Key, g => g.Count());

            _logger.LogInformation("Found {FailureCount} failure reasons for period {StartDate} to {EndDate}",
                failureReasons.Count, range.StartDate, range.EndDate);

            return failureReasons;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting failure reasons breakdown for period {StartDate} to {EndDate}", 
                range.StartDate, range.EndDate);
            return new Dictionary<string, int>();
        }
    }

    /// <summary>
    /// Gets revenue metrics for a specific provider and date range
    /// </summary>
    public async Task<RevenueMetrics> GetRevenueMetricsAsync(DateRange range, Guid providerId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_tenantContext.HasTenant)
            {
                _logger.LogWarning("Cannot get revenue metrics: Tenant context is required");
                return new RevenueMetrics { Period = range, ProviderId = providerId };
            }

            var tenantId = _tenantContext.CurrentTenantId!;

            // SECURITY & PERFORMANCE: Use secure method with SQL-level filtering
            var payments = await _paymentRepository.GetPaymentsForMetricsAsync(
                tenantId,
                range.StartDate,
                range.EndDate,
                providerId,
                cancellationToken);

            var completedPayments = await payments
                .Where(p => p.Status == PaymentStatus.Completed && p.Amount != null)
                .Select(p => new { p.Amount, p.PaymentMethod })
                .ToListAsync(cancellationToken);

            if (!completedPayments.Any())
            {
                _logger.LogInformation("No completed payments found for revenue metrics for provider {ProviderId} in period {StartDate} to {EndDate}",
                    providerId, range.StartDate, range.EndDate);
                return new RevenueMetrics
                {
                    Period = range,
                    ProviderId = providerId,
                    Currency = "USD" // Default currency
                };
            }

            // SECURITY FIX: Check for mixed currencies - prevent invalid calculations
            var currencies = completedPayments
                .Select(p => p.Amount!.Currency)
                .Distinct()
                .ToList();

            // Calculate revenue by currency (safe for mixed currencies)
            var revenueByCurrency = completedPayments
                .GroupBy(p => p.Amount!.Currency)
                .ToDictionary(g => g.Key.ToString(), g => g.Sum(p => p.Amount!.Amount));

            // Use primary currency (most common) for total revenue
            var primaryCurrency = completedPayments
                .GroupBy(p => p.Amount!.Currency)
                .OrderByDescending(g => g.Count())
                .First()
                .Key;

            // CRITICAL FIX: Only sum payments in primary currency for TotalRevenue
            var totalRevenue = completedPayments
                .Where(p => p.Amount!.Currency == primaryCurrency)
                .Sum(p => p.Amount!.Amount);

            // Calculate revenue by payment method (only primary currency)
            var revenueByPaymentMethod = completedPayments
                .Where(p => p.Amount!.Currency == primaryCurrency && !string.IsNullOrEmpty(p.PaymentMethod))
                .GroupBy(p => p.PaymentMethod!)
                .ToDictionary(g => g.Key, g => g.Sum(p => p.Amount!.Amount));

            var paymentsInPrimaryCurrency = completedPayments
                .Count(p => p.Amount!.Currency == primaryCurrency);

            var averageRevenuePerPayment = paymentsInPrimaryCurrency > 0
                ? totalRevenue / paymentsInPrimaryCurrency
                : 0;

            // Log warning if mixed currencies detected
            if (currencies.Count > 1)
            {
                _logger.LogWarning("Mixed currencies detected in revenue metrics for provider {ProviderId}. Using {Currency} as primary. Found currencies: {Currencies}",
                    providerId, primaryCurrency, string.Join(", ", currencies));
            }

            var metrics = new RevenueMetrics
            {
                TotalRevenue = totalRevenue,
                Currency = primaryCurrency,
                GrowthPercentage = 0, // TODO: Implement growth calculation with previous period
                RevenueByCurrency = revenueByCurrency,
                AverageRevenuePerPayment = averageRevenuePerPayment,
                SuccessfulPaymentsCount = completedPayments.Count,
                RevenueByPaymentMethod = revenueByPaymentMethod,
                MonthlyRecurringRevenue = 0, // TODO: Calculate MRR for subscription-based revenue
                Period = range,
                ProviderId = providerId
            };

            _logger.LogInformation("Calculated revenue metrics for provider {ProviderId}: {TotalRevenue} {Currency} from {PaymentCount} payments ({PrimaryCurrencyCount} in primary currency) in period {StartDate} to {EndDate}",
                providerId, totalRevenue, primaryCurrency, completedPayments.Count, paymentsInPrimaryCurrency, range.StartDate, range.EndDate);

            return metrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting revenue metrics for provider {ProviderId} in period {StartDate} to {EndDate}", 
                providerId, range.StartDate, range.EndDate);
            return new RevenueMetrics { Period = range, ProviderId = providerId };
        }
    }

    /// <summary>
    /// Gets comprehensive payment statistics for a specific date range
    /// </summary>
    public async Task<PaymentStatistics> GetPaymentStatisticsAsync(DateRange range, Guid? providerId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_tenantContext.HasTenant)
            {
                _logger.LogWarning("Cannot get payment statistics: Tenant context is required");
                return new PaymentStatistics { Period = range, ProviderId = providerId };
            }

            var tenantId = _tenantContext.CurrentTenantId!;

            // SECURITY & PERFORMANCE: Use secure method with SQL-level filtering
            var payments = await _paymentRepository.GetPaymentsForMetricsAsync(
                tenantId,
                range.StartDate,
                range.EndDate,
                providerId,
                cancellationToken);

            var filteredPayments = await payments
                .Select(p => new
                {
                    p.Status,
                    p.Amount,
                    p.PaymentMethod,
                    p.FailureReason,
                    p.CreatedAt,
                    p.ProcessedAt
                })
                .ToListAsync(cancellationToken);

            if (!filteredPayments.Any())
            {
                _logger.LogInformation("No payments found for statistics in period {StartDate} to {EndDate}", 
                    range.StartDate, range.EndDate);
                return new PaymentStatistics 
                { 
                    Period = range, 
                    ProviderId = providerId,
                    Currency = "USD" // Default currency
                };
            }

            var totalPayments = filteredPayments.Count;
            var completedPayments = filteredPayments.Count(p => p.Status == PaymentStatus.Completed);
            var failedPayments = filteredPayments.Count(p => p.Status == PaymentStatus.Failed);
            var pendingPayments = filteredPayments.Count(p => p.Status == PaymentStatus.Pending);
            var processingPayments = filteredPayments.Count(p => p.Status == PaymentStatus.Processing);
            var refundedPayments = filteredPayments.Count(p => p.Status == PaymentStatus.Refunded || p.Status == PaymentStatus.PartiallyRefunded);

            var successRate = totalPayments > 0 ? (decimal)completedPayments / totalPayments * 100 : 0;

            // Calculate total revenue (with currency safety)
            var completedWithAmount = filteredPayments
                .Where(p => p.Status == PaymentStatus.Completed && p.Amount != null)
                .ToList();

            decimal totalRevenue = 0;
            string currency = "USD";

            if (completedWithAmount.Any())
            {
                // Use primary currency (most common) to avoid mixing currencies
                var primaryCurrency = completedWithAmount
                    .GroupBy(p => p.Amount!.Currency)
                    .OrderByDescending(g => g.Count())
                    .First()
                    .Key;

                totalRevenue = completedWithAmount
                    .Where(p => p.Amount!.Currency == primaryCurrency)
                    .Sum(p => p.Amount!.Amount);

                currency = primaryCurrency;
            }

            // Calculate average processing time
            var completedWithProcessingTime = filteredPayments
                .Where(p => p.Status == PaymentStatus.Completed && p.ProcessedAt.HasValue);
            var averageProcessingTime = completedWithProcessingTime.Any() 
                ? (decimal)completedWithProcessingTime.Average(p => (p.ProcessedAt!.Value - p.CreatedAt).TotalSeconds)
                : 0;

            // Breakdown by status
            var paymentsByStatus = filteredPayments
                .GroupBy(p => p.Status.ToString())
                .ToDictionary(g => g.Key, g => g.Count());

            // Breakdown by payment method
            var paymentsByMethod = filteredPayments
                .Where(p => !string.IsNullOrEmpty(p.PaymentMethod))
                .GroupBy(p => p.PaymentMethod!)
                .ToDictionary(g => g.Key, g => g.Count());

            // Breakdown by failure reasons
            var failureReasons = filteredPayments
                .Where(p => p.Status == PaymentStatus.Failed && !string.IsNullOrEmpty(p.FailureReason))
                .GroupBy(p => p.FailureReason!)
                .ToDictionary(g => g.Key, g => g.Count());

            var statistics = new PaymentStatistics
            {
                TotalPayments = totalPayments,
                CompletedPayments = completedPayments,
                FailedPayments = failedPayments,
                PendingPayments = pendingPayments,
                ProcessingPayments = processingPayments,
                RefundedPayments = refundedPayments,
                SuccessRate = Math.Round(successRate, 2),
                AverageProcessingTimeSeconds = Math.Round(averageProcessingTime, 2),
                TotalRevenue = totalRevenue,
                Currency = currency,
                PaymentsByStatus = paymentsByStatus,
                PaymentsByMethod = paymentsByMethod,
                FailureReasons = failureReasons,
                Period = range,
                ProviderId = providerId
            };

            _logger.LogInformation("Calculated payment statistics: {TotalPayments} total, {SuccessRate}% success rate, {TotalRevenue} {Currency} revenue in period {StartDate} to {EndDate}",
                totalPayments, successRate, totalRevenue, currency, range.StartDate, range.EndDate);

            return statistics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment statistics for period {StartDate} to {EndDate}", 
                range.StartDate, range.EndDate);
            return new PaymentStatistics { Period = range, ProviderId = providerId };
        }
    }

    /// <summary>
    /// Gets payment trends over time for a specific date range
    /// </summary>
    public async Task<PaymentTrends> GetPaymentTrendsAsync(DateRange range, Guid? providerId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_tenantContext.HasTenant)
            {
                _logger.LogWarning("Cannot get payment trends: Tenant context is required");
                return new PaymentTrends { Period = range, ProviderId = providerId };
            }

            var tenantId = _tenantContext.CurrentTenantId!;

            // SECURITY & PERFORMANCE: Use secure method with SQL-level filtering
            var payments = await _paymentRepository.GetPaymentsForMetricsAsync(
                tenantId,
                range.StartDate,
                range.EndDate,
                providerId,
                cancellationToken);

            var filteredPayments = await payments
                .Select(p => new
                {
                    p.Status,
                    p.Amount,
                    p.CreatedAt
                })
                .ToListAsync(cancellationToken);

            if (!filteredPayments.Any())
            {
                _logger.LogInformation("No payments found for trends in period {StartDate} to {EndDate}", 
                    range.StartDate, range.EndDate);
                return new PaymentTrends { Period = range, ProviderId = providerId };
            }

            // Group by day for daily trends
            var dailyTrends = filteredPayments
                .GroupBy(p => p.CreatedAt.Date)
                .OrderBy(g => g.Key)
                .Select(g =>
                {
                    var paymentsWithAmount = g.Where(p => p.Status == PaymentStatus.Completed && p.Amount != null).ToList();

                    // Determine primary currency for this day (most common)
                    var primaryCurrency = paymentsWithAmount.Any()
                        ? paymentsWithAmount
                            .GroupBy(p => p.Amount!.Currency)
                            .OrderByDescending(cg => cg.Count())
                            .First()
                            .Key
                        : "USD";

                    var revenue = paymentsWithAmount
                        .Where(p => p.Amount!.Currency == primaryCurrency)
                        .Sum(p => p.Amount!.Amount);

                    var totalCount = g.Count();
                    var successCount = g.Count(p => p.Status == PaymentStatus.Completed);

                    return new TrendDataPoint
                    {
                        Date = g.Key,
                        PaymentCount = totalCount,
                        SuccessfulPayments = successCount,
                        FailedPayments = g.Count(p => p.Status == PaymentStatus.Failed),
                        Revenue = revenue,
                        Currency = primaryCurrency,
                        SuccessRate = totalCount > 0 ? (decimal)successCount / totalCount * 100 : 0
                    };
                })
                .ToList();

            // Calculate overall trend
            var firstDataPoint = dailyTrends.FirstOrDefault();
            var lastDataPoint = dailyTrends.LastOrDefault();
            
            TrendDirection overallTrend = TrendDirection.Stable;
            decimal percentageChange = 0;

            if (firstDataPoint != null && lastDataPoint != null && firstDataPoint.PaymentCount > 0)
            {
                percentageChange = (decimal)(lastDataPoint.PaymentCount - firstDataPoint.PaymentCount) / firstDataPoint.PaymentCount * 100;
                
                if (percentageChange > 5)
                    overallTrend = TrendDirection.Increasing;
                else if (percentageChange < -5)
                    overallTrend = TrendDirection.Decreasing;
            }

            var trends = new PaymentTrends
            {
                DataPoints = dailyTrends,
                OverallTrend = overallTrend,
                PercentageChange = Math.Round(percentageChange, 2),
                Period = range,
                ProviderId = providerId
            };

            _logger.LogInformation("Calculated payment trends: {DataPointsCount} data points, {OverallTrend} trend ({PercentageChange}% change) in period {StartDate} to {EndDate}",
                dailyTrends.Count, overallTrend, percentageChange, range.StartDate, range.EndDate);

            return trends;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment trends for period {StartDate} to {EndDate}", 
                range.StartDate, range.EndDate);
            return new PaymentTrends { Period = range, ProviderId = providerId };
        }
    }
}
