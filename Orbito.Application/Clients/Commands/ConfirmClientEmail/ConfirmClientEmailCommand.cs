using MediatR;
using Orbito.Domain.Common;

namespace Orbito.Application.Clients.Commands.ConfirmClientEmail;

public record ConfirmClientEmailCommand : IRequest<Result>
{
    public string Token { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
