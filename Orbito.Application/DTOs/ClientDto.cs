namespace Orbito.Application.DTOs
{
    public record ClientDto
    {
        public Guid Id { get; init; }
        public Guid TenantId { get; init; }
        public Guid? UserId { get; init; }
        public string? CompanyName { get; init; }
        public string? Phone { get; init; }
        public string? DirectEmail { get; init; }
        public string? DirectFirstName { get; init; }
        public string? DirectLastName { get; init; }
        public bool IsActive { get; init; }
        public DateTime CreatedAt { get; init; }
        public string Email { get; init; } = string.Empty;
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string FullName { get; init; } = string.Empty;
        public string? UserEmail { get; init; }
        public string? UserFirstName { get; init; }
        public string? UserLastName { get; init; }
    }
}
