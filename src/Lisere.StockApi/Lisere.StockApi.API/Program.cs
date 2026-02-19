using System.Text;
using Lisere.StockApi.Application.Interfaces;
using Lisere.StockApi.Application.Services;
using Lisere.StockApi.Domain.Interfaces;
using Lisere.StockApi.Infrastructure.Persistence;
using Lisere.StockApi.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

// Utilise les interfaces du StockApi.Domain (pas Lisere.Domain.Interfaces)
using IArticleRepo = Lisere.StockApi.Domain.Interfaces.IArticleRepository;
using ArticleRepo = Lisere.StockApi.Infrastructure.Persistence.Repositories.ArticleRepository;

var builder = WebApplication.CreateBuilder(args);

// EF Core — base de données séparée : LisereStockApi
builder.Services.AddDbContext<StockApiDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("StockApiDb")));

// Repositories
builder.Services.AddScoped<IArticleRepo, ArticleRepo>();
builder.Services.AddScoped<IStockEntryRepository, StockEntryRepository>();
builder.Services.AddScoped<IStoreRepository, StoreRepository>();

// Services
builder.Services.AddScoped<IStockService, StockService>();

// JWT validation uniquement (pas d'ASP.NET Identity)
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
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
