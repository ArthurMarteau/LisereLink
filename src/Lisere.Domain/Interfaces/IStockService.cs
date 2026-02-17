using Lisere.Domain.Entities;
using Lisere.Domain.Enums;

namespace Lisere.Domain.Interfaces;

public interface IStockService
{
    Task<Stock?> GetAvailabilityAsync(Guid articleId, Size size, CancellationToken cancellationToken = default);
}
