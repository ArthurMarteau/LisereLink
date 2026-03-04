using System.Text;
using Lisere.Application.Interfaces;
using Lisere.Domain.Entities;
using Lisere.Domain.Interfaces;
using Lisere.Infrastructure.ExternalServices;
using Lisere.Infrastructure.Identity;
using Lisere.Infrastructure.Persistence;
using Lisere.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace Lisere.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment env)
    {
        // Database — skipped en environnement "Test" : WebApplicationFactory injecte sa propre base InMemory
        if (!env.IsEnvironment("Test"))
        {
            services.AddDbContext<LisereDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
        }

        // Repositories
        services.AddScoped<IRequestRepository, RequestRepository>();
        services.AddScoped<IRequestLineRepository, RequestLineRepository>();

        // External API client (typed HttpClient)
        services.AddHttpContextAccessor();
        services.AddHttpClient<IExternalStockApiClient, ExternalStockApiClient>(client =>
        {
            client.BaseAddress = new Uri(
                configuration["ExternalStockApi:BaseUrl"] ?? "https://localhost:5200");
        });

        // Redis distributed cache
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
        });

        // Stock service
        services.AddScoped<IStockService, StockService>();

        // Auth service
        services.AddScoped<IAuthService, AuthService>();

        // Identity
        services.AddIdentity<User, IdentityRole<Guid>>()
            .AddEntityFrameworkStores<LisereDbContext>()
            .AddDefaultTokenProviders();

        // JWT Authentication — overrides Identity's default cookie scheme
        var jwtSecret = configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("Jwt:Secret est requis.");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidAudience = configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSecret))
            };
        });

        return services;
    }
}
