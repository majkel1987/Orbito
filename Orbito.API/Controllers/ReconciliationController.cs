using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orbito.Application.Common.Authorization;
using Orbito.API.Extensions;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations;

namespace Orbito.API.Controllers;

/// <summary>
/// Controller for payment reconciliation operations
/// </summary>
[Authorize(Policy = PolicyNames.ProviderTeamAccess)]
[Authorize(Policy = PolicyNames.ActiveProviderSubscription)]
[Route("api/reconciliation")]
public class ReconciliationController : BaseController
{
    private readonly IPaymentReconciliationService _reconciliationService;
    private readonly IReconciliationRepository _reconciliationRepository;
    private readonly ITenantContext _tenantContext;

    public ReconciliationController(
        IMediator mediator,
        ILogger<ReconciliationController> logger,
        IPaymentReconciliationService reconciliationService,
        IReconciliationRepository reconciliationRepository,
        ITenantContext tenantContext)
        : base(mediator, logger)
    {
        _reconciliationService = reconciliationService ?? throw new ArgumentNullException(nameof(reconciliationService));
        _reconciliationRepository = reconciliationRepository ?? throw new ArgumentNullException(nameof(reconciliationRepository));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
    }

    /// <summary>
    /// Runs reconciliation for a specific date range
    /// </summary>
    /// <param name="request">Reconciliation request parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Reconciliation report</returns>
    [HttpPost("run")]
    [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("reconciliation")]
    [ProducesResponseType(typeof(ReconciliationReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RunReconciliation(
        [FromBody] RunReconciliationRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // SECURITY: Validate date range to prevent abuse
            if (request.FromDate >= request.ToDate)
            {
                return BadRequest("FromDate must be before ToDate");
            }

            if (request.FromDate > DateTime.UtcNow)
            {
                return BadRequest("FromDate cannot be in the future");
            }

            if (request.ToDate > DateTime.UtcNow)
            {
                return BadRequest("ToDate cannot be in the future");
            }

            if (request.ToDate - request.FromDate > TimeSpan.FromDays(30))
            {
                return BadRequest("Reconciliation period cannot exceed 30 days");
            }

            var tenantId = TenantId.Create(request.TenantId);

            // SECURITY: Verify JWT tenant_id claim matches request body
            var jwtTenantIdClaim = User.FindFirst("tenant_id")?.Value;
            if (!User.IsInRole("PlatformAdmin") && !string.IsNullOrEmpty(jwtTenantIdClaim))
            {
                if (!Guid.TryParse(jwtTenantIdClaim, out var jwtTenantId) || jwtTenantId != request.TenantId)
                {
                    Logger.LogWarning(
                        "SECURITY: JWT tenant_id {JwtTenantId} does not match request tenant {RequestTenantId}",
                        jwtTenantIdClaim, request.TenantId);
                    return Forbid();
                }
            }

            // SECURITY: Verify tenant ownership - Providers can only reconcile their own tenant
            if (!User.IsInRole("PlatformAdmin"))
            {
                if (!_tenantContext.HasTenant || _tenantContext.CurrentTenantId != tenantId)
                {
                    Logger.LogWarning(
                        "SECURITY: User attempted to reconcile tenant {RequestedTenantId} but current tenant is {CurrentTenantId}",
                        request.TenantId, _tenantContext.CurrentTenantId?.Value);
                    return Forbid();
                }
            }
            var report = await _reconciliationService.ReconcileWithStripeAsync(
                request.FromDate,
                request.ToDate,
                tenantId,
                cancellationToken);

            Logger.LogInformation(
                "Reconciliation completed for tenant {TenantId}. Report ID: {ReportId}, Discrepancies: {DiscrepancyCount}",
                request.TenantId, report.Id, report.DiscrepanciesCount);

            return Ok(report.ToDto());
        }
        catch (ArgumentException ex)
        {
            Logger.LogWarning(ex, "Invalid reconciliation request: {ErrorMessage}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            Logger.LogError(ex, "Unauthorized reconciliation attempt: {ErrorMessage}", ex.Message);
            return Unauthorized("Insufficient permissions for reconciliation");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to run reconciliation: {ErrorMessage}", ex.Message);
            return StatusCode(500, "Internal server error during reconciliation");
        }
    }

    /// <summary>
    /// Gets recent reconciliation reports for a tenant
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="count">Number of reports to retrieve (max 50)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of reconciliation reports</returns>
    [HttpGet("reports")]
    [ProducesResponseType(typeof(IEnumerable<ReconciliationReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetReports(
        [FromQuery, Required] Guid tenantId,
        [FromQuery] int count = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (count <= 0 || count > 50)
            {
                return BadRequest("Count must be between 1 and 50");
            }

            var tenantIdValue = TenantId.Create(tenantId);

            // SECURITY: Verify tenant ownership
            if (!User.IsInRole("PlatformAdmin"))
            {
                if (!_tenantContext.HasTenant || _tenantContext.CurrentTenantId != tenantIdValue)
                {
                    Logger.LogWarning(
                        "SECURITY: User attempted to get reports for tenant {RequestedTenantId} but current tenant is {CurrentTenantId}",
                        tenantId, _tenantContext.CurrentTenantId?.Value);
                    return Forbid();
                }
            }
            var reports = await _reconciliationRepository.GetRecentReportsAsync(
                tenantIdValue, count, cancellationToken);

            return Ok(reports.ToDto());
        }
        catch (ArgumentException ex)
        {
            Logger.LogWarning(ex, "Invalid get reports request: {ErrorMessage}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to get reconciliation reports: {ErrorMessage}", ex.Message);
            return StatusCode(500, "Internal server error while retrieving reports");
        }
    }

    /// <summary>
    /// Gets discrepancies for a specific reconciliation report
    /// </summary>
    /// <param name="reportId">Report ID</param>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of payment discrepancies</returns>
    [HttpGet("reports/{reportId}/discrepancies")]
    [ProducesResponseType(typeof(IEnumerable<PaymentDiscrepancyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetDiscrepancies(
        [FromRoute] Guid reportId,
        [FromQuery, Required] Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tenantIdValue = TenantId.Create(tenantId);

            // SECURITY: Verify tenant ownership
            if (!User.IsInRole("PlatformAdmin"))
            {
                if (!_tenantContext.HasTenant || _tenantContext.CurrentTenantId != tenantIdValue)
                {
                    Logger.LogWarning(
                        "SECURITY: User attempted to get discrepancies for tenant {RequestedTenantId} but current tenant is {CurrentTenantId}",
                        tenantId, _tenantContext.CurrentTenantId?.Value);
                    return Forbid();
                }
            }

            var discrepancies = await _reconciliationRepository.GetDiscrepanciesByReportIdAsync(
                reportId, tenantIdValue, cancellationToken);

            return Ok(discrepancies.ToDto());
        }
        catch (ArgumentException ex)
        {
            Logger.LogWarning(ex, "Invalid get discrepancies request: {ErrorMessage}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to get discrepancies for report {ReportId}: {ErrorMessage}",
                reportId, ex.Message);
            return StatusCode(500, "Internal server error while retrieving discrepancies");
        }
    }

    /// <summary>
    /// Manually resolves a payment discrepancy
    /// </summary>
    /// <param name="discrepancyId">Discrepancy ID</param>
    /// <param name="request">Resolution request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    [HttpPost("discrepancies/{discrepancyId}/resolve")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ResolveDiscrepancy(
        [FromRoute] Guid discrepancyId,
        [FromBody] ResolveDiscrepancyRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var tenantIdValue = TenantId.Create(request.TenantId);

            // SECURITY: Verify tenant ownership
            if (!User.IsInRole("PlatformAdmin"))
            {
                if (!_tenantContext.HasTenant || _tenantContext.CurrentTenantId != tenantIdValue)
                {
                    Logger.LogWarning(
                        "SECURITY: User attempted to resolve discrepancy {DiscrepancyId} for tenant {RequestedTenantId} but current tenant is {CurrentTenantId}",
                        discrepancyId, request.TenantId, _tenantContext.CurrentTenantId?.Value);
                    return Forbid();
                }
            }

            // Get the discrepancy
            var discrepancy = await _reconciliationRepository.GetDiscrepancyByIdAsync(
                discrepancyId,
                tenantIdValue,
                cancellationToken);

            if (discrepancy == null)
            {
                Logger.LogWarning("Discrepancy {DiscrepancyId} not found for tenant {TenantId}",
                    discrepancyId, request.TenantId);
                return NotFound("Discrepancy not found");
            }

            // IDEMPOTENCY: Check if already resolved
            if (discrepancy.Resolution != DiscrepancyResolution.Pending &&
                discrepancy.Resolution != DiscrepancyResolution.RequiresManualReview)
            {
                Logger.LogInformation(
                    "Discrepancy {DiscrepancyId} already resolved with status {Resolution}",
                    discrepancyId, discrepancy.Resolution);
                return Conflict(new
                {
                    message = "Discrepancy already resolved",
                    discrepancyId = discrepancy.Id,
                    currentResolution = discrepancy.Resolution.ToString(),
                    resolvedAt = discrepancy.ResolvedAt,
                    resolvedBy = discrepancy.ResolvedBy
                });
            }

            // Validate resolution action
            if (!Enum.TryParse<DiscrepancyResolution>(request.Resolution, ignoreCase: true, out var resolutionEnum))
            {
                return BadRequest($"Invalid resolution: {request.Resolution}. Valid values: ManuallyResolved, Ignored");
            }

            if (resolutionEnum != DiscrepancyResolution.ManuallyResolved &&
                resolutionEnum != DiscrepancyResolution.Ignored)
            {
                return BadRequest("Manual resolution can only set status to 'ManuallyResolved' or 'Ignored'");
            }

            // Apply resolution
            if (resolutionEnum == DiscrepancyResolution.ManuallyResolved)
            {
                discrepancy.MarkAsManuallyResolved(request.Notes, request.ResolvedBy);
            }
            else if (resolutionEnum == DiscrepancyResolution.Ignored)
            {
                discrepancy.MarkAsIgnored(request.Notes, request.ResolvedBy);
            }

            await _reconciliationRepository.UpdateDiscrepancyAsync(discrepancy, cancellationToken);

            Logger.LogInformation(
                "Discrepancy {DiscrepancyId} resolved as {Resolution} by {ResolvedBy}",
                discrepancyId, resolutionEnum, request.ResolvedBy);

            return Ok(new
            {
                message = "Discrepancy resolved successfully",
                discrepancyId = discrepancy.Id,
                resolution = discrepancy.Resolution.ToString(),
                resolvedAt = discrepancy.ResolvedAt,
                resolvedBy = discrepancy.ResolvedBy
            });
        }
        catch (ArgumentException ex)
        {
            Logger.LogWarning(ex, "Invalid resolve discrepancy request: {ErrorMessage}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            Logger.LogWarning(ex, "Cannot resolve discrepancy {DiscrepancyId}: {ErrorMessage}",
                discrepancyId, ex.Message);
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            Logger.LogError(ex, "Unauthorized attempt to resolve discrepancy {DiscrepancyId}: {ErrorMessage}",
                discrepancyId, ex.Message);
            return Unauthorized("Insufficient permissions");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to resolve discrepancy {DiscrepancyId}: {ErrorMessage}",
                discrepancyId, ex.Message);
            return StatusCode(500, "Internal server error while resolving discrepancy");
        }
    }
}

/// <summary>
/// Request model for running reconciliation
/// </summary>
public class RunReconciliationRequest
{
    /// <summary>
    /// Tenant ID to reconcile payments for
    /// </summary>
    [Required]
    public Guid TenantId { get; set; }

    /// <summary>
    /// Start date of reconciliation period
    /// </summary>
    [Required]
    public DateTime FromDate { get; set; }

    /// <summary>
    /// End date of reconciliation period
    /// </summary>
    [Required]
    public DateTime ToDate { get; set; }
}

/// <summary>
/// Request model for resolving discrepancies
/// </summary>
public class ResolveDiscrepancyRequest
{
    /// <summary>
    /// Tenant ID
    /// </summary>
    [Required]
    public Guid TenantId { get; set; }

    /// <summary>
    /// Resolution type: ManuallyResolved or Ignored
    /// </summary>
    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string Resolution { get; set; } = string.Empty;

    /// <summary>
    /// Resolution notes
    /// </summary>
    [Required]
    [StringLength(2000, MinimumLength = 1)]
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// User who resolved the discrepancy
    /// </summary>
    [Required]
    [StringLength(255, MinimumLength = 1)]
    public string ResolvedBy { get; set; } = string.Empty;
}
