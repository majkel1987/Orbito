using MediatR;

namespace Orbito.Application.Features.Payments.Queries.GetPaymentById
{
    public record GetPaymentByIdQuery(Guid PaymentId) : IRequest<GetPaymentByIdResult>;
}
