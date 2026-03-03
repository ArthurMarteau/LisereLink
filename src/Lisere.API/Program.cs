using Lisere.Application.Interfaces;
using Lisere.Application.Services;
using Lisere.Domain.Interfaces;
using Lisere.Infrastructure.ExternalServices;
using Lisere.Infrastructure.Persistence;
using Lisere.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Database
builder.Services.AddDbContext<LisereDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped<IRequestRepository, RequestRepository>();
builder.Services.AddScoped<IRequestLineRepository, RequestLineRepository>();

// External API client (typed HttpClient)
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient<IExternalStockApiClient, ExternalStockApiClient>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["ExternalStockApi:BaseUrl"] ?? "https://localhost:5200");
});

// Redis distributed cache
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

// Application services
builder.Services.AddScoped<IArticleService, ArticleService>();
builder.Services.AddScoped<IRequestService, RequestService>();
builder.Services.AddScoped<IStockService, StockService>();

var app = builder.Build();

app.UseDefaultFiles();
app.MapStaticAssets();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
