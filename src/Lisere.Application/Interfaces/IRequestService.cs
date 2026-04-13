using Lisere.Application.Common;
using Lisere.Application.DTOs;

namespace Lisere.Application.Interfaces;

public interface IRequestService
{
    Task<RequestDto> CreateAsync(CreateRequestDto dto, CancellationToken cancellationToken = default);

    Task<PagedResult<RequestDto>> GetAllAsync(
        int page,
        int pageSize,
        string? storeId = null,
        string? zone = null,
        CancellationToken cancellationToken = default);

    Task<RequestDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<RequestDto> UpdateAsync(Guid id, UpdateRequestDto dto, CancellationToken cancellationToken = default);

    Task CancelAsync(Guid id, CancellationToken cancellationToken = default);

    Task<RequestDto> TakeInProgressAsync(Guid requestId, Guid stockistId, CancellationToken cancellationToken = default);

    Task<RequestDto> MarkLineFoundAsync(Guid requestId, Guid lineId, CancellationToken cancellationToken = default);

    Task<RequestDto> ProposeAlternativesAsync(Guid requestId, ProposeAlternativesDto dto, CancellationToken cancellationToken = default);

    Task<RequestDto> RespondToAlternativesAsync(Guid requestId, RespondToAlternativesDto dto, CancellationToken cancellationToken = default);
}
