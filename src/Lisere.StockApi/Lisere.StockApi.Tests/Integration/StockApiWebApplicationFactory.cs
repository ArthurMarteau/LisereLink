using System.Text;
using Lisere.StockApi.Domain.Entities;
using Lisere.StockApi.Domain.Enums;
using Lisere.StockApi.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Lisere.StockApi.Tests.Integration;

/// <summary>
/// Factory partagée entre toutes les classes de tests d'intégration.
/// Remplace SQL Server par InMemory et seed des données déterministes.
/// </summary>
public class StockApiWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    // ── Identifiants déterministes pour les tests ────────────────────────────
    public static readonly Guid TestArticleId = new("a1b2c3d4-e5f6-7890-abcd-ef1234567890");
    public const string TestBarcode = "3400936123450";
    public const string TestStoreId = "paris-opera";

    // ── JWT ──────────────────────────────────────────────────────────────────
    public const string JwtSecret = "test-secret-key-must-be-at-least-32-chars-long!!";
    public const string JwtIssuer = "lisere-api";
    public const string JwtAudience = "lisere-services";

    // Base de données in-memory isolée par instance de factory
    private readonly string _dbName = $"StockApiTest_{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Environnement "Test" : empêche DataSeeder et OpenAPI d'être enregistrés
        builder.UseEnvironment("Test");

        builder.ConfigureServices(services =>
        {
            // Program.cs ne s'enregistre pas en env "Test" → on ajoute directement InMemory
            services.AddDbContext<StockApiDbContext>(options =>
                options.UseInMemoryDatabase(_dbName));

            // PostConfigure garantit que les paramètres JWT de test priment
            // sur ceux lus dans appsettings au démarrage de Program.cs
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
                    ClockSkew = TimeSpan.Zero
                };
            });
        });
    }

    // ── IAsyncLifetime ───────────────────────────────────────────────────────

    async Task IAsyncLifetime.InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StockApiDbContext>();
        await db.Database.EnsureCreatedAsync();
        await SeedAsync(db);
    }

    Task IAsyncLifetime.DisposeAsync() => Task.CompletedTask;

    // ── Données de test ──────────────────────────────────────────────────────

    private static async Task SeedAsync(StockApiDbContext db)
    {
        if (await db.Articles.AnyAsync()) return;

        var now = DateTime.UtcNow;

        var article = new Article
        {
            Id = TestArticleId,
            Barcode = TestBarcode,
            Family = ClothingFamily.DRE,
            Name = "Robe Rouge",
            ColorOrPrint = "Rouge",
            AvailableSizes = [Size.S, Size.M, Size.L],
            LastUpdatedAt = now
        };
        await db.Articles.AddAsync(article);

        var store = new Store
        {
            Id = Guid.NewGuid(),
            Code = TestStoreId,
            Name = "Paris Opéra",
            Type = StoreType.Physical
        };
        await db.Stores.AddAsync(store);

        var entries = new[]
        {
            new StockEntry { Id = Guid.NewGuid(), ArticleId = TestArticleId, Size = Size.S, AvailableQuantity = 3, StoreId = TestStoreId, StoreType = StoreType.Physical, LastUpdatedAt = now },
            new StockEntry { Id = Guid.NewGuid(), ArticleId = TestArticleId, Size = Size.M, AvailableQuantity = 0, StoreId = TestStoreId, StoreType = StoreType.Physical, LastUpdatedAt = now },
            new StockEntry { Id = Guid.NewGuid(), ArticleId = TestArticleId, Size = Size.L, AvailableQuantity = 2, StoreId = TestStoreId, StoreType = StoreType.Physical, LastUpdatedAt = now },
        };
        await db.StockEntries.AddRangeAsync(entries);

        await db.SaveChangesAsync();
    }
}
