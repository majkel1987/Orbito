using MediatR;

namespace Orbito.Application.Features.Payments.Queries.GetPaymentsBySubscription
{
    public record GetPaymentsBySubscriptionQuery(
        Guid SubscriptionId,
        int PageNumber = 1,
        int PageSize = 10
    ) : IRequest<GetPaymentsBySubscriptionResult>;
}
