using MediatR;
using Orbito.Application.DTOs;
using Orbito.Domain.Common;

namespace Orbito.Application.Features.Payments.Queries.GetPaymentsBySubscription
{
    public record GetPaymentsBySubscriptionQuery(
        Guid SubscriptionId,
        Guid ClientId,
        int PageNumber = 1,
        int PageSize = 10
    ) : IRequest<Result<GetPaymentsBySubscriptionResponse>>;
}
