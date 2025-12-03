namespace Orbito.Application.Common.Interfaces
{
    /// <summary>
    /// Allows temporary bypass of tenant validation for admin setup operations
    /// Should only be used during initial admin seeding when no tenant context exists
    /// </summary>
    public interface ITenantValidationBypass
    {
        /// <summary>
        /// Temporarily disables tenant validation for admin setup operations
        /// </summary>
        void SkipTenantValidation();

        /// <summary>
        /// Re-enables tenant validation after admin setup operations
        /// </summary>
        void ResetTenantValidation();
    }
}

