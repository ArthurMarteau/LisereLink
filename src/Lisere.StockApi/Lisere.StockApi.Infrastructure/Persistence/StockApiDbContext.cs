using Lisere.Domain.Entities;
using Lisere.StockApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lisere.StockApi.Infrastructure.Persistence;

public class StockApiDbContext : DbContext
{
    public StockApiDbContext(DbContextOptions<StockApiDbContext> options) : base(options)
    {
    }

    public DbSet<Article> Articles => Set<Article>();
    public DbSet<StockEntry> StockEntries => Set<StockEntry>();
    public DbSet<Store> Stores => Set<Store>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(StockApiDbContext).Assembly);
    }
}
