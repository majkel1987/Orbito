using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orbito.Application.Clients.Commands.ActivateClient;
using Orbito.Application.Clients.Commands.CreateClient;
using Orbito.Application.Clients.Commands.DeactivateClient;
using Orbito.Application.Clients.Commands.DeleteClient;
using Orbito.Application.Clients.Commands.UpdateClient;
using Orbito.Application.Clients.Queries.GetClientById;
using Orbito.Application.Clients.Queries.GetClientStats;
using Orbito.Application.Clients.Queries.GetClientsByProvider;
using Orbito.Application.Clients.Queries.SearchClients;

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
        /// <param name="command">Dane klienta do utworzenia</param>
        /// <returns>Utworzony klient</returns>
        [HttpPost]
        [Authorize(Roles = "Provider,PlatformAdmin")]
        public async Task<ActionResult<CreateClientResult>> CreateClient([FromBody] CreateClientCommand command)
        {
            var result = await Mediator.Send(command);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return CreatedAtAction(nameof(GetClientById), new { id = result.Client!.Id }, result);
        }

        /// <summary>
        /// Pobiera listę klientów z paginacją i filtrowaniem
        /// </summary>
        /// <param name="pageNumber">Numer strony (domyślnie 1)</param>
        /// <param name="pageSize">Rozmiar strony (domyślnie 10)</param>
        /// <param name="activeOnly">Czy pokazać tylko aktywnych klientów</param>
        /// <param name="searchTerm">Termin wyszukiwania</param>
        /// <returns>Lista klientów</returns>
        [HttpGet]
        [Authorize(Roles = "Provider,PlatformAdmin")]
        public async Task<ActionResult<GetClientsByProviderResult>> GetClients(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] bool activeOnly = false,
            [FromQuery] string? searchTerm = null)
        {
            var query = new GetClientsByProviderQuery(pageNumber, pageSize, activeOnly, searchTerm);
            var result = await Mediator.Send(query);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Pobiera szczegóły klienta po ID
        /// </summary>
        /// <param name="id">ID klienta</param>
        /// <returns>Szczegóły klienta</returns>
        [HttpGet("{id}")]
        [Authorize(Roles = "Provider,PlatformAdmin")]
        public async Task<ActionResult<GetClientByIdResult>> GetClientById(Guid id)
        {
            var query = new GetClientByIdQuery(id);
            var result = await Mediator.Send(query);

            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Aktualizuje informacje klienta
        /// </summary>
        /// <param name="id">ID klienta</param>
        /// <param name="command">Dane do aktualizacji</param>
        /// <returns>Zaktualizowany klient</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Provider,PlatformAdmin")]
        public async Task<ActionResult<UpdateClientResult>> UpdateClient(Guid id, [FromBody] UpdateClientCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("ID in URL does not match ID in request body");
            }

            var result = await Mediator.Send(command);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Usuwa klienta (soft delete domyślnie)
        /// </summary>
        /// <param name="id">ID klienta</param>
        /// <param name="hardDelete">Czy wykonać hard delete</param>
        /// <returns>Wynik operacji</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Provider,PlatformAdmin")]
        public async Task<ActionResult<DeleteClientResult>> DeleteClient(Guid id, [FromQuery] bool hardDelete = false)
        {
            var command = new DeleteClientCommand(id, hardDelete);
            var result = await Mediator.Send(command);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Aktywuje klienta
        /// </summary>
        /// <param name="id">ID klienta</param>
        /// <returns>Zaktualizowany klient</returns>
        [HttpPost("{id}/activate")]
        [Authorize(Roles = "Provider,PlatformAdmin")]
        public async Task<ActionResult<ActivateClientResult>> ActivateClient(Guid id)
        {
            var command = new ActivateClientCommand(id);
            var result = await Mediator.Send(command);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Dezaktywuje klienta
        /// </summary>
        /// <param name="id">ID klienta</param>
        /// <returns>Zaktualizowany klient</returns>
        [HttpPost("{id}/deactivate")]
        [Authorize(Roles = "Provider,PlatformAdmin")]
        public async Task<ActionResult<DeactivateClientResult>> DeactivateClient(Guid id)
        {
            var command = new DeactivateClientCommand(id);
            var result = await Mediator.Send(command);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
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
        [Authorize(Roles = "Provider,PlatformAdmin")]
        public async Task<ActionResult<SearchClientsResult>> SearchClients(
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

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Pobiera statystyki klientów
        /// </summary>
        /// <returns>Statystyki klientów</returns>
        [HttpGet("stats")]
        [Authorize(Roles = "Provider,PlatformAdmin")]
        public async Task<ActionResult<GetClientStatsResult>> GetClientStats()
        {
            var query = new GetClientStatsQuery();
            var result = await Mediator.Send(query);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
