using MediatR;
using Orbito.Domain.Common;
using Orbito.Domain.Enums;

namespace Orbito.Application.Features.Payments.Queries.GetAllPayments
{
    public record GetAllPaymentsQuery(
        int PageNumber = 1,
        int PageSize = 10,
        string? SearchTerm = null,
        PaymentStatus? Status = null,
        Guid? ClientId = null
    ) : IRequest<Result<GetAllPaymentsResponse>>;
}
