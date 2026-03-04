using Lisere.Application.Interfaces;
using Lisere.Domain.Enums;
using Lisere.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Lisere.Infrastructure.BackgroundJobs;

public class RequestTimeoutService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan Timeout = TimeSpan.FromMinutes(30);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RequestTimeoutService> _logger;

    public RequestTimeoutService(IServiceScopeFactory scopeFactory, ILogger<RequestTimeoutService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await ProcessExpiredRequestsAsync(stoppingToken);
        }
    }

    internal async Task ProcessExpiredRequestsAsync(CancellationToken cancellationToken = default)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRequestRepository>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        var threshold = DateTime.UtcNow - Timeout;
        var expired = await repository.GetExpiredPendingAsync(threshold, cancellationToken);

        var cancelled = 0;
        foreach (var request in expired)
        {
            request.Status = RequestStatus.Cancelled;
            request.ModifiedAt = DateTime.UtcNow;

            await repository.UpdateAsync(request, cancellationToken);

            _ = notificationService.NotifyRequestCancelledAsync(request.Id, request.SellerId)
                .ContinueWith(
                    t => _logger.LogWarning(t.Exception,
                        "Échec de la notification SignalR (timeout requestId={RequestId}).", request.Id),
                    TaskContinuationOptions.OnlyOnFaulted);

            cancelled++;
        }

        _logger.LogInformation(
            "RequestTimeoutService : {Count} demande(s) annulée(s) pour dépassement de délai.", cancelled);
    }
}
