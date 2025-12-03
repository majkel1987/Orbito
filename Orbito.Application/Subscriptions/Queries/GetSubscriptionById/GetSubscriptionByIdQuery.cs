using MediatR;
using Orbito.Application.DTOs;
using Orbito.Domain.Common;

namespace Orbito.Application.Subscriptions.Queries.GetSubscriptionById
{
    public record GetSubscriptionByIdQuery : IRequest<Result<SubscriptionDto>>
    {
        public Guid SubscriptionId { get; init; }
        public bool IncludeDetails { get; init; } = false;
    }
}
