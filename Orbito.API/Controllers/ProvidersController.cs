using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orbito.Application.Providers.Commands.CreateProvider;
using Orbito.Application.Providers.Commands.UpdateProvider;
using Orbito.Application.Providers.Commands.DeleteProvider;
using Orbito.Application.Providers.Queries.GetProviderById;
using Orbito.Application.Providers.Queries.GetAllProviders;
using Orbito.Application.Providers.Queries.GetProviderByUserId;
using Orbito.Domain.Enums;

namespace Orbito.API.Controllers
{
    [Route("api/[controller]")]
    public class ProvidersController : BaseController
    {
        public ProvidersController(IMediator mediator, ILogger<ProvidersController> logger)
            : base(mediator, logger)
        {
        }

        /// <summary>
        /// Pobiera wszystkich providerów z paginacją
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "PlatformAdmin")]
        public async Task<IActionResult> GetAllProviders(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] bool activeOnly = false)
        {
            var query = new GetAllProvidersQuery(pageNumber, pageSize, activeOnly);
            var result = await Mediator.Send(query);

            if (result.IsFailure)
            {
                return HandleResult(result);
            }

            // Custom response format for frontend compatibility
            return Ok(new
            {
                providers = result.Value.Items,
                pagination = new
                {
                    pageNumber = result.Value.PageNumber,
                    pageSize = result.Value.PageSize,
                    totalCount = result.Value.TotalCount,
                    totalPages = result.Value.TotalPages
                }
            });
        }

        /// <summary>
        /// Pobiera providera po ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetProviderById(Guid id)
        {
            var query = new GetProviderByIdQuery(id);
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }

        /// <summary>
        /// Pobiera providera po ID użytkownika
        /// </summary>
        [HttpGet("by-user/{userId}")]
        [Authorize]
        public async Task<IActionResult> GetProviderByUserId(Guid userId)
        {
            var query = new GetProviderByUserIdQuery(userId);
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }

        /// <summary>
        /// Tworzy nowego providera (dostawcę usług)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "PlatformAdmin")]
        public async Task<IActionResult> CreateProvider([FromBody] CreateProviderRequest request)
        {
            var command = new CreateProviderCommand(
                request.UserId,
                request.BusinessName,
                request.SubdomainSlug,
                request.Description,
                request.Avatar,
                request.CustomDomain);

            var result = await Mediator.Send(command);

            if (result.IsFailure)
            {
                return HandleResult(result);
            }

            Logger.LogInformation("Provider utworzony: {BusinessName} (ID: {ProviderId})",
                result.Value.BusinessName, result.Value.ProviderId);

            return Ok(new
            {
                message = "Provider został pomyślnie utworzony",
                provider = result.Value
            });
        }

        /// <summary>
        /// Aktualizuje informacje providera
        /// </summary>
        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(Orbito.Application.DTOs.ProviderDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProvider(Guid id, [FromBody] UpdateProviderRequest request)
        {
            var command = new UpdateProviderCommand(
                id,
                request.BusinessName,
                request.Description,
                request.Avatar,
                request.SubdomainSlug,
                request.CustomDomain);

            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// Usuwa providera (soft delete - deaktywacja lub hard delete)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "PlatformAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteProvider(Guid id, [FromQuery] bool hardDelete = false)
        {
            var command = new DeleteProviderCommand(id, hardDelete);
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }
    }

    public class CreateProviderRequest
    {
        public Guid UserId { get; set; }
        public string BusinessName { get; set; } = string.Empty;
        public string SubdomainSlug { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Avatar { get; set; }
        public string? CustomDomain { get; set; }
    }

    public class UpdateProviderRequest
    {
        public string BusinessName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Avatar { get; set; }
        public string? SubdomainSlug { get; set; }
        public string? CustomDomain { get; set; }
    }
}
