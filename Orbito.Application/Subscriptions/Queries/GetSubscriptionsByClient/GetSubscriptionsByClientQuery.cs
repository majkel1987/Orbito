using MediatR;

namespace Orbito.Application.Subscriptions.Queries.GetSubscriptionsByClient
{
    public record GetSubscriptionsByClientQuery : IRequest<GetSubscriptionsByClientResult>
    {
        public Guid ClientId { get; init; }
        public int PageNumber { get; init; } = 1;
        public int PageSize { get; init; } = 10;
        public bool ActiveOnly { get; init; } = false;
    }
}
