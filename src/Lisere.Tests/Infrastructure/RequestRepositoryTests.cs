using Lisere.Domain.Entities;
using Lisere.Domain.Enums;
using Lisere.Infrastructure.Persistence;
using Lisere.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Lisere.Tests.Infrastructure;

public class RequestRepositoryTests
{
    private static LisereDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<LisereDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    // GetAllAsync uses Include(r => r.Seller) — required FK — InMemory filters out
    // rows where the related entity doesn't exist, so we must seed a User per Request.
    private static User BuildUser(Guid id) => new()
    {
        Id                   = id,
        UserName             = $"user_{id:N}",
        NormalizedUserName   = $"USER_{id:N}",
        Email                = $"user_{id:N}@test.com",
        NormalizedEmail      = $"USER_{id:N}@TEST.COM",
        SecurityStamp        = Guid.NewGuid().ToString(),
        FirstName            = "Test",
        LastName             = "User",
        CreatedAt            = DateTime.UtcNow,
        CreatedBy            = "test",
    };

    private static (Request request, User seller) BuildRequestWithSeller(
        RequestStatus status,
        DateTime createdAt,
        bool isDeleted = false)
    {
        var sellerId = Guid.NewGuid();
        var request = new Request
        {
            Id        = Guid.NewGuid(),
            SellerId  = sellerId,
            Zone      = ZoneType.RTW,
            Status    = status,
            Lines     = [],
            CreatedAt = createdAt,
            CreatedBy = "test",
            IsDeleted = isDeleted,
        };
        return (request, BuildUser(sellerId));
    }

    // For tests that do NOT use GetAllAsync (no Include on Seller), a plain Request is enough.
    private static Request BuildRequest(
        RequestStatus status,
        DateTime createdAt,
        bool isDeleted = false) => new()
    {
        Id        = Guid.NewGuid(),
        SellerId  = Guid.NewGuid(),
        Zone      = ZoneType.RTW,
        Status    = status,
        Lines     = [],
        CreatedAt = createdAt,
        CreatedBy = "test",
        IsDeleted = isDeleted,
    };

    // Helper pour créer un DbContext avec un nom de base précis (pour tests multi-contexte)
    private static LisereDbContext CreateContextWithName(string name) =>
        new(new DbContextOptionsBuilder<LisereDbContext>()
            .UseInMemoryDatabase(name)
            .Options);

    // ── GetByIdAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ReturnsRequest_WhenIdMatches()
    {
        // Tue le mutant r.Id == id → r.Id != id (ligne 23)
        // GetByIdAsync fait Include(Seller) — FK requise sur InMemory → doit seeder le User
        await using var context = CreateContext();
        var repo = new RequestRepository(context);

        var (request, seller) = BuildRequestWithSeller(RequestStatus.Pending, DateTime.UtcNow);
        context.Users.Add(seller);
        context.Requests.Add(request);
        await context.SaveChangesAsync();

        var result = await repo.GetByIdAsync(request.Id);

        Assert.NotNull(result);
        Assert.Equal(request.Id, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenIdDoesNotMatch()
    {
        await using var context = CreateContext();
        var repo = new RequestRepository(context);

        var (request, seller) = BuildRequestWithSeller(RequestStatus.Pending, DateTime.UtcNow);
        context.Users.Add(seller);
        context.Requests.Add(request);
        await context.SaveChangesAsync();

        var result = await repo.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    // ── AddAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task AddAsync_PersistsToDatabase()
    {
        // Tue le mutant SaveChangesAsync supprimé (ligne 51-52) via un second contexte
        var dbName = Guid.NewGuid().ToString();
        var request = BuildRequest(RequestStatus.Pending, DateTime.UtcNow);

        await using (var ctx1 = CreateContextWithName(dbName))
        {
            await new RequestRepository(ctx1).AddAsync(request);
        }

        await using var ctx2 = CreateContextWithName(dbName);
        var stored = await ctx2.Requests
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.Id == request.Id);
        Assert.NotNull(stored);
    }

    // ── GetAllAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_ReturnsMostRecentFirst()
    {
        await using var context = CreateContext();
        var repo = new RequestRepository(context);

        var (oldest, s1) = BuildRequestWithSeller(RequestStatus.Pending, DateTime.UtcNow.AddMinutes(-10));
        var (middle, s2) = BuildRequestWithSeller(RequestStatus.Pending, DateTime.UtcNow.AddMinutes(-5));
        var (newest, s3) = BuildRequestWithSeller(RequestStatus.Pending, DateTime.UtcNow);
        context.Users.AddRange(s1, s2, s3);
        context.Requests.AddRange(oldest, middle, newest);
        await context.SaveChangesAsync();

        var (items, _) = await repo.GetAllAsync(1, 10);
        var list = items.ToList();

        Assert.Equal(3, list.Count);
        Assert.Equal(newest.Id, list[0].Id);
        Assert.Equal(oldest.Id, list[2].Id);
    }

    [Fact]
    public async Task GetAllAsync_PaginationSkipsCorrectly()
    {
        await using var context = CreateContext();
        var repo = new RequestRepository(context);

        var pairs = Enumerable.Range(1, 5)
            .Select(i => BuildRequestWithSeller(RequestStatus.Pending, DateTime.UtcNow.AddMinutes(-i)))
            .ToList();
        context.Users.AddRange(pairs.Select(p => p.seller));
        context.Requests.AddRange(pairs.Select(p => p.request));
        await context.SaveChangesAsync();

        var (page1Items, _) = await repo.GetAllAsync(1, 2);
        var (page2Items, _) = await repo.GetAllAsync(2, 2);

        var page1Ids = page1Items.Select(r => r.Id).ToHashSet();
        var page2Ids = page2Items.Select(r => r.Id).ToHashSet();

        Assert.Equal(2, page1Ids.Count);
        Assert.Equal(2, page2Ids.Count);
        Assert.Empty(page1Ids.Intersect(page2Ids));
    }

    [Fact]
    public async Task GetAllAsync_CapsPageSizeAt50()
    {
        await using var context = CreateContext();
        var repo = new RequestRepository(context);

        var pairs = Enumerable.Range(1, 60)
            .Select(i => BuildRequestWithSeller(RequestStatus.Pending, DateTime.UtcNow.AddMinutes(-i)))
            .ToList();
        context.Users.AddRange(pairs.Select(p => p.seller));
        context.Requests.AddRange(pairs.Select(p => p.request));
        await context.SaveChangesAsync();

        var (items, totalCount) = await repo.GetAllAsync(1, 999);

        Assert.Equal(60, totalCount);
        Assert.Equal(50, items.Count());
    }

    [Fact]
    public async Task GetAllAsync_PageBelowOne_UsesPageOne()
    {
        await using var context = CreateContext();
        var repo = new RequestRepository(context);

        var (req1, s1) = BuildRequestWithSeller(RequestStatus.Pending, DateTime.UtcNow.AddMinutes(-2));
        var (req2, s2) = BuildRequestWithSeller(RequestStatus.Pending, DateTime.UtcNow.AddMinutes(-1));
        context.Users.AddRange(s1, s2);
        context.Requests.AddRange(req1, req2);
        await context.SaveChangesAsync();

        var (itemsPage0, total0) = await repo.GetAllAsync(0, 10);
        var (itemsPage1, total1) = await repo.GetAllAsync(1, 10);

        Assert.Equal(total1, total0);
        Assert.Equal(
            itemsPage1.Select(r => r.Id).ToList(),
            itemsPage0.Select(r => r.Id).ToList());
    }

    [Fact]
    public async Task GetAllAsync_ExcludesSoftDeletedRequests()
    {
        await using var context = CreateContext();
        var repo = new RequestRepository(context);

        var (active,  sa) = BuildRequestWithSeller(RequestStatus.Pending, DateTime.UtcNow.AddMinutes(-1));
        var (deleted, sd) = BuildRequestWithSeller(RequestStatus.Pending, DateTime.UtcNow.AddMinutes(-2), isDeleted: true);
        context.Users.AddRange(sa, sd);
        context.Requests.AddRange(active, deleted);
        await context.SaveChangesAsync();

        var (items, totalCount) = await repo.GetAllAsync(1, 10);

        Assert.Equal(1, totalCount);
        Assert.Single(items);
        Assert.Equal(active.Id, items.First().Id);
    }

    // ── GetExpiredPendingAsync ─────────────────────────────────────────────

    [Fact]
    public async Task GetExpiredPendingAsync_ReturnsOnlyPendingBeforeThreshold()
    {
        await using var context = CreateContext();
        var repo = new RequestRepository(context);

        var threshold = DateTime.UtcNow.AddMinutes(-30);

        var expiredPending    = BuildRequest(RequestStatus.Pending,    DateTime.UtcNow.AddMinutes(-31));
        var recentPending     = BuildRequest(RequestStatus.Pending,    DateTime.UtcNow.AddMinutes(-29));
        var expiredInProgress = BuildRequest(RequestStatus.InProgress, DateTime.UtcNow.AddMinutes(-31));
        context.Requests.AddRange(expiredPending, recentPending, expiredInProgress);
        await context.SaveChangesAsync();

        var result = (await repo.GetExpiredPendingAsync(threshold)).ToList();

        Assert.Single(result);
        Assert.Equal(expiredPending.Id, result[0].Id);
    }

    [Fact]
    public async Task GetExpiredPendingAsync_RequestCreatedExactlyAtThreshold_IsCancelled()
    {
        // Tue le mutant <= → < (strict) : la borne exacte doit être incluse
        await using var context = CreateContext();
        var repo = new RequestRepository(context);

        var threshold = DateTime.UtcNow - TimeSpan.FromMinutes(30);
        var request = BuildRequest(RequestStatus.Pending, threshold);
        context.Requests.Add(request);
        await context.SaveChangesAsync();

        var result = (await repo.GetExpiredPendingAsync(threshold)).ToList();

        Assert.Single(result);
        Assert.Equal(request.Id, result[0].Id);
    }

    // ── DeleteAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_SetIsDeletedTrue()
    {
        await using var context = CreateContext();
        var repo = new RequestRepository(context);

        var request = BuildRequest(RequestStatus.Pending, DateTime.UtcNow);
        context.Requests.Add(request);
        await context.SaveChangesAsync();

        await repo.DeleteAsync(request.Id);

        var fromDb = await context.Requests
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.Id == request.Id);

        Assert.NotNull(fromDb);
        Assert.True(fromDb.IsDeleted);
    }

    [Fact]
    public async Task DeleteAsync_PersistsIsDeletedTrueToDatabase()
    {
        // Tue le mutant SaveChangesAsync supprimé (ligne 70) via un second contexte
        var dbName = Guid.NewGuid().ToString();
        var request = BuildRequest(RequestStatus.Pending, DateTime.UtcNow);

        await using (var ctx1 = CreateContextWithName(dbName))
        {
            ctx1.Requests.Add(request);
            await ctx1.SaveChangesAsync();
        }

        await using (var ctx2 = CreateContextWithName(dbName))
        {
            await new RequestRepository(ctx2).DeleteAsync(request.Id);
        }

        await using var ctx3 = CreateContextWithName(dbName);
        var fromDb = await ctx3.Requests
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.Id == request.Id);
        Assert.NotNull(fromDb);
        Assert.True(fromDb.IsDeleted);
    }
}
