using Lisere.Application.Interfaces;
using Lisere.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Lisere.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IArticleService, ArticleService>();
        services.AddScoped<IRequestService, RequestService>();

        return services;
    }
}
