using Orbito.Application.Clients.Commands.CreateClient;

namespace Orbito.Application.Clients.Commands.ActivateClient
{
    public record ActivateClientResult
    {
        public bool Success { get; init; }
        public string? Message { get; init; }
        public ClientDto? Client { get; init; }

        public static ActivateClientResult SuccessResult(ClientDto client)
        {
            return new ActivateClientResult
            {
                Success = true,
                Client = client
            };
        }

        public static ActivateClientResult FailureResult(string message)
        {
            return new ActivateClientResult
            {
                Success = false,
                Message = message
            };
        }
    }
}
