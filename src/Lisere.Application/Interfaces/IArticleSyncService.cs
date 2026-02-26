namespace Lisere.Application.Interfaces;

public interface IArticleSyncService
{
    Task SyncAsync(CancellationToken cancellationToken = default);
}
