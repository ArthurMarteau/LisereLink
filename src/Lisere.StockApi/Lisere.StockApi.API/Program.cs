using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using Lisere.StockApi.API.Data;
using Lisere.StockApi.API.Middlewares;
using Lisere.StockApi.Application.Interfaces;
using Lisere.StockApi.Application.Services;
using Lisere.StockApi.Domain.Interfaces;
using Lisere.StockApi.Infrastructure.Persistence;
using Lisere.StockApi.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

// Aliases pour lever l'ambiguïté avec Lisere.Domain.Interfaces
using IArticleRepo = Lisere.StockApi.Domain.Interfaces.IArticleRepository;
using ArticleRepo = Lisere.StockApi.Infrastructure.Persistence.Repositories.ArticleRepository;

var builder = WebApplication.CreateBuilder(args);

// ── EF Core — base de données séparée : LisereStockApi ──────────────────────
// En environnement "Test", le WebApplicationFactory injecte sa propre base InMemory
if (!builder.Environment.IsEnvironment("Test"))
{
    builder.Services.AddDbContext<StockApiDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("StockApiConnection")));
}

// ── Repositories ─────────────────────────────────────────────────────────────
builder.Services.AddScoped<IArticleRepo, ArticleRepo>();
builder.Services.AddScoped<IStockEntryRepository, StockEntryRepository>();
builder.Services.AddScoped<IStoreRepository, StoreRepository>();

// ── Services ──────────────────────────────────────────────────────────────────
builder.Services.AddScoped<IStockService, StockService>();

// ── JWT validation uniquement (pas d'ASP.NET Identity) ───────────────────────
var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("Jwt:Secret manquant dans la configuration.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// ── CORS ──────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("LiserePolicy", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        }
        else
        {
            var allowedOrigins = builder.Configuration
                .GetSection("Cors:AllowedOrigins")
                .Get<string[]>() ?? [];
            policy.WithOrigins(allowedOrigins).AllowAnyMethod().AllowAnyHeader();
        }
    });
});

// ── Rate Limiting ─────────────────────────────────────────────────────────────
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("fixed", httpContext =>
    {
        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? httpContext.User.FindFirst("sub")?.Value;
        var key = userId ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous";

        return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 100,
            Window = TimeSpan.FromMinutes(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0,
        });
    });
});

builder.Services.AddControllers();
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddOpenApi();
}

var app = builder.Build();

// ── Seed données de développement ────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<StockApiDbContext>();
    await DataSeeder.SeedAsync(db);

    app.MapOpenApi();
}

// ── Pipeline ──────────────────────────────────────────────────────────────────
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseCors("LiserePolicy");

app.UseAuthentication();

app.UseRateLimiter();

app.UseAuthorization();
app.MapControllers();

app.Run();

// Nécessaire pour WebApplicationFactory<Program> dans les tests d'intégration
public partial class Program { }
