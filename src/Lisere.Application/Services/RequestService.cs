using Lisere.Application.Common;
using Lisere.Application.DTOs;
using Lisere.Application.Exceptions;
using Lisere.Application.Interfaces;
using Lisere.Application.Mapping;
using Lisere.Domain.Entities;
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

        if (request.Status != RequestStatus.Pending)
            throw new BusinessException("Seules les demandes en attente peuvent être annulées.");

        request.Status = RequestStatus.Cancelled;
        request.CancelledAt = DateTime.UtcNow;
        request.ModifiedAt = DateTime.UtcNow;

        await _requestRepository.UpdateAsync(request, cancellationToken);
        var requestDto = request.ToDto();

        _ = _notificationService.NotifyRequestUpdatedAsync(requestDto).ContinueWith(
            t => _logger.LogWarning(t.Exception, "Échec de la notification SignalR (CancelRequest)."),
            TaskContinuationOptions.OnlyOnFaulted);
    }

    public async Task<RequestDto> TakeInProgressAsync(
        Guid requestId,
        Guid stockistId,
        CancellationToken cancellationToken = default)
    {
        var request = await _requestRepository.GetByIdAsync(requestId, cancellationToken)
            ?? throw new KeyNotFoundException($"Demande {requestId} introuvable.");

        if (request.Status != RequestStatus.Pending)
            throw new BusinessException("Seules les demandes en attente peuvent être prises en charge.");

        request.StockistId = stockistId;
        request.Status = RequestStatus.InProgress;
        request.ModifiedAt = DateTime.UtcNow;
        request.ModifiedBy = stockistId.ToString();

        await _requestRepository.UpdateAsync(request, cancellationToken);
        var requestDto = request.ToDto();

        _ = _notificationService.NotifyRequestUpdatedAsync(requestDto).ContinueWith(
            t => _logger.LogWarning(t.Exception, "Échec de la notification SignalR (TakeInProgress)."),
            TaskContinuationOptions.OnlyOnFaulted);

        return requestDto;
    }

    public async Task<RequestDto> MarkLineFoundAsync(
        Guid requestId,
        Guid lineId,
        CancellationToken cancellationToken = default)
    {
        var request = await _requestRepository.GetByIdAsync(requestId, cancellationToken)
            ?? throw new KeyNotFoundException($"Demande {requestId} introuvable.");

        if (request.Status != RequestStatus.InProgress)
            throw new BusinessException("La demande doit être en cours de traitement pour marquer une ligne.");

        var line = request.Lines.FirstOrDefault(l => l.Id == lineId)
            ?? throw new KeyNotFoundException($"Ligne {lineId} introuvable dans la demande {requestId}.");

        if (line.Status != RequestLineStatus.Pending)
            throw new BusinessException("Seules les lignes en attente peuvent être marquées comme trouvées.");

        line.Status = RequestLineStatus.Found;
        line.ModifiedAt = DateTime.UtcNow;
        line.ModifiedBy = request.StockistId?.ToString() ?? string.Empty;

        if (request.Lines.All(l => l.Status == RequestLineStatus.Found))
        {
            request.Status = RequestStatus.Delivered;
            request.CompletedAt = DateTime.UtcNow;
        }

        request.ModifiedAt = DateTime.UtcNow;

        await _requestRepository.UpdateAsync(request, cancellationToken);
        var requestDto = request.ToDto();

        _ = _notificationService.NotifyRequestUpdatedAsync(requestDto).ContinueWith(
            t => _logger.LogWarning(t.Exception, "Échec de la notification SignalR (MarkLineFound)."),
            TaskContinuationOptions.OnlyOnFaulted);

        return requestDto;
    }

    public async Task<RequestDto> ProposeAlternativesAsync(
        Guid requestId,
        ProposeAlternativesDto dto,
        CancellationToken cancellationToken = default)
    {
        var request = await _requestRepository.GetByIdAsync(requestId, cancellationToken)
            ?? throw new KeyNotFoundException($"Demande {requestId} introuvable.");

        if (request.Status != RequestStatus.InProgress)
            throw new BusinessException("Des alternatives ne peuvent être proposées que pour une demande en cours de traitement.");

        var now = DateTime.UtcNow;
        var actor = dto.StockistId.ToString();

        foreach (var line in dto.Lines)
        {
            request.AlternativeLines.Add(new AlternativeRequestLine
            {
                Id                  = Guid.NewGuid(),
                RequestId           = requestId,
                ArticleId           = line.ArticleId,
                ArticleName         = line.ArticleName,
                ArticleColorOrPrint = line.ArticleColorOrPrint,
                ArticleBarcode      = line.ArticleBarcode,
                RequestedSizes      = line.RequestedSizes.ToList(),
                Quantity            = line.Quantity,
                Status              = RequestLineStatus.AlternativeProposed,
                StockOverride       = line.StockOverride,
                CreatedAt           = now,
                CreatedBy           = actor,
            });
        }

        request.Status = RequestStatus.AwaitingSellerResponse;
        request.ModifiedAt = now;
        request.ModifiedBy = actor;

        await _requestRepository.UpdateAsync(request, cancellationToken);
        var requestDto = request.ToDto();

        _ = _notificationService.NotifyRequestUpdatedAsync(requestDto).ContinueWith(
            t => _logger.LogWarning(t.Exception, "Échec de la notification SignalR (ProposeAlternatives)."),
            TaskContinuationOptions.OnlyOnFaulted);

        return requestDto;
    }

    public async Task<RequestDto> RespondToAlternativesAsync(
        Guid requestId,
        RespondToAlternativesDto dto,
        CancellationToken cancellationToken = default)
    {
        var request = await _requestRepository.GetByIdAsync(requestId, cancellationToken)
            ?? throw new KeyNotFoundException($"Demande {requestId} introuvable.");

        if (request.Status != RequestStatus.AwaitingSellerResponse)
            throw new BusinessException("La demande doit être en attente de réponse vendeur.");

        var now = DateTime.UtcNow;

        foreach (var response in dto.Responses)
        {
            var altLine = request.AlternativeLines.FirstOrDefault(a => a.Id == response.AlternativeLineId)
                ?? throw new KeyNotFoundException($"Ligne alternative {response.AlternativeLineId} introuvable.");

            altLine.Status = response.Accepted
                ? RequestLineStatus.Found
                : RequestLineStatus.AlternativeDenied;

            altLine.ModifiedAt = now;
            altLine.ModifiedBy = request.SellerId.ToString();
        }

        request.Status = RequestStatus.InProgress;
        request.ModifiedAt = now;
        request.ModifiedBy = request.SellerId.ToString();

        await _requestRepository.UpdateAsync(request, cancellationToken);
        var requestDto = request.ToDto();

        _ = _notificationService.NotifyStockistRequestUpdatedAsync(requestDto).ContinueWith(
            t => _logger.LogWarning(t.Exception, "Échec de la notification SignalR (RespondToAlternatives)."),
            TaskContinuationOptions.OnlyOnFaulted);

        return requestDto;
    }
}
