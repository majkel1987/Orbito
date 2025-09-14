namespace Orbito.Domain.Enums
{
    public enum UserRole
    {
        Provider = 1,      // Dostawca usług (admin swojego tenanta)
        Client = 2,        // Klient dostawcy
        PlatformAdmin = 3  // Administrator całej platformy Orbito
    }
}
