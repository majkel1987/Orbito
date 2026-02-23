using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Enums;

namespace Orbito.Application.Features.Analytics.Queries.GetRevenueHistory;

/// <summary>
/// Handler for get revenue history query
/// </summary>
public class GetRevenueHistoryQueryHandler : IRequestHandler<GetRevenueHistoryQuery, GetRevenueHistoryResponse>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetRevenueHistoryQueryHandler> _logger;

    public GetRevenueHistoryQueryHandler(
        IPaymentRepository paymentRepository,
        ITenantContext tenantContext,
        ILogger<GetRevenueHistoryQueryHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<GetRevenueHistoryResponse> Handle(GetRevenueHistoryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting revenue history for period {StartDate} to {EndDate}",
                request.StartDate, request.EndDate);

            var tenantId = _tenantContext.CurrentTenantId;
            if (tenantId == null)
            {
                _logger.LogWarning("No tenant context available for revenue history");
                return new GetRevenueHistoryResponse
                {
                    StartDate = request.StartDate,
                    EndDate = request.EndDate
                };
            }

            // Get payments for the period using metrics query
            var paymentsQuery = await _paymentRepository.GetPaymentsForMetricsAsync(
                tenantId,
                request.StartDate,
                request.EndDate,
                providerId: null,
                cancellationToken);

            // Filter completed payments and group by date
            var completedPayments = await paymentsQuery
                .Where(p => p.Status == PaymentStatus.Completed)
                .ToListAsync(cancellationToken);

            var revenueByDate = completedPayments
                .GroupBy(p => p.ProcessedAt?.Date ?? p.CreatedAt.Date)
                .Select(g => new RevenueHistoryItemDto
                {
                    Date = g.Key,
                    Amount = g.Sum(p => p.Amount.Amount),
                    Currency = g.First().Amount.Currency
                })
                .OrderBy(r => r.Date)
                .ToList();

            // Fill in missing dates with zero values
            var allDates = new List<RevenueHistoryItemDto>();
            var currentDate = request.StartDate.Date;
            var endDate = request.EndDate.Date;

            while (currentDate <= endDate)
            {
                var existingData = revenueByDate.FirstOrDefault(r => r.Date.Date == currentDate);
                allDates.Add(existingData ?? new RevenueHistoryItemDto
                {
                    Date = currentDate,
                    Amount = 0,
                    Currency = "PLN"
                });
                currentDate = currentDate.AddDays(1);
            }

            var totalRevenue = allDates.Sum(r => r.Amount);

            _logger.LogInformation("Revenue history calculated: {Count} data points, Total={Total}",
                allDates.Count, totalRevenue);

            return new GetRevenueHistoryResponse
            {
                Items = allDates,
                TotalRevenue = totalRevenue,
                StartDate = request.StartDate,
                EndDate = request.EndDate
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting revenue history for period {StartDate} to {EndDate}",
                request.StartDate, request.EndDate);

            return new GetRevenueHistoryResponse
            {
                StartDate = request.StartDate,
                EndDate = request.EndDate
            };
        }
    }
}
