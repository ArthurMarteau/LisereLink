using Lisere.Application.Common;
using Lisere.Application.DTOs;

namespace Lisere.Application.Interfaces;

public interface IRequestService
{
    Task<RequestDto> CreateAsync(CreateRequestDto dto, CancellationToken cancellationToken = default);

    Task<PagedResult<RequestDto>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    Task<RequestDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<RequestDto> UpdateAsync(Guid id, UpdateRequestDto dto, CancellationToken cancellationToken = default);

    Task CancelAsync(Guid id, CancellationToken cancellationToken = default);
}
