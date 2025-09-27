using Orbito.Application.DTOs;

namespace Orbito.Application.Features.Payments.Commands.ProcessPayment
{
    public record ProcessPaymentResult
    {
        public bool Success { get; init; }
        public string? Message { get; init; }
        public PaymentDto? Payment { get; init; }

        public static ProcessPaymentResult SuccessResult(PaymentDto payment)
        {
            return new ProcessPaymentResult
            {
                Success = true,
                Payment = payment
            };
        }

        public static ProcessPaymentResult FailureResult(string message)
        {
            return new ProcessPaymentResult
            {
                Success = false,
                Message = message
            };
        }
    }
}
