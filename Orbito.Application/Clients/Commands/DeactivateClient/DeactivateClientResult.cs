using Orbito.Application.Clients.Commands.CreateClient;

namespace Orbito.Application.Clients.Commands.DeactivateClient
{
    public record DeactivateClientResult
    {
        public bool Success { get; init; }
        public string? Message { get; init; }
        public ClientDto? Client { get; init; }

        public static DeactivateClientResult SuccessResult(ClientDto client)
        {
            return new DeactivateClientResult
            {
                Success = true,
                Client = client
            };
        }

        public static DeactivateClientResult FailureResult(string message)
        {
            return new DeactivateClientResult
            {
                Success = false,
                Message = message
            };
        }
    }
}
