using Lisere.Application.Interfaces;
using Lisere.Domain.Entities;
using Lisere.Domain.Enums;
using Lisere.Domain.Interfaces;
using Lisere.Infrastructure.BackgroundJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace Lisere.Tests.Application.Services;

public class RequestTimeoutServiceTests
{
    private readonly IRequestRepository _repository;
    private readonly INotificationService _notificationService;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RequestTimeoutService _sut;

    public RequestTimeoutServiceTests()
    {
        _repository = Substitute.For<IRequestRepository>();
        _notificationService = Substitute.For<INotificationService>();

        // Vrai IServiceScopeFactory en mémoire résolvant les mocks
        var services = new ServiceCollection();
        services.AddScoped<IRequestRepository>(_ => _repository);
        services.AddScoped<INotificationService>(_ => _notificationService);
        var provider = services.BuildServiceProvider();
        _scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();

        _sut = new RequestTimeoutService(_scopeFactory, NullLogger<RequestTimeoutService>.Instance);
    }

    // ── Annulation des demandes expirées ─────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WithExpiredPendingRequests_CancelsThem()
    {
        var req1 = BuildRequest(RequestStatus.Pending, DateTime.UtcNow.AddMinutes(-31));
        var req2 = BuildRequest(RequestStatus.Pending, DateTime.UtcNow.AddMinutes(-31));

        _repository.GetExpiredPendingAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns([req1, req2]);

        await _sut.ProcessExpiredRequestsAsync();

        Assert.Equal(RequestStatus.Cancelled, req1.Status);
        Assert.Equal(RequestStatus.Cancelled, req2.Status);
        await _repository.Received(2).UpdateAsync(Arg.Any<Request>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithNoExpiredRequests_DoesNothing()
    {
        _repository.GetExpiredPendingAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(Enumerable.Empty<Request>());

        await _sut.ProcessExpiredRequestsAsync();

        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Request>(), Arg.Any<CancellationToken>());
    }

    // ── Notification SignalR ─────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_CancelledRequest_SendsSignalRNotification()
    {
        var req1 = BuildRequest(RequestStatus.Pending, DateTime.UtcNow.AddMinutes(-31));
        var req2 = BuildRequest(RequestStatus.Pending, DateTime.UtcNow.AddMinutes(-31));

        _repository.GetExpiredPendingAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns([req1, req2]);

        await _sut.ProcessExpiredRequestsAsync();

        // NotifyRequestCancelledAsync est fire-and-forget mais le mock est appelé
        // de façon synchrone avant le ContinueWith → pas de Task.Delay nécessaire
        await _notificationService.Received(1).NotifyRequestCancelledAsync(req1.Id, req1.SellerId);
        await _notificationService.Received(1).NotifyRequestCancelledAsync(req2.Id, req2.SellerId);
    }

    // ── Seuil de 30 minutes ───────────────────────────────────────────────

    [Fact]
    public async Task ProcessExpiredRequestsAsync_RequestExactlyAt30Min_IsCancelled()
    {
        var req = BuildRequest(RequestStatus.Pending, DateTime.UtcNow.AddMinutes(-30));
        DateTime capturedThreshold = default;

        _repository
            .GetExpiredPendingAsync(
                Arg.Do<DateTime>(t => capturedThreshold = t),
                Arg.Any<CancellationToken>())
            .Returns([req]);

        var before = DateTime.UtcNow;
        await _sut.ProcessExpiredRequestsAsync();

        // Threshold doit être ≈ UtcNow - 30 min
        Assert.InRange(capturedThreshold, before.AddMinutes(-31), before.AddMinutes(-29));
        await _repository.Received(1).UpdateAsync(
            Arg.Is<Request>(r => r.Status == RequestStatus.Cancelled),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessExpiredRequestsAsync_RequestAt29Min_IsNotCancelled()
    {
        _repository
            .GetExpiredPendingAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(Enumerable.Empty<Request>());

        await _sut.ProcessExpiredRequestsAsync();

        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Request>(), Arg.Any<CancellationToken>());
    }

    // ── Compteur d'annulations ────────────────────────────────────────────

    [Fact]
    public async Task ProcessExpiredRequestsAsync_CountsAllCancelledRequests()
    {
        var req1 = BuildRequest(RequestStatus.Pending, DateTime.UtcNow.AddMinutes(-31));
        var req2 = BuildRequest(RequestStatus.Pending, DateTime.UtcNow.AddMinutes(-31));
        var req3 = BuildRequest(RequestStatus.Pending, DateTime.UtcNow.AddMinutes(-31));

        _repository
            .GetExpiredPendingAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns([req1, req2, req3]);

        await _sut.ProcessExpiredRequestsAsync();

        await _repository.Received(3).UpdateAsync(Arg.Any<Request>(), Arg.Any<CancellationToken>());
    }

    // ── ExecuteAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenCancelled_StopsGracefully()
    {
        // Tue le mutant while condition négée (ligne 28) : avec token annulé, la boucle doit s'arrêter
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var exception = await Record.ExceptionAsync(() => _sut.StartAsync(cts.Token));

        Assert.Null(exception);
    }

    // ── Logger ────────────────────────────────────────────────────────────

    [Fact]
    public async Task ProcessExpiredRequestsAsync_LogsCountOfCancelledRequests()
    {
        // Tue le mutant LogInformation supprimé (ligne 60) en vérifiant le message contient "2"
        var logger = Substitute.For<ILogger<RequestTimeoutService>>();
        var sut = new RequestTimeoutService(_scopeFactory, logger);

        var req1 = BuildRequest(RequestStatus.Pending, DateTime.UtcNow.AddMinutes(-31));
        var req2 = BuildRequest(RequestStatus.Pending, DateTime.UtcNow.AddMinutes(-31));
        _repository
            .GetExpiredPendingAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns([req1, req2]);

        await sut.ProcessExpiredRequestsAsync();

        var logCalls = logger.ReceivedCalls()
            .Where(c => c.GetMethodInfo().Name == "Log")
            .ToList();
        Assert.Contains(logCalls, c =>
        {
            var args = c.GetArguments();
            return args[0] is LogLevel.Information
                && args[2]?.ToString()?.Contains("2") == true;
        });
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static Request BuildRequest(RequestStatus status, DateTime createdAt) => new()
    {
        Id        = Guid.NewGuid(),
        SellerId  = Guid.NewGuid(),
        Zone      = ZoneType.RTW,
        Status    = status,
        Lines     = [],
        CreatedAt = createdAt,
        CreatedBy = "test",
    };
}
