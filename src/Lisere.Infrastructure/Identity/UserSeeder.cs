using Lisere.Domain.Entities;
using Lisere.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lisere.Infrastructure.Identity;

public static class UserSeeder
{
    private record SeedAccount(string Email, string Password, UserRole Role, string FirstName, string LastName);

    private static readonly SeedAccount[] Accounts =
    [
        new("admin@lisere.fr",     "Admin123!",     UserRole.Admin,    "Admin",  "Lisere"),
        new("vendeur@lisere.fr",   "Vendeur123!",   UserRole.Seller,   "Marie",  "Dupont"),
        new("stockiste@lisere.fr", "Stockiste123!", UserRole.Stockist, "Thomas", "Martin"),
    ];

    public static async Task SeedUsersAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(UserSeeder));

        foreach (var account in Accounts)
        {
            try
            {
                if (await userManager.FindByEmailAsync(account.Email) != null)
                {
                    logger.LogInformation("User {Email} skipped (already exists)", account.Email);
                    continue;
                }

                var user = new User
                {
                    UserName = account.Email,
                    Email = account.Email,
                    EmailConfirmed = true,
                    FirstName = account.FirstName,
                    LastName = account.LastName,
                    Role = account.Role,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "Seeder",
                };

                var createResult = await userManager.CreateAsync(user, account.Password);
                if (!createResult.Succeeded)
                {
                    logger.LogError("Failed to create user {Email}: {Errors}",
                        account.Email, string.Join(", ", createResult.Errors.Select(e => e.Description)));
                    continue;
                }

                var roleResult = await userManager.AddToRoleAsync(user, account.Role.ToString());
                if (!roleResult.Succeeded)
                {
                    logger.LogError("Failed to assign role {Role} to {Email}: {Errors}",
                        account.Role, account.Email, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                    continue;
                }

                logger.LogInformation("User {Email} created with role {Role}", account.Email, account.Role);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error seeding user {Email}", account.Email);
            }
        }
    }
}
