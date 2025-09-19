namespace Orbito.Application.Common.Interfaces
{
    public interface IAdminSetupService
    {
        Task<bool> IsAdminSetupRequiredAsync();
        Task<bool> CreateInitialAdminAsync(string email, string password, string firstName, string lastName);
        Task<bool> IsAdminSetupEnabledAsync();
    }
}
