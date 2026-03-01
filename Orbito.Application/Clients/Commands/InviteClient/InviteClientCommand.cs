using MediatR;
using Orbito.Domain.Common;

namespace Orbito.Application.Clients.Commands.InviteClient;

public record InviteClientCommand : IRequest<Result<Guid>>
{
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? CompanyName { get; init; }
}
