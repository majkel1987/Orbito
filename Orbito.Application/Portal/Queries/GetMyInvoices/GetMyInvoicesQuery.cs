using MediatR;
using Orbito.Application.DTOs;
using Orbito.Domain.Common;

namespace Orbito.Application.Portal.Queries.GetMyInvoices
{
    public record GetMyInvoicesQuery : IRequest<Result<List<PaymentDto>>>
    {
        public int PageNumber { get; init; } = 1;
        public int PageSize { get; init; } = 20;
    }
}
