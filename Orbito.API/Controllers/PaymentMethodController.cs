using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orbito.Application.Common.Authorization;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Features.PaymentMethods.Commands;
using Orbito.Application.Features.PaymentMethods.Queries;
using Orbito.Application.Features.Payments.Queries.GetPaymentMethodsByClient;

namespace Orbito.API.Controllers;

/// <summary>
/// Controller for payment method operations
/// </summary>
[ApiController]
[Route("api/payment-methods")]
[Authorize]
public class PaymentMethodController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IUserContextService _userContextService;

    public PaymentMethodController(IMediator mediator, IUserContextService userContextService)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _userContextService = userContextService ?? throw new ArgumentNullException(nameof(userContextService));
    }

    /// <summary>
    /// Gets payment methods for the current user's client
    /// </summary>
    /// <param name="pageNumber">Page number (default 1)</param>
    /// <param name="pageSize">Page size (default 10, max 100)</param>
    /// <param name="activeOnly">Include only active payment methods (default true)</param>
    /// <returns>List of payment methods</returns>
    [HttpGet("my-payment-methods")]
    [Authorize(Policy = PolicyNames.ClientAccess)]
    public async Task<IActionResult> GetMyPaymentMethods(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool activeOnly = true)
    {
        // SECURITY: Get client ID from authenticated user context
        var clientId = await _userContextService.GetCurrentClientIdAsync();
        if (clientId == null)
        {
            return Forbid("Access denied - client not found");
        }

        // Validate pagination parameters
        if (pageSize > 100) pageSize = 100;
        if (pageNumber < 1) pageNumber = 1;

        var query = new GetPaymentMethodsByClientQuery
        {
            ClientId = clientId.Value,
            PageNumber = pageNumber,
            PageSize = pageSize,
            ActiveOnly = activeOnly
        };

        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.ErrorMessage });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Gets payment methods for a specific client (Provider/Admin only)
    /// </summary>
    /// <param name="clientId">Client ID</param>
    /// <param name="pageNumber">Page number (default 1)</param>
    /// <param name="pageSize">Page size (default 10, max 100)</param>
    /// <param name="activeOnly">Include only active payment methods (default true)</param>
    /// <returns>List of payment methods</returns>
    [HttpGet("client/{clientId}")]
    [Authorize(Policy = PolicyNames.ProviderTeamAccess)]
    public async Task<IActionResult> GetPaymentMethodsByClient(
        Guid clientId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool activeOnly = true)
    {
        // Validate pagination parameters
        if (pageSize > 100) pageSize = 100;
        if (pageNumber < 1) pageNumber = 1;

        var query = new GetPaymentMethodsByClientQuery
        {
            ClientId = clientId,
            PageNumber = pageNumber,
            PageSize = pageSize,
            ActiveOnly = activeOnly
        };

        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.ErrorMessage });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Gets default payment method for the current user's client
    /// </summary>
    /// <returns>Default payment method</returns>
    [HttpGet("my-default")]
    [Authorize(Policy = PolicyNames.ClientAccess)]
    public async Task<IActionResult> GetMyDefaultPaymentMethod()
    {
        // SECURITY: Get client ID from authenticated user context
        var clientId = await _userContextService.GetCurrentClientIdAsync();
        if (clientId == null)
        {
            return Forbid("Access denied - client not found");
        }

        var query = new GetDefaultPaymentMethodQuery
        {
            ClientId = clientId.Value
        };

        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.ErrorMessage });
        }

        if (result.Value == null || !result.Value.HasDefault)
        {
            return NotFound(new { message = "No default payment method found" });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Gets default payment method for a specific client (Provider/Admin only)
    /// </summary>
    /// <param name="clientId">Client ID</param>
    /// <returns>Default payment method</returns>
    [HttpGet("client/{clientId}/default")]
    [Authorize(Policy = PolicyNames.ProviderTeamAccess)]
    public async Task<IActionResult> GetDefaultPaymentMethod(Guid clientId)
    {
        var query = new GetDefaultPaymentMethodQuery
        {
            ClientId = clientId
        };

        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.ErrorMessage });
        }

        if (result.Value == null || !result.Value.HasDefault)
        {
            return NotFound(new { message = "No default payment method found" });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Gets a specific payment method by ID for the current user's client
    /// </summary>
    /// <param name="id">Payment method ID</param>
    /// <returns>Payment method details</returns>
    [HttpGet("{id}")]
    [Authorize(Policy = PolicyNames.ClientAccess)]
    public async Task<IActionResult> GetPaymentMethodById(Guid id)
    {
        // SECURITY: Get client ID from authenticated user context
        var clientId = await _userContextService.GetCurrentClientIdAsync();
        if (clientId == null)
        {
            return Forbid("Access denied - client not found");
        }

        var query = new GetPaymentMethodByIdQuery
        {
            PaymentMethodId = id,
            ClientId = clientId.Value
        };

        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.ErrorMessage });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Gets a specific payment method by ID for a specific client (Provider/Admin only)
    /// </summary>
    /// <param name="id">Payment method ID</param>
    /// <param name="clientId">Client ID</param>
    /// <returns>Payment method details</returns>
    [HttpGet("{id}/client/{clientId}")]
    [Authorize(Policy = PolicyNames.ProviderTeamAccess)]
    public async Task<IActionResult> GetPaymentMethodByIdForClient(Guid id, Guid clientId)
    {
        var query = new GetPaymentMethodByIdQuery
        {
            PaymentMethodId = id,
            ClientId = clientId
        };

        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.ErrorMessage });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Adds a new payment method for the current user's client
    /// </summary>
    /// <param name="command">Add payment method command</param>
    /// <returns>Created payment method</returns>
    [HttpPost]
    [Authorize(Policy = PolicyNames.ClientAccess)]
    public async Task<IActionResult> AddPaymentMethod([FromBody] AddPaymentMethodCommand command)
    {
        // SECURITY: Get client ID from authenticated user context and set it in command
        var clientId = await _userContextService.GetCurrentClientIdAsync();
        if (clientId == null)
        {
            return Forbid("Access denied - client not found");
        }

        // Override client ID in command for security
        command = command with { ClientId = clientId.Value };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.ErrorMessage });
        }

        return CreatedAtAction(
            nameof(GetPaymentMethodById),
            new { id = result.Value?.PaymentMethodId },
            result.Value);
    }

    /// <summary>
    /// Sets a payment method as default for the current user's client
    /// </summary>
    /// <param name="id">Payment method ID</param>
    /// <returns>Updated payment method</returns>
    [HttpPut("{id}/set-default")]
    [Authorize(Policy = PolicyNames.ClientAccess)]
    public async Task<IActionResult> SetDefaultPaymentMethod(Guid id)
    {
        // SECURITY: Get client ID from authenticated user context
        var clientId = await _userContextService.GetCurrentClientIdAsync();
        if (clientId == null)
        {
            return Forbid("Access denied - client not found");
        }

        var command = new SetDefaultPaymentMethodCommand
        {
            PaymentMethodId = id,
            ClientId = clientId.Value
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.ErrorMessage });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Sets a payment method as default for a specific client (Provider/Admin only)
    /// </summary>
    /// <param name="id">Payment method ID</param>
    /// <param name="clientId">Client ID</param>
    /// <returns>Updated payment method</returns>
    [HttpPut("{id}/set-default/client/{clientId}")]
    [Authorize(Policy = PolicyNames.ProviderTeamAccess)]
    public async Task<IActionResult> SetDefaultPaymentMethodForClient(Guid id, Guid clientId)
    {
        var command = new SetDefaultPaymentMethodCommand
        {
            PaymentMethodId = id,
            ClientId = clientId
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.ErrorMessage });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Removes a payment method for the current user's client
    /// </summary>
    /// <param name="id">Payment method ID</param>
    /// <returns>Result of removal</returns>
    [HttpDelete("{id}")]
    [Authorize(Policy = PolicyNames.ClientAccess)]
    public async Task<IActionResult> RemovePaymentMethod(Guid id)
    {
        // SECURITY: Get client ID from authenticated user context
        var clientId = await _userContextService.GetCurrentClientIdAsync();
        if (clientId == null)
        {
            return Forbid("Access denied - client not found");
        }

        var command = new RemovePaymentMethodCommand
        {
            PaymentMethodId = id,
            ClientId = clientId.Value
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.ErrorMessage });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Removes a payment method for a specific client (Provider/Admin only)
    /// </summary>
    /// <param name="id">Payment method ID</param>
    /// <param name="clientId">Client ID</param>
    /// <returns>Result of removal</returns>
    [HttpDelete("{id}/client/{clientId}")]
    [Authorize(Policy = PolicyNames.ProviderTeamAccess)]
    public async Task<IActionResult> RemovePaymentMethodForClient(Guid id, Guid clientId)
    {
        var command = new RemovePaymentMethodCommand
        {
            PaymentMethodId = id,
            ClientId = clientId
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.ErrorMessage });
        }

        return Ok(result.Value);
    }
}
