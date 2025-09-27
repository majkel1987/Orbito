using Orbito.Application.DTOs;

namespace Orbito.Application.Features.Payments.Queries.GetPaymentById
{
    public record GetPaymentByIdResult
    {
        public bool Success { get; init; }
        public string? Message { get; init; }
        public PaymentDto? Payment { get; init; }

        public static GetPaymentByIdResult SuccessResult(PaymentDto payment)
        {
            return new GetPaymentByIdResult
            {
                Success = true,
                Payment = payment
            };
        }

        public static GetPaymentByIdResult FailureResult(string message)
        {
            return new GetPaymentByIdResult
            {
                Success = false,
                Message = message
            };
        }
    }
}
