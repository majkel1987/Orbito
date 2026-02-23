using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orbito.Application.Common.Authorization;
using Orbito.Application.Common.Models;
using Orbito.Application.Features.Analytics.Queries.GetClientGrowth;
using Orbito.Application.Features.Analytics.Queries.GetDashboardStats;
using Orbito.Application.Features.Analytics.Queries.GetRevenueHistory;

namespace Orbito.API.Controllers;

/// <summary>
/// Controller for analytics and dashboard statistics endpoints
/// </summary>
[Route("api/[controller]")]
[Authorize(Policy = PolicyNames.ProviderTeamAccess)]
public class AnalyticsController : BaseController
{
    public AnalyticsController(IMediator mediator, ILogger<AnalyticsController> logger)
        : base(mediator, logger)
    {
    }

    /// <summary>
    /// Gets dashboard statistics for a specific period
    /// </summary>
    /// <param name="startDate">Start date of the statistics period (YYYY-MM-DD)</param>
    /// <param name="endDate">End date of the statistics period (YYYY-MM-DD)</param>
    /// <returns>Dashboard statistics including MRR, ARR, clients, subscriptions</returns>
    /// <response code="200">Returns dashboard statistics</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(DashboardStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<DashboardStatsDto>> GetDashboardStats(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        // Default to last 30 days if not specified
        var end = endDate ?? DateTime.UtcNow;
        var start = startDate ?? end.AddDays(-30);

        // Validate date range
        var dateValidation = ValidateDateRange(start, end);
        if (dateValidation != null)
            return dateValidation;

        var query = new GetDashboardStatsQuery(start, end);
        return await ExecuteQueryAsync<GetDashboardStatsQuery, DashboardStatsDto>(
            query,
            $"GetDashboardStats for period {start:yyyy-MM-dd} to {end:yyyy-MM-dd}");
    }

    /// <summary>
    /// Gets revenue history for charts
    /// </summary>
    /// <param name="startDate">Start date of the revenue period (YYYY-MM-DD)</param>
    /// <param name="endDate">End date of the revenue period (YYYY-MM-DD)</param>
    /// <returns>Revenue history data points for charts</returns>
    /// <response code="200">Returns revenue history</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    [HttpGet("revenue")]
    [ProducesResponseType(typeof(GetRevenueHistoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<GetRevenueHistoryResponse>> GetRevenueHistory(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        // Default to last 30 days if not specified
        var end = endDate ?? DateTime.UtcNow;
        var start = startDate ?? end.AddDays(-30);

        // Validate date range
        var dateValidation = ValidateDateRange(start, end);
        if (dateValidation != null)
            return dateValidation;

        var query = new GetRevenueHistoryQuery(start, end);
        return await ExecuteQueryAsync<GetRevenueHistoryQuery, GetRevenueHistoryResponse>(
            query,
            $"GetRevenueHistory for period {start:yyyy-MM-dd} to {end:yyyy-MM-dd}");
    }

    /// <summary>
    /// Gets client growth history for charts
    /// </summary>
    /// <param name="startDate">Start date of the period (YYYY-MM-DD)</param>
    /// <param name="endDate">End date of the period (YYYY-MM-DD)</param>
    /// <returns>Client growth data points for charts</returns>
    /// <response code="200">Returns client growth history</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    [HttpGet("clients")]
    [ProducesResponseType(typeof(GetClientGrowthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<GetClientGrowthResponse>> GetClientGrowth(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        // Default to last 30 days if not specified
        var end = endDate ?? DateTime.UtcNow;
        var start = startDate ?? end.AddDays(-30);

        // Validate date range
        var dateValidation = ValidateDateRange(start, end);
        if (dateValidation != null)
            return dateValidation;

        var query = new GetClientGrowthQuery(start, end);
        return await ExecuteQueryAsync<GetClientGrowthQuery, GetClientGrowthResponse>(
            query,
            $"GetClientGrowth for period {start:yyyy-MM-dd} to {end:yyyy-MM-dd}");
    }
}
