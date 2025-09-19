using MediatR;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;

namespace Orbito.Application.Providers.Commands.DeleteProvider
{
    public record DeleteProviderCommand(
        Guid Id,
        bool HardDelete = false
    ) : IRequest<DeleteProviderResult>;

    public record DeleteProviderResult
    {
        public bool Success { get; init; }
        public string? Message { get; init; }
        public bool WasHardDelete { get; init; }
        public List<string> Errors { get; init; } = new();

        public static DeleteProviderResult SuccessResult(bool wasHardDelete, string message)
        {
            return new DeleteProviderResult
            {
                Success = true,
                Message = message,
                WasHardDelete = wasHardDelete
            };
        }

        public static DeleteProviderResult FailureResult(string message, List<string>? errors = null)
        {
            return new DeleteProviderResult
            {
                Success = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }
    }
}
