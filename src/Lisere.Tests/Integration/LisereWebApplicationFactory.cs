using System.Net;
using System.Text;
using Lisere.Infrastructure.Identity;
using Lisere.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using StackExchange.Redis;
using Xunit;

namespace Lisere.Tests.Integration;

/// <summary>
/// Factory partagée pour les tests d'intégration de Lisere.API.
/// En environnement "Test", AddInfrastructureServices saute l'enregistrement SQL Server.
/// La factory injecte InMemory à la place, ainsi qu'un cache mémoire (pas de Redis en CI).
/// IConnectionMultiplexer est remplacé par un mock NSubstitute.
/// </summary>
public class LisereWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    // ── JWT — cohérent avec appsettings.Test.json et StockApi.Tests ──────────
    public const string JwtSecret = "test-secret-key-must-be-at-least-32-chars-long!!";
    public const string JwtIssuer = "lisere-api";
    public const string JwtAudience = "lisere-services";

    // ── Webhook secret utilisé dans les tests d'intégration ──────────────────
    public const string WebhookSecret = "test-webhook-secret";

    // Base de données in-memory isolée par instance de factory
    private readonly string _dbName = $"LisereTest_{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        // Injecte le secret webhook dans la configuration de test
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Webhooks:Secret"] = WebhookSecret
            });
        });

        builder.ConfigureServices(services =>
        {
            // SQL Server n'est pas enregistré en env "Test" → on injecte directement InMemory
            services.AddDbContext<LisereDbContext>(options =>
                options.UseInMemoryDatabase(_dbName));

            // Remplace Redis par un cache mémoire (pas de Redis en CI)
            foreach (var d in services.Where(d => d.ServiceType == typeof(IDistributedCache)).ToList())
                services.Remove(d);
            services.AddDistributedMemoryCache();

            // Remplace IConnectionMultiplexer par un mock no-op (pas de Redis en CI)
            foreach (var d in services.Where(d => d.ServiceType == typeof(IConnectionMultiplexer)).ToList())
                services.Remove(d);

            var mockServer = Substitute.For<IServer>();
            mockServer.Keys(
                    Arg.Any<int>(), Arg.Any<RedisValue>(), Arg.Any<int>(),
                    Arg.Any<long>(), Arg.Any<int>(), Arg.Any<CommandFlags>())
                .Returns(Array.Empty<RedisKey>());

            var mockMultiplexer = Substitute.For<IConnectionMultiplexer>();
            mockMultiplexer.GetEndPoints(Arg.Any<bool>())
                .Returns(new EndPoint[] { new IPEndPoint(IPAddress.Loopback, 6379) });
            mockMultiplexer.GetServer(Arg.Any<EndPoint>(), Arg.Any<object?>())
                .Returns(mockServer);

            services.AddSingleton(mockMultiplexer);

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
