using Orbito.Application.Clients.Commands.CreateClient;

namespace Orbito.Application.Clients.Commands.UpdateClient
{
    public record UpdateClientResult
    {
        public bool Success { get; init; }
        public string? Message { get; init; }
        public ClientDto? Client { get; init; }

        public static UpdateClientResult SuccessResult(ClientDto client)
        {
            return new UpdateClientResult
            {
                Success = true,
                Client = client
            };
        }

        public static UpdateClientResult FailureResult(string message)
        {
            return new UpdateClientResult
            {
                Success = false,
                Message = message
            };
        }
    }
}
