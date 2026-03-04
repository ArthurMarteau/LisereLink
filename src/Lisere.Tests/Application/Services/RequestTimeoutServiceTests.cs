using Lisere.Application.Interfaces;
using Lisere.Domain.Entities;
using Lisere.Domain.Enums;
using Lisere.Domain.Interfaces;
using Lisere.Infrastructure.BackgroundJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace Lisere.Tests.Application.Services;

public class RequestTimeoutServiceTests
{
    private readonly IRequestRepository _repository;
    private readonly INotificationService _notificationService;
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
        var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();

        _sut = new RequestTimeoutService(scopeFactory, NullLogger<RequestTimeoutService>.Instance);
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
