namespace Orbito.Application.Clients.Commands.DeleteClient
{
    public record DeleteClientResult
    {
        public bool Success { get; init; }
        public string? Message { get; init; }

        public static DeleteClientResult SuccessResult()
        {
            return new DeleteClientResult
            {
                Success = true
            };
        }

        public static DeleteClientResult FailureResult(string message)
        {
            return new DeleteClientResult
            {
                Success = false,
                Message = message
            };
        }
    }
}
