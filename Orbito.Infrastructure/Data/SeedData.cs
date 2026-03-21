using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.Identity;
using Orbito.Domain.ValueObjects;
using System.Reflection;

namespace Orbito.Infrastructure.Data;

/// <summary>
/// Seeds the database with test data for development purposes
/// </summary>
public static class SeedData
{
    private static readonly Random _random = new();

    public static async Task SeedDevelopmentDataAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        logger.LogInformation("Starting database seeding...");

        // Apply pending migrations
        await context.Database.MigrateAsync();

        try
        {
            // Bypass tenant validation for seeding operations
            if (context is ITenantValidationBypass tenantBypass)
            {
                tenantBypass.SkipTenantValidation();
            }

            // Seed platform plans (Orbito's subscription plans for Providers) - global, no tenant
            await SeedPlatformPlansAsync(context, logger);

            // Seed PlatformAdmin provider (required for Provider-as-Client feature)
            await SeedPlatformAdminAsync(context, userManager, logger);

            // Seed test users and providers
            var provider = await SeedProviderWithTeamAsync(context, userManager, logger);
            if (provider == null) return; // Data already exists

            var tenantId = provider.TenantId;

            // Seed subscription plans
            var plans = await SeedSubscriptionPlansAsync(context, tenantId, logger);

            // Seed clients
            var clients = await SeedClientsAsync(context, userManager, tenantId, logger);

            // Seed subscriptions
            await SeedSubscriptionsAsync(context, tenantId, clients, plans, logger);

            // Seed payments
            await SeedPaymentsAsync(context, tenantId, clients, logger);

            await context.SaveChangesAsync();
            logger.LogInformation("Database seeding completed successfully!");
        }
        finally
        {
            // Always reset tenant validation after seeding
            if (context is ITenantValidationBypass tenantBypass)
            {
                tenantBypass.ResetTenantValidation();
            }
        }
    }

    private static async Task SeedPlatformPlansAsync(
        ApplicationDbContext context,
        ILogger logger)
    {
        // Check if platform plans already exist
        if (await context.PlatformPlans.AnyAsync())
        {
            logger.LogInformation("Platform plans already seeded. Skipping...");
            return;
        }

        logger.LogInformation("Seeding platform plans (Orbito's subscription plans for Providers)...");

        var plans = new List<PlatformPlan>
        {
            PlatformPlan.Create(
                name: "Starter",
                price: Money.Create(49m, "PLN"),
                trialDays: 14,
                description: "Plan dla początkujących - idealna opcja na start z Orbito",
                featuresJson: "[\"Do 10 klientów\",\"Email support\",\"Podstawowa analityka\",\"1 użytkownik\"]",
                sortOrder: 1
            ),
            PlatformPlan.Create(
                name: "Pro",
                price: Money.Create(149m, "PLN"),
                trialDays: 14,
                description: "Plan dla rozwijających się firm - większe możliwości",
                featuresJson: "[\"Do 50 klientów\",\"Priority support\",\"Zaawansowana analityka\",\"Do 5 użytkowników\",\"Integracje API\"]",
                sortOrder: 2
            ),
            PlatformPlan.Create(
                name: "Enterprise",
                price: Money.Create(399m, "PLN"),
                trialDays: 14,
                description: "Plan dla dużych organizacji - pełna elastyczność",
                featuresJson: "[\"Nieograniczona liczba klientów\",\"Dedykowany support 24/7\",\"Custom integracje\",\"Nieograniczona liczba użytkowników\",\"SLA guarantee\",\"White-label\"]",
                sortOrder: 3
            )
        };

        context.PlatformPlans.AddRange(plans);
        await context.SaveChangesAsync();

        logger.LogInformation("Created {Count} platform plans", plans.Count);
    }

    private static async Task SeedPlatformAdminAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger logger)
    {
        // Check if PlatformAdmin already exists
        var existingAdmin = await context.Providers
            .FirstOrDefaultAsync(p => p.SubdomainSlug == "admin");

        if (existingAdmin != null)
        {
            logger.LogInformation("PlatformAdmin already exists. Skipping...");
            return;
        }

        logger.LogInformation("Creating PlatformAdmin provider...");

        // Create PlatformAdmin user
        var adminUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "platform@orbito.pl",
            Email = "platform@orbito.pl",
            FirstName = "Platform",
            LastName = "Admin",
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(adminUser, "PlatformAdmin123!");
        if (!result.Succeeded)
        {
            logger.LogError("Failed to create PlatformAdmin user: {Errors}",
                string.Join(", ", result.Errors.Select(e => e.Description)));
            throw new Exception("Failed to create PlatformAdmin user");
        }

        // Assign both Provider and PlatformAdmin roles
        await userManager.AddToRoleAsync(adminUser, "Provider");
        await userManager.AddToRoleAsync(adminUser, "PlatformAdmin");

        // Create PlatformAdmin provider with subdomain "admin"
        var platformAdmin = Provider.Create(
            adminUser.Id,
            "Orbito Platform",
            "admin"
        );

        adminUser.TenantId = platformAdmin.TenantId;
        await userManager.UpdateAsync(adminUser);

        context.Providers.Add(platformAdmin);
        await context.SaveChangesAsync();

        // Create owner team member
        var ownerMember = new TeamMember(
            platformAdmin.TenantId,
            adminUser.Id,
            TeamMemberRole.Owner,
            adminUser.Email!,
            adminUser.FirstName,
            adminUser.LastName
        );

        context.TeamMembers.Add(ownerMember);
        await context.SaveChangesAsync();

        logger.LogInformation("Created PlatformAdmin provider with TenantId: {TenantId}",
            platformAdmin.TenantId.Value);
    }

    private static async Task<Provider?> SeedProviderWithTeamAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger logger)
    {
        // Check if test provider already exists (skip PlatformAdmin which has subdomain "admin")
        var existingTestProvider = await context.Providers
            .FirstOrDefaultAsync(p => p.SubdomainSlug == "demo");

        if (existingTestProvider != null)
        {
            logger.LogInformation("Test provider already seeded. Skipping...");
            return null;
        }

        logger.LogInformation("Creating test provider and admin user...");

        // Create admin user
        var adminUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "admin@orbito.pl",
            Email = "admin@orbito.pl",
            FirstName = "Jan",
            LastName = "Kowalski",
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(adminUser, "Admin123!");
        if (!result.Succeeded)
        {
            logger.LogError("Failed to create admin user: {Errors}",
                string.Join(", ", result.Errors.Select(e => e.Description)));
            throw new Exception("Failed to create admin user");
        }

        // Assign Provider role
        await userManager.AddToRoleAsync(adminUser, "Provider");

        // Create provider
        var provider = Provider.Create(
            adminUser.Id,
            "Orbito Demo Provider",
            "demo"
        );

        adminUser.TenantId = provider.TenantId;
        await userManager.UpdateAsync(adminUser);

        context.Providers.Add(provider);
        await context.SaveChangesAsync();

        // Create team members
        var teamMembers = new List<TeamMember>
        {
            new TeamMember(
                provider.TenantId,
                adminUser.Id,
                TeamMemberRole.Owner,
                adminUser.Email!,
                adminUser.FirstName,
                adminUser.LastName
            )
        };

        // Add additional team members
        var teamEmails = new[]
        {
            ("anna.nowak@orbito.pl", "Anna", "Nowak", TeamMemberRole.Admin),
            ("piotr.wisniewski@orbito.pl", "Piotr", "Wiśniewski", TeamMemberRole.Member)
        };

        foreach (var (email, firstName, lastName, role) in teamEmails)
        {
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                EmailConfirmed = true,
                TenantId = provider.TenantId
            };

            await userManager.CreateAsync(user, "Member123!");
            await userManager.AddToRoleAsync(user, "Provider");

            teamMembers.Add(new TeamMember(
                provider.TenantId,
                user.Id,
                role,
                email,
                firstName,
                lastName
            ));
        }

        context.TeamMembers.AddRange(teamMembers);
        await context.SaveChangesAsync();

        logger.LogInformation("Created provider '{ProviderName}' with {TeamCount} team members",
            provider.BusinessName, teamMembers.Count);

        return provider;
    }

    private static async Task<List<SubscriptionPlan>> SeedSubscriptionPlansAsync(
        ApplicationDbContext context,
        TenantId tenantId,
        ILogger logger)
    {
        logger.LogInformation("Seeding subscription plans...");

        var plans = new List<SubscriptionPlan>
        {
            SubscriptionPlan.Create(
                tenantId,
                "Starter",
                49.99m,
                "PLN",
                BillingPeriodType.Monthly,
                "Plan dla małych firm - do 10 klientów",
                trialDays: 14,
                trialPeriodDays: 14,
                featuresJson: "[\"10 klientów\",\"Email support\",\"Podstawowa analityka\"]",
                sortOrder: 1
            ),
            SubscriptionPlan.Create(
                tenantId,
                "Business",
                149.99m,
                "PLN",
                BillingPeriodType.Monthly,
                "Plan dla rozwijających się firm - do 50 klientów",
                trialDays: 14,
                trialPeriodDays: 14,
                featuresJson: "[\"50 klientów\",\"Priority support\",\"Zaawansowana analityka\",\"Integracje API\"]",
                sortOrder: 2
            ),
            SubscriptionPlan.Create(
                tenantId,
                "Enterprise",
                499.99m,
                "PLN",
                BillingPeriodType.Monthly,
                "Plan dla dużych organizacji - nieograniczona liczba klientów",
                trialDays: 30,
                trialPeriodDays: 30,
                featuresJson: "[\"Nieograniczona liczba klientów\",\"Dedykowany support 24/7\",\"Custom integracje\",\"SLA guarantee\"]",
                sortOrder: 3
            ),
            SubscriptionPlan.Create(
                tenantId,
                "Annual Business",
                1499.99m,
                "PLN",
                BillingPeriodType.Yearly,
                "Plan roczny Business z rabatem 20%",
                trialDays: 14,
                trialPeriodDays: 14,
                featuresJson: "[\"50 klientów\",\"Priority support\",\"Zaawansowana analityka\",\"20% taniej niż miesięczny\"]",
                sortOrder: 4
            )
        };

        context.SubscriptionPlans.AddRange(plans);
        await context.SaveChangesAsync();

        logger.LogInformation("Created {Count} subscription plans", plans.Count);
        return plans;
    }

    private static async Task<List<Client>> SeedClientsAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        TenantId tenantId,
        ILogger logger)
    {
        logger.LogInformation("Seeding clients...");

        var clients = new List<Client>();

        // Clients with User accounts
        var clientsWithAccounts = new[]
        {
            ("maria.kowalczyk@example.com", "Maria", "Kowalczyk", "ABC Solutions Sp. z o.o."),
            ("tomasz.lewandowski@example.com", "Tomasz", "Lewandowski", "Tech Innovators"),
            ("katarzyna.kaminska@example.com", "Katarzyna", "Kamińska", "Digital Marketing Pro"),
            ("robert.zielinski@example.com", "Robert", "Zieliński", "Consulting Group"),
            ("magdalena.wojcik@example.com", "Magdalena", "Wójcik", "Creative Studio"),
        };

        foreach (var (email, firstName, lastName, company) in clientsWithAccounts)
        {
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                EmailConfirmed = true,
                TenantId = tenantId
            };

            await userManager.CreateAsync(user, "Client123!");
            await userManager.AddToRoleAsync(user, "Client");

            var client = Client.CreateWithUser(tenantId, user.Id, company);
            client.Phone = GeneratePhoneNumber();
            clients.Add(client);
        }

        // Direct clients (without User accounts)
        var directClients = new[]
        {
            ("jan.nowak@firma.pl", "Jan", "Nowak", "Firma Budowlana Nowak"),
            ("anna.kowalska@biznes.pl", "Anna", "Kowalska", "E-Commerce Kings"),
            ("piotr.wisniewski@startup.pl", "Piotr", "Wiśniewski", "AI Startup Lab"),
            ("ewa.dabrowska@agencja.pl", "Ewa", "Dąbrowska", "Media Agency Plus"),
            ("krzysztof.kaminski@software.pl", "Krzysztof", "Kamiński", "Software House XYZ"),
            ("joanna.szymanska@consulting.pl", "Joanna", "Szymańska", "Business Consulting"),
            ("michal.wojcik@tech.pl", "Michał", "Wójcik", "Tech Solutions"),
            ("aleksandra.kowalczyk@design.pl", "Aleksandra", "Kowalczyk", "Design Studio"),
        };

        foreach (var (email, firstName, lastName, company) in directClients)
        {
            var client = Client.CreateDirect(
                tenantId,
                email,
                firstName,
                lastName,
                company
            );
            client.Phone = GeneratePhoneNumber();
            clients.Add(client);
        }

        // Add some inactive clients
        for (int i = 0; i < 3; i++)
        {
            var client = Client.CreateDirect(
                tenantId,
                $"inactive{i + 1}@example.com",
                $"Inactive{i + 1}",
                "User",
                $"Inactive Company {i + 1}"
            );
            client.IsActive = false;
            clients.Add(client);
        }

        context.Clients.AddRange(clients);
        await context.SaveChangesAsync();

        logger.LogInformation("Created {Count} clients ({Active} active, {Inactive} inactive)",
            clients.Count, clients.Count(c => c.IsActive), clients.Count(c => !c.IsActive));

        return clients;
    }

    private static async Task SeedSubscriptionsAsync(
        ApplicationDbContext context,
        TenantId tenantId,
        List<Client> clients,
        List<SubscriptionPlan> plans,
        ILogger logger)
    {
        logger.LogInformation("Seeding subscriptions...");

        var subscriptions = new List<Subscription>();
        var activeClients = clients.Where(c => c.IsActive).ToList();

        // Assign subscriptions to active clients
        foreach (var client in activeClients.Take(10))
        {
            var plan = plans[_random.Next(plans.Count)];

            // Create new instances of value objects to avoid EF Core tracking issues
            var price = Money.Create(plan.Price.Amount, plan.Price.Currency.Code);
            var billingPeriod = BillingPeriod.Create(plan.BillingPeriod.Value, plan.BillingPeriod.Type);

            // Create subscription using factory method
            var subscription = Subscription.Create(
                tenantId,
                client.Id,
                plan.Id,
                price,
                billingPeriod,
                trialDays: plan.TrialDays
            );

            // Calculate backdated dates for realistic test data
            var createdAt = DateTime.UtcNow.AddDays(-_random.Next(1, 365));
            var startDate = createdAt;
            var isInTrial = plan.TrialDays > 0;
            var trialEndDate = isInTrial ? startDate.AddDays(plan.TrialDays) : (DateTime?)null;
            var nextBillingDate = isInTrial
                ? trialEndDate!.Value
                : plan.BillingPeriod.GetNextBillingDate(startDate);

            // Use reflection to set backdated values for seeding
            SetPrivateProperty(subscription, "CreatedAt", createdAt);
            SetPrivateProperty(subscription, "StartDate", startDate);
            SetPrivateProperty(subscription, "NextBillingDate", nextBillingDate);
            if (isInTrial)
            {
                SetPrivateProperty(subscription, "TrialEndDate", trialEndDate);
            }

            // Randomize some statuses
            var statusRoll = _random.Next(100);
            if (statusRoll < 70)
            {
                subscription.Status = SubscriptionStatus.Active;
            }
            else if (statusRoll < 85)
            {
                subscription.Status = SubscriptionStatus.Cancelled;
                subscription.CancelledAt = DateTime.UtcNow.AddDays(-_random.Next(1, 60));
            }
            else if (statusRoll < 95)
            {
                subscription.Status = SubscriptionStatus.Suspended;
            }
            else
            {
                subscription.Status = SubscriptionStatus.Cancelled;
                subscription.CancelledAt = DateTime.UtcNow.AddDays(-_random.Next(1, 30));
            }

            subscriptions.Add(subscription);
        }

        context.Subscriptions.AddRange(subscriptions);
        await context.SaveChangesAsync();

        logger.LogInformation("Created {Count} subscriptions ({Active} active, {Cancelled} cancelled)",
            subscriptions.Count,
            subscriptions.Count(s => s.Status == SubscriptionStatus.Active),
            subscriptions.Count(s => s.Status == SubscriptionStatus.Cancelled));
    }

    private static async Task SeedPaymentsAsync(
        ApplicationDbContext context,
        TenantId tenantId,
        List<Client> clients,
        ILogger logger)
    {
        logger.LogInformation("Seeding payments...");

        var payments = new List<Payment>();
        var clientsWithSubscriptions = await context.Clients
            .Include(c => c.Subscriptions)
            .Where(c => c.Subscriptions.Any())
            .ToListAsync();

        foreach (var client in clientsWithSubscriptions)
        {
            var subscription = client.Subscriptions.First();

            // Create 3-5 historical payments for each subscription
            var paymentCount = _random.Next(3, 6);
            for (int i = 0; i < paymentCount; i++)
            {
                var amount = subscription.CurrentPrice.Amount;
                var currency = subscription.CurrentPrice.Currency.Code;
                var payment = Payment.Create(
                    tenantId,
                    subscription.Id,
                    client.Id,
                    Money.Create(amount, currency)
                );

                // Randomize payment dates using reflection for backdated test data
                var createdAt = DateTime.UtcNow.AddMonths(-i).AddDays(-_random.Next(0, 28));
                SetPrivateProperty(payment, "CreatedAt", createdAt);

                // Randomize status
                var statusRoll = _random.Next(100);
                if (statusRoll < 85)
                {
                    payment.ExternalTransactionId = $"tx_{Guid.NewGuid():N}";
                    payment.MarkAsCompleted();
                }
                else if (statusRoll < 95)
                {
                    payment.MarkAsFailed("Insufficient funds");
                }
                else
                {
                    // Leave as Pending
                }

                payments.Add(payment);
            }
        }

        context.Payments.AddRange(payments);
        await context.SaveChangesAsync();

        logger.LogInformation("Created {Count} payments ({Completed} completed, {Failed} failed, {Pending} pending)",
            payments.Count,
            payments.Count(p => p.Status == PaymentStatus.Completed),
            payments.Count(p => p.Status == PaymentStatus.Failed),
            payments.Count(p => p.Status == PaymentStatus.Pending));
    }

    private static string GeneratePhoneNumber()
    {
        return $"+48 {_random.Next(500, 999)} {_random.Next(100, 999)} {_random.Next(100, 999)}";
    }

    /// <summary>
    /// Helper method to set private properties using reflection (for seeding only)
    /// </summary>
    private static void SetPrivateProperty<T>(T obj, string propertyName, object value)
    {
        var property = typeof(T).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        if (property != null && property.CanWrite)
        {
            property.SetValue(obj, value);
        }
    }
}
