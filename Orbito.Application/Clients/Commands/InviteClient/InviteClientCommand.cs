using MediatR;
using Orbito.Domain.Common;

namespace Orbito.Application.Clients.Commands.InviteClient;

public record InviteClientCommand : IRequest<Result<Guid>>
{
    public required string Email { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public string? CompanyName { get; init; }
}
