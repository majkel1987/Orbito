Szczegółowa Analiza Kodu ApplicationDbContext
🔴 KRYTYCZNE - Bezpieczeństwo

1. POWAŻNA LUKA - Silent Failure daje Admin Access
   csharpprivate Guid GetCurrentTenantIdSafe()
   {
   try
   {
   return \_tenantProvider?.GetCurrentTenantIdAsGuid() ?? Guid.Empty;
   }
   catch (Exception)
   {
   // Log error and return empty GUID (admin context)
   // This prevents application crashes due to tenant provider issues
   return Guid.Empty; // ⚠️ NIEBEZPIECZNE!
   }
   }
   Problemy:

❌ Komentarz mówi "Log error" ale NIE MA LOGOWANIA
❌ Każdy błąd w tenant provider daje dostęp do WSZYSTKICH danych (Guid.Empty = admin)
❌ Atakujący może celowo wywołać błąd by otrzymać admin access
❌ Brak informacji o błędzie - niemożliwe debugowanie

Poprawka:
csharpprivate readonly ILogger<ApplicationDbContext> \_logger;

private Guid GetCurrentTenantIdSafe()
{
try
{
var tenantId = \_tenantProvider?.GetCurrentTenantIdAsGuid();
if (!tenantId.HasValue || tenantId == Guid.Empty)
{
\_logger.LogWarning("Tenant ID not available, denying access");
throw new UnauthorizedAccessException("Tenant context required");
}
return tenantId.Value;
}
catch (Exception ex)
{
\_logger.LogError(ex, "Failed to get tenant ID - SECURITY VIOLATION");
throw new UnauthorizedAccessException("Unable to determine tenant context", ex);
}
} 2. BRAK Write Protection dla Multi-Tenancy
Query filters chronią tylko odczyt. Nic nie powstrzymuje użytkownika przed:
csharp// Użytkownik TenantA może próbować:
var payment = new Payment { TenantId = TenantId.Create(TenantB_Guid), ... };
context.Payments.Add(payment);
await context.SaveChangesAsync(); // ⚠️ ZAPISZE SIĘ!
Poprawka - Override SaveChanges:
csharppublic override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
{
ValidateTenantContext();
SetAuditFields();
return await base.SaveChangesAsync(cancellationToken);
}

private void ValidateTenantContext()
{
var currentTenantId = GetCurrentTenantIdSafe();

    // Skip validation for admin context
    if (currentTenantId == AdminTenantId)
        return;

    var entries = ChangeTracker.Entries()
        .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

    foreach (var entry in entries)
    {
        if (entry.Entity is IHasTenant hasTenant)
        {
            // For new entities, set TenantId automatically
            if (entry.State == EntityState.Added)
            {
                if (hasTenant.TenantId.Value == Guid.Empty)
                {
                    hasTenant.TenantId = TenantId.Create(currentTenantId);
                }
            }

            // Validate tenant ownership
            if (hasTenant.TenantId.Value != currentTenantId)
            {
                throw new UnauthorizedAccessException(
                    $"Cannot modify entity belonging to different tenant. " +
                    $"Current: {currentTenantId}, Entity: {hasTenant.TenantId.Value}");
            }
        }
    }

} 3. Magic Values - Brak stałych
csharp// Wszędzie: GetCurrentTenantIdSafe() == Guid.Empty
Poprawka:
csharpprivate static readonly Guid AdminTenantId = Guid.Empty;
private static readonly Guid SystemTenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");

