using MediatR;
using Orbito.Application.DTOs;
using Orbito.Domain.Common;

namespace Orbito.Application.Subscriptions.Queries.GetSubscriptionsByClient
{
    public record GetSubscriptionsByClientQuery : IRequest<Result<Common.Models.PaginatedList<SubscriptionDto>>>
    {
        public Guid ClientId { get; init; }
        public int PageNumber { get; init; } = 1;
        public int PageSize { get; init; } = 10;
        public bool ActiveOnly { get; init; } = false;
    }
}
