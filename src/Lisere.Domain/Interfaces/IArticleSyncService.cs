namespace Lisere.Domain.Interfaces;

public interface IArticleSyncService
{
    Task SyncAsync(CancellationToken cancellationToken = default);
}
