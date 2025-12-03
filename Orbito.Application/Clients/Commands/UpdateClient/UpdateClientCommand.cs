using MediatR;
using Orbito.Application.DTOs;
using Orbito.Domain.Common;

namespace Orbito.Application.Clients.Commands.UpdateClient
{
    public record UpdateClientCommand(
        Guid Id,
        string? CompanyName,
        string? Phone,
        string? DirectEmail,
        string? DirectFirstName,
        string? DirectLastName
    ) : IRequest<Result<ClientDto>>;
}
