using Lisere.Application.Interfaces;
using Lisere.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Lisere.Infrastructure.ExternalServices;

public class ArticleSyncService : BackgroundService, IArticleSyncService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ArticleSyncService> _logger;

    public ArticleSyncService(IServiceScopeFactory scopeFactory, ILogger<ArticleSyncService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task SyncAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Début de la synchronisation des articles depuis le StockApi.");

        using var scope = _scopeFactory.CreateScope();
        var apiClient = scope.ServiceProvider.GetRequiredService<IExternalStockApiClient>();
        var articleRepo = scope.ServiceProvider.GetRequiredService<ILocalArticleRepository>();

        IEnumerable<Domain.Entities.Article> articles;
        try
        {
            articles = await apiClient.GetArticlesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Impossible de récupérer les articles depuis le StockApi.");
            return;
        }

        var synced = 0;
        foreach (var incoming in articles)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var existing = await articleRepo.GetByBarcodeAsync(incoming.Barcode, cancellationToken);
                if (existing is null)
                {
                    incoming.Id = Guid.NewGuid();
                    incoming.CreatedAt = DateTime.UtcNow;
                    incoming.CreatedBy = "sync";
                    await articleRepo.AddAsync(incoming, cancellationToken);
                }
                else
                {
                    existing.Name = incoming.Name;
                    existing.Family = incoming.Family;
                    existing.ColorOrPrint = incoming.ColorOrPrint;
                    existing.AvailableSizes = incoming.AvailableSizes;
                    existing.Price = incoming.Price;
                    existing.ImageUrl = incoming.ImageUrl;
                    existing.ModifiedAt = DateTime.UtcNow;
                    existing.ModifiedBy = "sync";
                    await articleRepo.UpdateAsync(existing, cancellationToken);
                }
                synced++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erreur lors de la synchronisation de l'article {Barcode}.", incoming.Barcode);
            }
        }

        _logger.LogInformation("Synchronisation terminée : {Count} article(s) traité(s).", synced);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await SyncAsync(stoppingToken);

        using var timer = new PeriodicTimer(TimeSpan.FromHours(1));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await SyncAsync(stoppingToken);
        }
    }
}
