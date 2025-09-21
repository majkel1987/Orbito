using MediatR;

namespace Orbito.Application.Clients.Commands.CreateClient
{
    public record CreateClientCommand(
        Guid? UserId,
        string? CompanyName,
        string? Phone,
        string? DirectEmail,
        string? DirectFirstName,
        string? DirectLastName
    ) : IRequest<CreateClientResult>;
}
