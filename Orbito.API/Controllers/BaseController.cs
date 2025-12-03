using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orbito.Application.Common.Constants;
using Orbito.Application.Common.Models;
using System.ComponentModel.DataAnnotations;

namespace Orbito.API.Controllers;

/// <summary>
/// Base controller with common functionality for all API controllers
/// </summary>
[ApiController]
[Authorize]
public abstract class BaseController : ControllerBase
{
    protected readonly IMediator Mediator;
    protected readonly ILogger Logger;

    protected BaseController(IMediator mediator, ILogger logger)
    {
        Mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Executes a query with proper error handling and logging
    /// </summary>
    /// <typeparam name="TRequest">Request type</typeparam>
    /// <typeparam name="TResponse">Response type</typeparam>
    /// <param name="request">The request to execute</param>
    /// <param name="operationName">Name of the operation for logging</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>ActionResult with the response or error</returns>
    protected async Task<ActionResult<TResponse>> ExecuteQueryAsync<TRequest, TResponse>(
        TRequest request, 
        string operationName, 
        CancellationToken cancellationToken = default)
        where TRequest : IRequest<TResponse>
    {
        try
        {
            Logger.LogInformation("Executing {OperationName}", operationName);
            
            var result = await Mediator.Send(request, cancellationToken);
            
            Logger.LogInformation("Successfully executed {OperationName}", operationName);
            return Ok(result);
        }
        catch (ValidationException ex)
        {
            Logger.LogWarning(ex, "Validation error in {OperationName}", operationName);
            return BadRequest(ErrorResponse.ValidationError(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            Logger.LogWarning(ex, "Unauthorized access in {OperationName}", operationName);
            return Unauthorized(ErrorResponse.Unauthorized(ex.Message));
        }
        catch (ArgumentException ex)
        {
            Logger.LogWarning(ex, "Invalid argument in {OperationName}", operationName);
            return BadRequest(ErrorResponse.ValidationError(ex.Message));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error executing {OperationName}", operationName);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                ErrorResponse.InternalError(HttpContext.TraceIdentifier));
        }
    }

    /// <summary>
    /// Executes a command with proper error handling and logging
    /// </summary>
    /// <typeparam name="TRequest">Request type</typeparam>
    /// <typeparam name="TResponse">Response type</typeparam>
    /// <param name="request">The request to execute</param>
    /// <param name="operationName">Name of the operation for logging</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>ActionResult with the response or error</returns>
    protected async Task<ActionResult<TResponse>> ExecuteCommandAsync<TRequest, TResponse>(
        TRequest request, 
        string operationName, 
        CancellationToken cancellationToken = default)
        where TRequest : IRequest<TResponse>
    {
        try
        {
            Logger.LogInformation("Executing {OperationName}", operationName);
            
            var result = await Mediator.Send(request, cancellationToken);
            
            Logger.LogInformation("Successfully executed {OperationName}", operationName);
            return Ok(result);
        }
        catch (ValidationException ex)
        {
            Logger.LogWarning(ex, "Validation error in {OperationName}", operationName);
            return BadRequest(ErrorResponse.ValidationError(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            Logger.LogWarning(ex, "Unauthorized access in {OperationName}", operationName);
            return Unauthorized(ErrorResponse.Unauthorized(ex.Message));
        }
        catch (ArgumentException ex)
        {
            Logger.LogWarning(ex, "Invalid argument in {OperationName}", operationName);
            return BadRequest(ErrorResponse.ValidationError(ex.Message));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error executing {OperationName}", operationName);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                ErrorResponse.InternalError(HttpContext.TraceIdentifier));
        }
    }

    /// <summary>
    /// Validates date range parameters
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <returns>Error response if validation fails, null if valid</returns>
    protected ActionResult? ValidateDateRange(DateTime startDate, DateTime endDate)
    {
        if (startDate > endDate)
        {
            return BadRequest(ErrorResponse.ValidationError("Start date cannot be after end date"));
        }

        var dateRange = (endDate - startDate).TotalDays;
        if (dateRange > 365)
        {
            return BadRequest(ErrorResponse.ValidationError("Date range cannot exceed 365 days"));
        }

        if (startDate > DateTime.UtcNow.AddDays(1))
        {
            return BadRequest(ErrorResponse.ValidationError("Start date cannot be in the future"));
        }

        return null;
    }

    /// <summary>
    /// Validates GUID parameter
    /// </summary>
    /// <param name="guid">GUID to validate</param>
    /// <param name="parameterName">Name of the parameter for error message</param>
    /// <returns>Error response if validation fails, null if valid</returns>
    protected ActionResult? ValidateGuid(Guid guid, string parameterName)
    {
        if (guid == Guid.Empty)
        {
            return BadRequest(ErrorResponse.ValidationError($"{parameterName} is required"));
        }

        return null;
    }

    /// <summary>
    /// Handles Result<T> and converts it to appropriate ActionResult
    /// Returns full Result wrapper for frontend compatibility
    /// </summary>
    /// <typeparam name="T">Type of the value</typeparam>
    /// <param name="result">Result to handle</param>
    /// <returns>ActionResult with Result wrapper containing isSuccess, value, and error</returns>
    protected IActionResult HandleResult<T>(Orbito.Domain.Common.Result<T> result)
    {
        if (result.IsSuccess)
        {
            // Return full Result wrapper for frontend
            return Ok(new
            {
                isSuccess = true,
                value = result.Value,
                error = default(string)
            });
        }

        // Return error with Result wrapper
        return HandleErrorCode(result.Error);
    }

    /// <summary>
    /// Handles Result (without value) and converts it to appropriate ActionResult
    /// Maps error codes to HTTP status codes
    /// </summary>
    /// <param name="result">Result to handle</param>
    /// <returns>ActionResult with appropriate status code and response</returns>
    protected IActionResult HandleResult(Orbito.Domain.Common.Result result)
    {
        if (result.IsSuccess)
        {
            return Ok();
        }

        return HandleErrorCode(result.Error);
    }

    /// <summary>
    /// Maps error codes to HTTP status codes and creates Result wrapper with error
    /// </summary>
    /// <param name="error">Error to handle</param>
    /// <returns>ActionResult with appropriate status code and Result wrapper</returns>
    private IActionResult HandleErrorCode(Orbito.Domain.Common.Error error)
    {
        // Create Result wrapper for frontend
        var resultWrapper = new
        {
            isSuccess = false,
            value = default(object),
            error = error.Message
        };

        // Map error codes to HTTP status codes
        return error.Code switch
        {
            // NotFound errors (404)
            var code when code.Contains("NotFound") => NotFound(resultWrapper),

            // AlreadyExists/Conflict errors (409)
            var code when code.Contains("AlreadyExists") => Conflict(resultWrapper),
            var code when code.Contains("Conflict") => Conflict(resultWrapper),
            var code when code.Contains("Duplicate") => Conflict(resultWrapper),

            // Unauthorized errors (401)
            var code when code.Contains("Unauthorized") => Unauthorized(resultWrapper),
            var code when code.Contains("InvalidCredentials") => Unauthorized(resultWrapper),

            // Forbidden errors (403)
            var code when code.Contains("CrossTenant") => StatusCode(StatusCodes.Status403Forbidden, resultWrapper),
            var code when code.Contains("NoTenantContext") => StatusCode(StatusCodes.Status403Forbidden, resultWrapper),

            // Validation/Bad Request errors (400)
            var code when code.Contains("Invalid") => BadRequest(resultWrapper),
            var code when code.Contains("Validation") => BadRequest(resultWrapper),
            var code when code.Contains("Cannot") => BadRequest(resultWrapper),
            var code when code.Contains("Inactive") => BadRequest(resultWrapper),

            // Too Many Requests (429)
            var code when code.Contains("RateLimit") => StatusCode(StatusCodes.Status429TooManyRequests, resultWrapper),

            // Default to BadRequest (400) for unknown errors
            _ => BadRequest(resultWrapper)
        };
    }
}
