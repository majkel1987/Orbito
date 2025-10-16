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
            try
            {
                var query = new GetAllProvidersQuery(pageNumber, pageSize, activeOnly);
                var result = await Mediator.Send(query);

                if (!result.Success)
                {
                    return BadRequest(new { message = result.Message });
                }

                return Ok(new
                {
                    providers = result.Providers,
                    pagination = new
                    {
                        pageNumber = result.PageNumber,
                        pageSize = result.PageSize,
                        totalCount = result.TotalCount,
                        totalPages = result.TotalPages
                    }
                });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Błąd podczas pobierania providerów");
                return StatusCode(500, new { message = "Wystąpił błąd podczas pobierania providerów" });
            }
        }

        /// <summary>
        /// Pobiera providera po ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetProviderById(Guid id)
        {
            try
            {
                var query = new GetProviderByIdQuery(id);
                var result = await Mediator.Send(query);

                if (!result.Success)
                {
                    return NotFound(new { message = result.Message });
                }

                return Ok(new { provider = result.Provider });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Błąd podczas pobierania providera: {ProviderId}", id);
                return StatusCode(500, new { message = "Wystąpił błąd podczas pobierania providera" });
            }
        }

        /// <summary>
        /// Pobiera providera po ID użytkownika
        /// </summary>
        [HttpGet("by-user/{userId}")]
        [Authorize]
        public async Task<IActionResult> GetProviderByUserId(Guid userId)
        {
            try
            {
                var query = new GetProviderByUserIdQuery(userId);
                var result = await Mediator.Send(query);

                if (!result.Success)
                {
                    return NotFound(new { message = result.Message });
                }

                return Ok(new { provider = result.Provider });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Błąd podczas pobierania providera dla użytkownika: {UserId}", userId);
                return StatusCode(500, new { message = "Wystąpił błąd podczas pobierania providera" });
            }
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
        public async Task<IActionResult> UpdateProvider(Guid id, [FromBody] UpdateProviderRequest request)
        {
            try
            {
                var command = new UpdateProviderCommand(
                    id,
                    request.BusinessName,
                    request.Description,
                    request.Avatar,
                    request.SubdomainSlug,
                    request.CustomDomain);

                var result = await Mediator.Send(command);

                if (!result.Success)
                {
                    return BadRequest(new
                    {
                        message = result.Message,
                        errors = result.Errors
                    });
                }

                Logger.LogInformation("Provider zaktualizowany: {ProviderId}", id);

                return Ok(new
                {
                    message = result.Message,
                    provider = result.Provider
                });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Błąd podczas aktualizacji providera: {ProviderId}", id);
                return StatusCode(500, new { message = "Wystąpił błąd podczas aktualizacji providera" });
            }
        }

        /// <summary>
        /// Usuwa providera (soft delete - deaktywacja)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "PlatformAdmin")]
        public async Task<IActionResult> DeleteProvider(Guid id, [FromQuery] bool hardDelete = false)
        {
            try
            {
                var command = new DeleteProviderCommand(id, hardDelete);
                var result = await Mediator.Send(command);

                if (!result.Success)
                {
                    return BadRequest(new
                    {
                        message = result.Message,
                        errors = result.Errors
                    });
                }

                Logger.LogInformation("Provider usunięty: {ProviderId}, HardDelete: {HardDelete}", 
                    id, result.WasHardDelete);

                return Ok(new
                {
                    message = result.Message,
                    wasHardDelete = result.WasHardDelete
                });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Błąd podczas usuwania providera: {ProviderId}", id);
                return StatusCode(500, new { message = "Wystąpił błąd podczas usuwania providera" });
            }
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
