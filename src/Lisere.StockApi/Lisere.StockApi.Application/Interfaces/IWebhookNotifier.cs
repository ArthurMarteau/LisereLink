namespace Lisere.StockApi.Application.Interfaces;

public interface IWebhookNotifier
{
    Task NotifyStockUpdatedAsync(Guid articleId, string storeId);
}
