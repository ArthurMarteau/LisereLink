namespace Lisere.StockApi.Infrastructure.Webhooks;

public class WebhookOptions
{
    public string LisereApiUrl { get; set; } = string.Empty;
    public string Secret { get; set; } = string.Empty;
}
