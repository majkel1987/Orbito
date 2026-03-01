using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orbito.Application.Common.Authorization;
using Orbito.Application.Clients.Commands.ActivateClient;
using Orbito.Application.Clients.Commands.CreateClient;
using Orbito.Application.Clients.Commands.DeactivateClient;
using Orbito.Application.Clients.Commands.DeleteClient;
using Orbito.Application.Clients.Commands.UpdateClient;
using Orbito.Application.Clients.Queries.GetClientById;
using Orbito.Application.Clients.Queries.GetClientsByProvider;
using Orbito.Application.Clients.Commands.InviteClient;
using Orbito.Application.Clients.Queries.SearchClients;
using Orbito.Application.DTOs;

namespace Orbito.API.Controllers
{
    [Route("api/[controller]")]
    public class ClientsController : BaseController
    {
        public ClientsController(IMediator mediator, ILogger<ClientsController> logger)
            : base(mediator, logger)
        {
        }

        /// <summary>
        /// Tworzy nowego klienta
        /// </summary>
        /// <remarks>
        /// **WAŻNE: UserId i DirectEmail są wzajemnie wykluczające się (XOR)**
        /// 
        /// **Scenariusz A - Klient z istniejącym kontem User:**
        /// ```json
        /// {
        ///   "userId": "5fa7c148-7bd9-4ba9-8ac0-211db8c04d46",
        ///   "companyName": "Acme Corporation",
        ///   "phone": "+48123456789"
        /// }
        /// ```
        /// 
        /// **Scenariusz B - Klient bezpośredni (bez konta):**
        /// ```json
        /// {
        ///   "directEmail": "coyote@acme.com",
        ///   "directFirstName": "Willy",
        ///   "directLastName": "Coyote",
        ///   "companyName": "Acme Corporation",
        ///   "phone": "+48123456789"
        /// }
        /// ```
        /// </remarks>
        /// <param name="command">Dane klienta do utworzenia. Musi zawierać UserId LUB DirectEmail (nie oba jednocześnie).</param>
        /// <returns>Utworzony klient</returns>
        /// <response code="201">Klient został utworzony pomyślnie</response>
        /// <response code="400">Błąd walidacji - sprawdź czy podano UserId LUB DirectEmail (nie oba)</response>
        /// <response code="401">Brak autoryzacji</response>
        [HttpPost]
        [Authorize(Policy = PolicyNames.ProviderTeamAccess)]
        [ProducesResponseType(typeof(Orbito.Application.DTOs.ClientDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateClient([FromBody] CreateClientCommand command)
        {
            var result = await Mediator.Send(command);

            if (!result.IsSuccess)
            {
                return HandleResult(result);
            }

            return CreatedAtAction(nameof(GetClientById), new { id = result.Value.Id }, result.Value);
        }

        /// <summary>
        /// Pobiera listę klientów z paginacją i filtrowaniem
        /// </summary>
        /// <param name="pageNumber">Numer strony (domyślnie 1)</param>
        /// <param name="pageSize">Rozmiar strony (domyślnie 10)</param>
        /// <param name="status">Filtr statusu: 'active', 'inactive', lub null/pusty dla wszystkich</param>
        /// <param name="searchTerm">Termin wyszukiwania</param>
        /// <returns>Lista klientów</returns>
        [HttpGet]
        [Authorize(Policy = PolicyNames.ProviderTeamAccess)]
        public async Task<IActionResult> GetClients(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? status = null,
            [FromQuery] string? searchTerm = null)
        {
            var query = new GetClientsByProviderQuery(pageNumber, pageSize, status, searchTerm);
            var result = await Mediator.Send(query);

            // Convert Result<PaginatedList<ClientDto>> to standard Result wrapper format for frontend
            if (!result.IsSuccess)
            {
                return BadRequest(new
                {
                    isSuccess = false,
                    value = default(object),
                    error = result.Error.Code
                });
            }

            // Return Result wrapper with ClientListResponse structure
            // Frontend expects: ApiResult<ClientListResponse> where ClientListResponse = { success, message?, clients, ... }
            return Ok(new
            {
                isSuccess = true,
                value = new
                {
                    success = true,
                    clients = result.Value.Items,
                    totalCount = result.Value.TotalCount,
                    pageNumber = result.Value.PageNumber,
                    pageSize = result.Value.PageSize,
                    totalPages = result.Value.TotalPages,
                    message = default(string)
                },
                error = default(string)
            });
        }

        /// <summary>
        /// Pobiera szczegóły klienta po ID
        /// </summary>
        /// <param name="id">ID klienta</param>
        /// <returns>Szczegóły klienta</returns>
        [HttpGet("{id}")]
        [Authorize(Policy = PolicyNames.ProviderTeamAccess)]
        [ProducesResponseType(typeof(Orbito.Application.DTOs.ClientDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetClientById(Guid id)
        {
            var query = new GetClientByIdQuery(id);
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }

        /// <summary>
        /// Aktualizuje informacje klienta
        /// </summary>
        /// <param name="id">ID klienta</param>
        /// <param name="command">Dane do aktualizacji</param>
        /// <returns>Zaktualizowany klient</returns>
        [HttpPut("{id}")]
        [Authorize(Policy = PolicyNames.ProviderTeamAccess)]
        [ProducesResponseType(typeof(Orbito.Application.DTOs.ClientDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateClient(Guid id, [FromBody] UpdateClientCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("ID in URL does not match ID in request body");
            }

            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// Usuwa klienta (soft delete domyślnie)
        /// </summary>
        /// <param name="id">ID klienta</param>
        /// <param name="hardDelete">Czy wykonać hard delete</param>
        /// <returns>Wynik operacji</returns>
        [HttpDelete("{id}")]
        [Authorize(Policy = PolicyNames.ProviderTeamAccess)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteClient(Guid id, [FromQuery] bool hardDelete = false)
        {
            var command = new DeleteClientCommand(id, hardDelete);
            var result = await Mediator.Send(command);

            if (!result.IsSuccess)
            {
                return HandleResult(result);
            }

            return NoContent();
        }

        /// <summary>
        /// Aktywuje klienta
        /// </summary>
        /// <param name="id">ID klienta</param>
        /// <returns>Zaktualizowany klient</returns>
        [HttpPost("{id}/activate")]
        [Authorize(Policy = PolicyNames.ProviderTeamAccess)]
        [ProducesResponseType(typeof(Orbito.Application.DTOs.ClientDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ActivateClient(Guid id)
        {
            var command = new ActivateClientCommand(id);
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// Dezaktywuje klienta
        /// </summary>
        /// <param name="id">ID klienta</param>
        /// <returns>Zaktualizowany klient</returns>
        [HttpPost("{id}/deactivate")]
        [Authorize(Policy = PolicyNames.ProviderTeamAccess)]
        [ProducesResponseType(typeof(Orbito.Application.DTOs.ClientDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeactivateClient(Guid id)
        {
            var command = new DeactivateClientCommand(id);
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// Zaprasza nowego klienta – tworzy klienta ze statusem Inactive i wysyła email z tokenem
        /// </summary>
        /// <param name="command">Dane klienta do zaproszenia</param>
        /// <returns>ID nowo utworzonego klienta</returns>
        /// <response code="201">Klient zaproszony, email wysłany</response>
        /// <response code="400">Błąd walidacji lub klient z tym emailem już istnieje</response>
        /// <response code="401">Brak autoryzacji</response>
        [HttpPost("invite")]
        [Authorize(Policy = PolicyNames.ProviderTeamAccess)]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> InviteClient([FromBody] InviteClientCommand command)
        {
            var result = await Mediator.Send(command);

            if (!result.IsSuccess)
                return HandleResult(result);

            return StatusCode(StatusCodes.Status201Created, result.Value);
        }

        /// <summary>
        /// Wyszukuje klientów według terminu wyszukiwania
        /// </summary>
        /// <param name="searchTerm">Termin wyszukiwania</param>
        /// <param name="pageNumber">Numer strony</param>
        /// <param name="pageSize">Rozmiar strony</param>
        /// <param name="activeOnly">Czy pokazać tylko aktywnych klientów</param>
        /// <returns>Wyniki wyszukiwania</returns>
        [HttpGet("search")]
        [Authorize(Policy = PolicyNames.ProviderTeamAccess)]
        [ProducesResponseType(typeof(Orbito.Application.Common.Models.PaginatedList<Orbito.Application.DTOs.ClientDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> SearchClients(
            [FromQuery] string searchTerm,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] bool activeOnly = false)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return BadRequest("Search term is required");
            }

            var query = new SearchClientsQuery(searchTerm, pageNumber, pageSize, activeOnly);
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }

    }
}
