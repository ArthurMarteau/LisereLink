using Lisere.StockApi.Application.Interfaces;
using Lisere.StockApi.Application.Services;
using Lisere.StockApi.Domain.Interfaces;
using NSubstitute;

namespace Lisere.StockApi.Tests.Unit;

public abstract class StockServiceTestBase
{
    protected readonly IStockEntryRepository StockEntryRepo;
    protected readonly IStoreRepository StoreRepo;
    protected readonly IArticleRepository ArticleRepo;
    protected readonly IWebhookNotifier WebhookNotifier;
    protected readonly StockService Service;

    protected StockServiceTestBase()
    {
        StockEntryRepo  = Substitute.For<IStockEntryRepository>();
        StoreRepo       = Substitute.For<IStoreRepository>();
        ArticleRepo     = Substitute.For<IArticleRepository>();
        WebhookNotifier = Substitute.For<IWebhookNotifier>();
        Service = new StockService(StockEntryRepo, StoreRepo, ArticleRepo, WebhookNotifier);
    }
}
