using MediatR;
using Orbito.Domain.Common;

namespace Orbito.Application.Clients.Commands.ResendClientInvitation;

public record ResendClientInvitationCommand(Guid ClientId) : IRequest<Result>;
