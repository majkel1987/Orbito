using MediatR;
using Orbito.Domain.Common;

namespace Orbito.Application.Clients.Commands.ConfirmClientEmail;

public record ConfirmClientEmailCommand : IRequest<Result>
{
    public required string Token { get; init; }
    public required string Password { get; init; }
}
