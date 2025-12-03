using MediatR;
using Orbito.Domain.Common;

namespace Orbito.Application.Providers.Commands.DeleteProvider
{
    public record DeleteProviderCommand(
        Guid Id,
        bool HardDelete = false
    ) : IRequest<Result<Unit>>;
}
