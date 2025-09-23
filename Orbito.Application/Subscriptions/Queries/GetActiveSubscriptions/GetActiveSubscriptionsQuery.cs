using MediatR;

namespace Orbito.Application.Subscriptions.Queries.GetActiveSubscriptions
{
    public record GetActiveSubscriptionsQuery : IRequest<GetActiveSubscriptionsResult>
    {
        public int PageNumber { get; init; } = 1;
        public int PageSize { get; init; } = 10;
        public string? SearchTerm { get; init; }
    }
}
