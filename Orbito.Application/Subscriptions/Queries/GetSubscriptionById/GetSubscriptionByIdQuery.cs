using MediatR;

namespace Orbito.Application.Subscriptions.Queries.GetSubscriptionById
{
    public record GetSubscriptionByIdQuery : IRequest<GetSubscriptionByIdResult?>
    {
        public Guid SubscriptionId { get; init; }
        public bool IncludeDetails { get; init; } = false;
    }
}
