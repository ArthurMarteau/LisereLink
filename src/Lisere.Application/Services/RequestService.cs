using Lisere.Application.Common;
using Lisere.Application.DTOs;
using Lisere.Application.Exceptions;
using Lisere.Application.Interfaces;
using Lisere.Application.Mapping;
using Lisere.Domain.Enums;
using Lisere.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Lisere.Application.Services;

public class RequestService : IRequestService
{
    private readonly IRequestRepository _requestRepository;
    private readonly IStockService _stockService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<RequestService> _logger;

    public RequestService(
        IRequestRepository requestRepository,
        IStockService stockService,
        INotificationService notificationService,
        ILogger<RequestService> logger)
    {
        _requestRepository = requestRepository;
        _stockService = stockService;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<RequestDto> CreateAsync(CreateRequestDto dto, CancellationToken cancellationToken = default)
    {
        foreach (var line in dto.Lines)
        {
            foreach (var size in line.RequestedSizes)
            {
                var isAvailable = await _stockService.IsAvailableAsync(line.ArticleId, size, dto.StoreId, cancellationToken);
                if (!isAvailable)
                    throw new BusinessException(
                        $"Stock insuffisant pour l'article {line.ArticleId} en taille {size}.");
            }
        }

        var request = dto.ToEntity();
        request.CreatedAt = DateTime.UtcNow;
        request.CreatedBy = dto.SellerId.ToString();

        foreach (var line in request.Lines)
        {
            line.RequestId = request.Id;
            line.CreatedAt = DateTime.UtcNow;
            line.CreatedBy = dto.SellerId.ToString();
        }

        await _requestRepository.AddAsync(request, cancellationToken);
        var requestDto = request.ToDto();

        _ = _notificationService.NotifyNewRequestAsync(requestDto).ContinueWith(
            t => _logger.LogWarning(t.Exception, "Échec de la notification SignalR (NewRequest)."),
            TaskContinuationOptions.OnlyOnFaulted);

        return requestDto;
    }

    public async Task<PagedResult<RequestDto>> GetAllAsync(
        int page,
        int pageSize,
        string? storeId = null,
        string? zone = null,
        CancellationToken cancellationToken = default)
    {
        pageSize = Math.Min(pageSize, 50);
        page = Math.Max(page, 1);

        var (items, totalCount) = await _requestRepository.GetAllAsync(page, pageSize, storeId, zone, cancellationToken);

        return new PagedResult<RequestDto>
        {
            Items = items.Select(r => r.ToDto()),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        };
    }

    public async Task<RequestDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var request = await _requestRepository.GetByIdAsync(id, cancellationToken);
        return request?.ToDto();
    }

    public async Task<RequestDto> UpdateAsync(
        Guid id,
        UpdateRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        var request = await _requestRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Demande {id} introuvable.");

        if (request.Status != RequestStatus.Pending)
            throw new BusinessException("La demande ne peut être modifiée que si son statut est En attente.");

        request.Status = dto.Status;
        request.StockistId = dto.StockistId;
        request.ModifiedAt = DateTime.UtcNow;

        await _requestRepository.UpdateAsync(request, cancellationToken);
        var requestDto = request.ToDto();

        _ = _notificationService.NotifyRequestUpdatedAsync(requestDto).ContinueWith(
            t => _logger.LogWarning(t.Exception, "Échec de la notification SignalR (RequestUpdated)."),
            TaskContinuationOptions.OnlyOnFaulted);

        return requestDto;
    }

    public async Task CancelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var request = await _requestRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Demande {id} introuvable.");

        var sellerId = request.SellerId;

        await _requestRepository.DeleteAsync(id, cancellationToken);

        _ = _notificationService.NotifyRequestCancelledAsync(id, sellerId).ContinueWith(
            t => _logger.LogWarning(t.Exception, "Échec de la notification SignalR (RequestCancelled)."),
            TaskContinuationOptions.OnlyOnFaulted);
    }

    public async Task<RequestDto> AcceptAlternativeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var request = await _requestRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Demande {id} introuvable.");

        if (request.Status != RequestStatus.AwaitingSellerResponse)
            throw new BusinessException("La demande doit être en attente de réponse vendeur pour accepter une alternative.");

        request.Status = RequestStatus.InProgress;
        request.ModifiedAt = DateTime.UtcNow;

        await _requestRepository.UpdateAsync(request, cancellationToken);
        var requestDto = request.ToDto();

        _ = _notificationService.NotifyRequestUpdatedAsync(requestDto).ContinueWith(
            t => _logger.LogWarning(t.Exception, "Échec de la notification SignalR (AcceptAlternative)."),
            TaskContinuationOptions.OnlyOnFaulted);

        return requestDto;
    }

    public async Task<RequestDto> RejectAlternativeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var request = await _requestRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Demande {id} introuvable.");

        if (request.Status != RequestStatus.AwaitingSellerResponse)
            throw new BusinessException("La demande doit être en attente de réponse vendeur pour rejeter une alternative.");

        request.Status = RequestStatus.Unavailable;
        request.ModifiedAt = DateTime.UtcNow;

        await _requestRepository.UpdateAsync(request, cancellationToken);
        var requestDto = request.ToDto();

        _ = _notificationService.NotifyRequestUpdatedAsync(requestDto).ContinueWith(
            t => _logger.LogWarning(t.Exception, "Échec de la notification SignalR (RejectAlternative)."),
            TaskContinuationOptions.OnlyOnFaulted);

        return requestDto;
    }
}