// Użycie:
.HasQueryFilter(p => GetCurrentTenantIdSafe() == AdminTenantId || p.TenantId.Value == GetCurrentTenantIdSafe());
🟡 WAŻNE - Błędy i Bugi 4. Brak Audit Trail
csharp// Brak automatycznego ustawiania:
// - CreatedAt, UpdatedAt
// - CreatedBy, UpdatedBy
// - ModifiedProperties
Poprawka:
csharpprivate void SetAuditFields()
{
var entries = ChangeTracker.Entries()
.Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

    var now = DateTime.UtcNow;
    var userId = GetCurrentUserIdSafe();

    foreach (var entry in entries)
    {
        if (entry.Entity is IAuditableEntity auditable)
        {
            if (entry.State == EntityState.Added)
            {
                auditable.CreatedAt = now;
                auditable.CreatedBy = userId;
            }

            auditable.UpdatedAt = now;
            auditable.UpdatedBy = userId;
        }
    }

} 5. Seed Data - Potencjalne kolizje
csharpbuilder.Entity<ApplicationRole>().HasData(
new ApplicationRole("PlatformAdmin")
{
Id = platformAdminRoleId, // ⚠️ Jeśli już istnieje?
// ...
}
);
Problem: Brak sprawdzenia czy dane już istnieją. W migrations może powodować konflikty. 6. Performance - GetCurrentTenantIdSafe() wywoływane wielokrotnie
Każdy query filter wywołuje metodę na nowo:
csharp.HasQueryFilter(p => GetCurrentTenantIdSafe() == Guid.Empty || ...)
// ↑ Wywoływane dla KAŻDEGO rekordu w query!
Poprawka - Cache w scope request:
csharpprivate Guid? \_cachedTenantId;

private Guid GetCurrentTenantIdSafe()
{
if (\_cachedTenantId.HasValue)
return \_cachedTenantId.Value;

    try
    {
        _cachedTenantId = _tenantProvider?.GetCurrentTenantIdAsGuid() ?? Guid.Empty;
        return _cachedTenantId.Value;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to get tenant ID");
        throw new UnauthorizedAccessException("Unable to determine tenant context", ex);
    }

}
🟢 Średnie - Jakość Kodu 7. Duplikacja Query Filters
12 prawie identycznych query filters. Można uprościć:
csharpprivate void ConfigureTenantFiltering(ModelBuilder builder)
{
var tenantEntityTypes = new[]
{
typeof(Provider), typeof(Client), typeof(SubscriptionPlan),
typeof(Subscription), typeof(Payment), typeof(PaymentMethod),
typeof(PaymentHistory), typeof(PaymentWebhookLog),
typeof(EmailNotification), typeof(PaymentRetrySchedule)
};

    foreach (var entityType in tenantEntityTypes)
    {
        var method = typeof(ApplicationDbContext)
            .GetMethod(nameof(SetTenantQueryFilter), BindingFlags.NonPublic | BindingFlags.Static)
            ?.MakeGenericMethod(entityType);

        method?.Invoke(null, new[] { builder });
    }

    // Specjalne przypadki
    builder.Entity<ApplicationRole>()
        .HasQueryFilter(r => GetCurrentTenantIdSafe() == AdminTenantId ||
                             r.TenantId == null ||
                             r.TenantId == GetCurrentTenantIdSafe());

    builder.Entity<ApplicationUser>()
        .HasQueryFilter(u => GetCurrentTenantIdSafe() == AdminTenantId ||
                             u.TenantId == GetCurrentTenantIdSafe());

}

private static void SetTenantQueryFilter<T>(ModelBuilder builder) where T : class, IHasTenant
{
builder.Entity<T>()
.HasQueryFilter(e => GetCurrentTenantIdSafe() == AdminTenantId ||
e.TenantId.Value == GetCurrentTenantIdSafe());
} 8. Brak Soft Delete
csharp// Rozważ dodanie:
public interface ISoftDeletable
{
bool IsDeleted { get; set; }
DateTime? DeletedAt { get; set; }
Guid? DeletedBy { get; set; }
}

// W query filters:
.HasQueryFilter(e => !e.IsDeleted && (GetCurrentTenantIdSafe() == AdminTenantId || ...))
📊 Podsumowanie Priorytetów
PriorytetProblemRyzykoTrudność naprawy🔴 P0Silent failure = Admin accessKRYTYCZNEŚrednia🔴 P0Brak write protection tenancyKRYTYCZNEŚrednia🔴 P0Brak logowaniaWysokieŁatwa🟡 P1Brak audit trailŚrednieŚrednia🟡 P1Performance - cache tenant IDŚrednieŁatwa🟡 P2Magic valuesNiskieŁatwa🟢 P3Duplikacja koduNiskieŚrednia
✅ Co jest dobrze:

✅ Konsekwentne użycie query filters
✅ Admin bypass dobrze zaprojektowany (gdy poprawiony)
✅ Seed data validation
✅ Separacja odpowiedzialności w metodach
✅ Użycie ApplyConfigurationsFromAssembly
