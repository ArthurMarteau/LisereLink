using Lisere.Application.DTOs;
using Lisere.Application.Exceptions;
using Xunit;
using Lisere.Application.Interfaces;
using Lisere.Application.Services;
using Lisere.Domain.Entities;
using Lisere.Domain.Enums;
using Lisere.Domain.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Lisere.Tests.Application.Services;

public class RequestServiceTests
{
    private readonly IRequestRepository _repository;
    private readonly IStockService _stockService;
    private readonly INotificationService _notificationService;
    private readonly RequestService _sut;

    public RequestServiceTests()
    {
        _repository = Substitute.For<IRequestRepository>();
        _stockService = Substitute.For<IStockService>();
        _notificationService = Substitute.For<INotificationService>();
        _sut = new RequestService(
            _repository,
            _stockService,
            _notificationService,
            NullLogger<RequestService>.Instance);
    }

    // -------------------------------------------------------------------------
    // CreateAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task CreateAsync_WithAvailableStock_AddsToRepositoryAndReturnsDto()
    {
        var articleId = Guid.NewGuid();
        var dto = BuildCreateRequestDto(articleId, "M");

        _stockService.IsAvailableAsync(articleId, "M", Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _sut.CreateAsync(dto);

        await _repository.Received(1).AddAsync(Arg.Any<Request>(), Arg.Any<CancellationToken>());
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(dto.SellerId, result.SellerId);
    }

    [Fact]
    public async Task CreateAsync_WithAvailableStock_LinesHaveCorrectRequestId()
    {
        var articleId = Guid.NewGuid();
        var dto = BuildCreateRequestDto(articleId, "M");

        _stockService.IsAvailableAsync(articleId, "M", Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(true);

        await _sut.CreateAsync(dto);

        await _repository.Received(1).AddAsync(
            Arg.Is<Request>(r =>
                r.Lines.All(l =>
                    l.RequestId == r.Id &&
                    l.CreatedAt != default &&
                    l.CreatedBy == dto.SellerId.ToString())),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_WithUnavailableStock_ThrowsBusinessException()
    {
        var articleId = Guid.NewGuid();
        var dto = BuildCreateRequestDto(articleId, "M");

        _stockService.IsAvailableAsync(articleId, "M", Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);

        await Assert.ThrowsAsync<BusinessException>(() => _sut.CreateAsync(dto));
        await _repository.DidNotReceive().AddAsync(Arg.Any<Request>(), Arg.Any<CancellationToken>());
    }

    // -------------------------------------------------------------------------
    // GetAllAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetAllAsync_ReturnsMappedPagedResult()
    {
        var requests = new List<Request>
        {
            BuildRequest(RequestStatus.Pending),
            BuildRequest(RequestStatus.InProgress),
        };
        _repository.GetAllAsync(1, 20, Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns((requests.AsEnumerable(), 2));

        var result = await _sut.GetAllAsync(1, 20);

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count());
        Assert.Equal(1, result.Page);
        Assert.Equal(20, result.PageSize);
    }

    [Fact]
    public async Task GetAllAsync_CapsPageSizeAt50()
    {
        _repository.GetAllAsync(1, 50, Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns((Enumerable.Empty<Request>(), 0));

        await _sut.GetAllAsync(1, 999);

        await _repository.Received(1).GetAllAsync(1, 50, Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAllAsync_WithPageSizeBelowMax_PreservesOriginalPageSize()
    {
        _repository.GetAllAsync(1, 20, Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns((Enumerable.Empty<Request>(), 0));

        await _sut.GetAllAsync(1, 20);

        await _repository.Received(1).GetAllAsync(1, 20, Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    // -------------------------------------------------------------------------
    // CancelAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task CancelAsync_ExistingRequest_CallsDeleteOnRepository()
    {
        var request = BuildRequest(RequestStatus.Pending);
        _repository.GetByIdAsync(request.Id, Arg.Any<CancellationToken>())
            .Returns(request);

        await _sut.CancelAsync(request.Id);

        await _repository.Received(1).DeleteAsync(request.Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CancelAsync_NonExistingRequest_ThrowsKeyNotFoundException()
    {
        var id = Guid.NewGuid();
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>())
            .Returns((Request?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.CancelAsync(id));
        await _repository.DidNotReceive().DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    // -------------------------------------------------------------------------
    // UpdateAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task UpdateAsync_WithPendingStatus_UpdatesAndReturnsDto()
    {
        var request = BuildRequest(RequestStatus.Pending);
        _repository.GetByIdAsync(request.Id, Arg.Any<CancellationToken>())
            .Returns(request);

        var dto = new UpdateRequestDto { Status = RequestStatus.InProgress };

        var result = await _sut.UpdateAsync(request.Id, dto);

        await _repository.Received(1).UpdateAsync(request, Arg.Any<CancellationToken>());
        Assert.Equal(nameof(RequestStatus.InProgress), result.Status);
    }

    [Fact]
    public async Task UpdateAsync_WithNonPendingStatus_ThrowsBusinessException()
    {
        var request = BuildRequest(RequestStatus.InProgress);
        _repository.GetByIdAsync(request.Id, Arg.Any<CancellationToken>())
            .Returns(request);

        var dto = new UpdateRequestDto { Status = RequestStatus.Delivered };

        await Assert.ThrowsAsync<BusinessException>(() => _sut.UpdateAsync(request.Id, dto));
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Request>(), Arg.Any<CancellationToken>());
    }

    // -------------------------------------------------------------------------
    // GetByIdAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetByIdAsync_WhenFound_ReturnsMappedDto()
    {
        var request = BuildRequest(RequestStatus.Pending);
        _repository.GetByIdAsync(request.Id, Arg.Any<CancellationToken>())
            .Returns(request);

        var result = await _sut.GetByIdAsync(request.Id);

        Assert.NotNull(result);
        Assert.Equal(request.Id, result.Id);
        Assert.Equal(request.SellerId, result.SellerId);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNull()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Request?)null);

        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    // -------------------------------------------------------------------------
    // Notification fire-and-forget — ne propage pas les exceptions
    // -------------------------------------------------------------------------

    [Fact]
    public async Task CreateAsync_WhenNotificationFails_DoesNotThrow()
    {
        var articleId = Guid.NewGuid();
        var dto = BuildCreateRequestDto(articleId, "M");

        _stockService.IsAvailableAsync(articleId, "M", Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _notificationService
            .NotifyNewRequestAsync(Arg.Any<RequestDto>())
            .Returns(Task.FromException(new Exception("SignalR down")));

        var exception = await Record.ExceptionAsync(() => _sut.CreateAsync(dto));

        await Task.Delay(50);
        Assert.Null(exception);
    }

    [Fact]
    public async Task UpdateAsync_WhenNotificationFails_DoesNotThrow()
    {
        var request = BuildRequest(RequestStatus.Pending);
        _repository.GetByIdAsync(request.Id, Arg.Any<CancellationToken>())
            .Returns(request);
        _notificationService
            .NotifyRequestUpdatedAsync(Arg.Any<RequestDto>())
            .Returns(Task.FromException(new Exception("SignalR down")));

        var dto = new UpdateRequestDto { Status = RequestStatus.InProgress };

        var exception = await Record.ExceptionAsync(() => _sut.UpdateAsync(request.Id, dto));

        await Task.Delay(50);
        Assert.Null(exception);
    }

    [Fact]
    public async Task CancelAsync_WhenNotificationFails_DoesNotThrow()
    {
        var request = BuildRequest(RequestStatus.Pending);
        _repository.GetByIdAsync(request.Id, Arg.Any<CancellationToken>())
            .Returns(request);
        _notificationService
            .NotifyRequestCancelledAsync(Arg.Any<Guid>(), Arg.Any<Guid>())
            .Returns(Task.FromException(new Exception("SignalR down")));

        var exception = await Record.ExceptionAsync(() => _sut.CancelAsync(request.Id));

        await Task.Delay(50);
        Assert.Null(exception);
    }

    [Fact]
    public async Task CreateAsync_PassesDtoStoreIdToStockService()
    {
        var articleId = Guid.NewGuid();
        const string storeId = "002";
        var dto = BuildCreateRequestDto(articleId, "M", storeId);

        _stockService.IsAvailableAsync(articleId, "M", Arg.Is<string>(s => s == storeId), Arg.Any<CancellationToken>())
            .Returns(true);

        await _sut.CreateAsync(dto);

        await _stockService.Received(1).IsAvailableAsync(
            articleId,
            "M",
            Arg.Is<string>(s => s == storeId),
            Arg.Any<CancellationToken>());
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static CreateRequestDto BuildCreateRequestDto(Guid articleId, string size, string storeId = "001") => new()
    {
        SellerId = Guid.NewGuid(),
        StoreId = storeId,
        Zone = ZoneType.RTW,
        Lines =
        [
            new CreateRequestLineDto
            {
                ArticleId           = articleId,
                ArticleName         = "Manteau Test",
                ArticleColorOrPrint = "Noir",
                ArticleBarcode      = "1234567890123",
                RequestedSizes      = [size],
                Quantity            = 1,
            }
        ]
    };

    private static Request BuildRequest(RequestStatus status) => new()
    {
        Id       = Guid.NewGuid(),
        SellerId = Guid.NewGuid(),
        Zone     = ZoneType.RTW,
        Status   = status,
        Lines    = [],
        CreatedAt = DateTime.UtcNow,
        CreatedBy = "test",
    };
}
