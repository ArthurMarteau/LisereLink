using System.Security.Claims;
using System.Threading.RateLimiting;
using Lisere.Application;
using Lisere.Infrastructure;
using Lisere.Infrastructure.Identity;
using Lisere.API.Middlewares;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration, builder.Environment);

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

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddOpenApi();
}

var app = builder.Build();

await RoleSeeder.SeedRolesAsync(app.Services);

app.UseDefaultFiles();
app.MapStaticAssets();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseCors("LiserePolicy");

app.UseAuthentication();

app.UseRateLimiter();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();

// Nécessaire pour WebApplicationFactory<Program> dans les tests d'intégration
public partial class Program { }
