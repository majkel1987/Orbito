using MediatR;
using Orbito.Application.Clients.Commands.CreateClient;

namespace Orbito.Application.Clients.Queries.GetClientById
{
    public record GetClientByIdQuery(Guid Id) : IRequest<GetClientByIdResult>;

    public record GetClientByIdResult
    {
        public bool Success { get; init; }
        public string? Message { get; init; }
        public ClientDto? Client { get; init; }

        public static GetClientByIdResult SuccessResult(ClientDto client)
        {
            return new GetClientByIdResult
            {
                Success = true,
                Client = client
            };
        }

        public static GetClientByIdResult NotFoundResult(string message = "Client not found")
        {
            return new GetClientByIdResult
            {
                Success = false,
                Message = message
            };
        }
    }
}
