using Orbito.Application.DTOs;

namespace Orbito.Application.Features.Payments.Commands.UpdatePaymentStatus
{
    public record UpdatePaymentStatusResult
    {
        public bool Success { get; init; }
        public string? Message { get; init; }
        public PaymentDto? Payment { get; init; }

        public static UpdatePaymentStatusResult SuccessResult(PaymentDto payment)
        {
            return new UpdatePaymentStatusResult
            {
                Success = true,
                Payment = payment
            };
        }

        public static UpdatePaymentStatusResult FailureResult(string message)
        {
            return new UpdatePaymentStatusResult
            {
                Success = false,
                Message = message
            };
        }
    }
}
