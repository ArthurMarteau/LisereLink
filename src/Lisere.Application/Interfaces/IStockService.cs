using Lisere.Domain.Enums;

namespace Lisere.Application.Interfaces;

public interface IStockService
{
    Task<int> GetAvailabilityAsync(Guid articleId, Size size, CancellationToken cancellationToken = default);

    Task<bool> IsAvailableAsync(Guid articleId, Size size, CancellationToken cancellationToken = default);
}
