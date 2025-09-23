using MediatR;

namespace Orbito.Application.Subscriptions.Queries.GetExpiringSubscriptions
{
    public record GetExpiringSubscriptionsQuery : IRequest<GetExpiringSubscriptionsResult>
    {
        public int DaysBeforeExpiration { get; init; } = 7;
        public int PageNumber { get; init; } = 1;
        public int PageSize { get; init; } = 10;
    }
}
