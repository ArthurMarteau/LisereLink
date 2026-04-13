using Lisere.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Lisere.Infrastructure.Persistence;

public class LisereDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public LisereDbContext(DbContextOptions<LisereDbContext> options) : base(options)
    {
    }

    public DbSet<Request> Requests => Set<Request>();
    public DbSet<RequestLine> RequestLines => Set<RequestLine>();
    public DbSet<AlternativeRequestLine> AlternativeRequestLines => Set<AlternativeRequestLine>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(LisereDbContext).Assembly);
    }
}
