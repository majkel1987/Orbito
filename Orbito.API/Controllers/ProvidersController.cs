using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orbito.Application.Providers.Commands.CreateProvider;
using Orbito.Domain.Enums;

namespace Orbito.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProvidersController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ProvidersController> _logger;

        public ProvidersController(IMediator mediator, ILogger<ProvidersController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Tworzy nowego providera (dostawcę usług)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "PlatformAdmin")]
        public async Task<IActionResult> CreateProvider([FromBody] CreateProviderRequest request)
        {
            try
            {
                var command = new CreateProviderCommand(
                    request.UserId,
                    request.BusinessName,
                    request.SubdomainSlug,
                    request.Description,
                    request.Avatar,
                    request.CustomDomain);

                var result = await _mediator.Send(command);

                _logger.LogInformation("Provider utworzony: {BusinessName} (ID: {ProviderId})", 
                    result.BusinessName, result.ProviderId);

                return Ok(new
                {
                    message = "Provider został pomyślnie utworzony",
                    provider = result
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Błąd walidacji podczas tworzenia providera");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas tworzenia providera");
                return StatusCode(500, new { message = "Wystąpił błąd podczas tworzenia providera" });
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
}
