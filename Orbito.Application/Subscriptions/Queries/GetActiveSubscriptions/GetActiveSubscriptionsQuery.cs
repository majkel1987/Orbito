using MediatR;
using Orbito.Application.DTOs;
using Orbito.Domain.Common;

namespace Orbito.Application.Subscriptions.Queries.GetActiveSubscriptions
{
    public record GetActiveSubscriptionsQuery : IRequest<Result<Common.Models.PaginatedList<SubscriptionDto>>>
    {
        public int PageNumber { get; init; } = 1;
        public int PageSize { get; init; } = 10;
        public string? SearchTerm { get; init; }
    }
}
