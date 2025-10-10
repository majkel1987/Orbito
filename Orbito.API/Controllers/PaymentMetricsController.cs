using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orbito.Application.Common.Constants;
using Orbito.Application.Common.Models;
using Orbito.Application.Features.Payments.Queries.GetFailureReasons;
using Orbito.Application.Features.Payments.Queries.GetPaymentStatistics;
using Orbito.Application.Features.Payments.Queries.GetPaymentTrends;
using Orbito.Application.Features.Payments.Queries.GetRevenueReport;
using System.ComponentModel.DataAnnotations;

namespace Orbito.API.Controllers;

/// <summary>
/// Controller for payment metrics and statistics endpoints
/// </summary>
[Route("api/[controller]")]
[Authorize(Roles = "Provider,PlatformAdmin")]
public class PaymentMetricsController : BaseController
{
    public PaymentMetricsController(IMediator mediator, ILogger<PaymentMetricsController> logger) 
        : base(mediator, logger)
    {
    }

    /// <summary>
    /// Gets comprehensive payment statistics for a specific period
    /// </summary>
    /// <param name="startDate">Start date of the statistics period (YYYY-MM-DD)</param>
    /// <param name="endDate">End date of the statistics period (YYYY-MM-DD)</param>
    /// <param name="providerId">Optional provider ID to filter statistics by</param>
    /// <returns>Payment statistics for the specified period</returns>
    /// <response code="200">Returns payment statistics</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(PaymentStatistics), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaymentStatistics>> GetPaymentStatistics(
        [FromQuery, Required] DateTime startDate,
        [FromQuery, Required] DateTime endDate,
        [FromQuery] Guid? providerId = null)
    {
        // Validate date range
        var dateValidation = ValidateDateRange(startDate, endDate);
        if (dateValidation != null)
            return dateValidation;

        var query = new GetPaymentStatisticsQuery(startDate, endDate, providerId);
        return await ExecuteQueryAsync<GetPaymentStatisticsQuery, PaymentStatistics>(
            query, 
            $"GetPaymentStatistics for period {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}, ProviderId: {providerId}");
    }

    /// <summary>
    /// Gets revenue report for a specific provider and period
    /// </summary>
    /// <param name="startDate">Start date of the revenue period (YYYY-MM-DD)</param>
    /// <param name="endDate">End date of the revenue period (YYYY-MM-DD)</param>
    /// <param name="providerId">Provider ID to get revenue for</param>
    /// <returns>Revenue metrics for the specified provider and period</returns>
    /// <response code="200">Returns revenue metrics</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    [HttpGet("revenue")]
    [ProducesResponseType(typeof(RevenueMetrics), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RevenueMetrics>> GetRevenueReport(
        [FromQuery, Required] DateTime startDate,
        [FromQuery, Required] DateTime endDate,
        [FromQuery, Required] Guid providerId)
    {
        // Validate parameters
        var guidValidation = ValidateGuid(providerId, "Provider ID");
        if (guidValidation != null)
            return guidValidation;

        var dateValidation = ValidateDateRange(startDate, endDate);
        if (dateValidation != null)
            return dateValidation;

        var query = new GetRevenueReportQuery(startDate, endDate, providerId);
        return await ExecuteQueryAsync<GetRevenueReportQuery, RevenueMetrics>(
            query, 
            $"GetRevenueReport for provider {providerId} in period {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");
    }

    /// <summary>
    /// Gets payment trends over time for a specific period
    /// </summary>
    /// <param name="startDate">Start date of the trends period (YYYY-MM-DD)</param>
    /// <param name="endDate">End date of the trends period (YYYY-MM-DD)</param>
    /// <param name="providerId">Optional provider ID to filter trends by</param>
    /// <returns>Payment trends for the specified period</returns>
    /// <response code="200">Returns payment trends</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    [HttpGet("trends")]
    [ProducesResponseType(typeof(PaymentTrends), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PaymentTrends>> GetPaymentTrends(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] Guid? providerId = null)
    {
        try
        {
            Logger.LogInformation("Getting payment trends for period {StartDate} to {EndDate}, ProviderId: {ProviderId}",
                startDate, endDate, providerId);

            var query = new GetPaymentTrendsQuery(startDate, endDate, providerId);
            var result = await Mediator.Send(query);

            return Ok(result);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting payment trends for period {StartDate} to {EndDate}, ProviderId: {ProviderId}",
                startDate, endDate, providerId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving payment trends");
        }
    }

    /// <summary>
    /// Gets breakdown of failure reasons for a specific period
    /// </summary>
    /// <param name="startDate">Start date of the period (YYYY-MM-DD)</param>
    /// <param name="endDate">End date of the period (YYYY-MM-DD)</param>
    /// <param name="providerId">Optional provider ID to filter failure reasons by</param>
    /// <returns>Dictionary of failure reasons and their counts</returns>
    /// <response code="200">Returns failure reasons breakdown</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    [HttpGet("failure-reasons")]
    [ProducesResponseType(typeof(Dictionary<string, int>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Dictionary<string, int>>> GetFailureReasons(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] Guid? providerId = null)
    {
        try
        {
            Logger.LogInformation("Getting failure reasons for period {StartDate} to {EndDate}, ProviderId: {ProviderId}",
                startDate, endDate, providerId);

            var query = new GetFailureReasonsQuery(startDate, endDate, providerId);
            var result = await Mediator.Send(query);

            return Ok(result);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting failure reasons for period {StartDate} to {EndDate}, ProviderId: {ProviderId}",
                startDate, endDate, providerId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving failure reasons");
        }
    }

    /// <summary>
    /// Gets payment success rate for a specific period
    /// </summary>
    /// <param name="startDate">Start date of the period (YYYY-MM-DD)</param>
    /// <param name="endDate">End date of the period (YYYY-MM-DD)</param>
    /// <param name="providerId">Optional provider ID to filter by</param>
    /// <returns>Payment success rate percentage</returns>
    /// <response code="200">Returns success rate</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    [HttpGet("success-rate")]
    [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<decimal>> GetPaymentSuccessRate(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] Guid? providerId = null)
    {
        try
        {
            Logger.LogInformation("Getting payment success rate for period {StartDate} to {EndDate}, ProviderId: {ProviderId}",
                startDate, endDate, providerId);

            var query = new GetPaymentStatisticsQuery(startDate, endDate, providerId);
            var result = await Mediator.Send(query);

            return Ok(result.SuccessRate);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting payment success rate for period {StartDate} to {EndDate}, ProviderId: {ProviderId}",
                startDate, endDate, providerId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving success rate");
        }
    }

    /// <summary>
    /// Gets average payment processing time for a specific period
    /// </summary>
    /// <param name="startDate">Start date of the period (YYYY-MM-DD)</param>
    /// <param name="endDate">End date of the period (YYYY-MM-DD)</param>
    /// <returns>Average processing time in seconds</returns>
    /// <response code="200">Returns average processing time</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    [HttpGet("average-processing-time")]
    [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<decimal>> GetAverageProcessingTime(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        try
        {
            Logger.LogInformation("Getting average processing time for period {StartDate} to {EndDate}",
                startDate, endDate);

            var query = new GetPaymentStatisticsQuery(startDate, endDate);
            var result = await Mediator.Send(query);

            return Ok(result.AverageProcessingTimeSeconds);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting average processing time for period {StartDate} to {EndDate}",
                startDate, endDate);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving average processing time");
        }
    }
}
