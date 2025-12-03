using MediatR;
using Orbito.Application.DTOs;
using Orbito.Domain.Common;

namespace Orbito.Application.Features.Payments.Queries.GetPaymentById
{
    public record GetPaymentByIdQuery(Guid PaymentId, Guid ClientId) : IRequest<Result<PaymentDto>>;
}
