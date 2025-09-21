using MediatR;

namespace Orbito.Application.Clients.Commands.UpdateClient
{
    public record UpdateClientCommand(
        Guid Id,
        string? CompanyName,
        string? Phone,
        string? DirectEmail,
        string? DirectFirstName,
        string? DirectLastName
    ) : IRequest<UpdateClientResult>;
}
