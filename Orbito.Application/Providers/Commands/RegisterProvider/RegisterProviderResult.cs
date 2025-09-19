namespace Orbito.Application.Providers.Commands.RegisterProvider
{
    public record RegisterProviderResult
    {
        public bool Success { get; init; }
        public string? Message { get; init; }
        public Guid? UserId { get; init; }
        public Guid? ProviderId { get; init; }
        public string? BusinessName { get; init; }
        public string? SubdomainSlug { get; init; }
        public List<string> Errors { get; init; } = new();

        public static RegisterProviderResult SuccessResult(Guid userId, Guid providerId, string businessName, string subdomainSlug)
        {
            return new RegisterProviderResult
            {
                Success = true,
                Message = "Provider został pomyślnie zarejestrowany",
                UserId = userId,
                ProviderId = providerId,
                BusinessName = businessName,
                SubdomainSlug = subdomainSlug
            };
        }

        public static RegisterProviderResult FailureResult(string message, List<string>? errors = null)
        {
            return new RegisterProviderResult
            {
                Success = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }
    }
}
