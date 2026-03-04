using System.Text;
using Lisere.Infrastructure.Identity;
using Lisere.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Lisere.Tests.Integration;

/// <summary>
/// Factory partagée pour les tests d'intégration de Lisere.API.
/// En environnement "Test", AddInfrastructureServices saute l'enregistrement SQL Server.
/// La factory injecte InMemory à la place, ainsi qu'un cache mémoire (pas de Redis en CI).
/// </summary>
public class LisereWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    // ── JWT — cohérent avec appsettings.Test.json et StockApi.Tests ──────────
    public const string JwtSecret = "test-secret-key-must-be-at-least-32-chars-long!!";
    public const string JwtIssuer = "lisere-api";
    public const string JwtAudience = "lisere-services";

    // Base de données in-memory isolée par instance de factory
    private readonly string _dbName = $"LisereTest_{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureServices(services =>
        {
            // SQL Server n'est pas enregistré en env "Test" → on injecte directement InMemory
            services.AddDbContext<LisereDbContext>(options =>
                options.UseInMemoryDatabase(_dbName));

            // Remplace Redis par un cache mémoire (pas de Redis en CI)
            foreach (var d in services.Where(d => d.ServiceType == typeof(IDistributedCache)).ToList())
                services.Remove(d);
            services.AddDistributedMemoryCache();

            // PostConfigure garantit que les paramètres JWT de test priment
            // sur ceux lus dans appsettings.Test.json au démarrage de Program.cs
            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecret)),
                    ValidateIssuer = true,
                    ValidIssuer = JwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = JwtAudience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                };
            });
        });
    }

    // ── IAsyncLifetime ───────────────────────────────────────────────────────

    async Task IAsyncLifetime.InitializeAsync()
    {
        // Déclenche la création du host (et donc Program.cs + RoleSeeder)
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LisereDbContext>();
        await db.Database.EnsureCreatedAsync();

        // Seed les rôles Identity (idempotent — vérifie l'existence avant création)
        await RoleSeeder.SeedRolesAsync(Services);
    }

    Task IAsyncLifetime.DisposeAsync() => Task.CompletedTask;
}
