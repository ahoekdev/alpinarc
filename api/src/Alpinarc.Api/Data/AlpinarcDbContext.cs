using Alpinarc.Api.Lodges;
using Microsoft.EntityFrameworkCore;

namespace Alpinarc.Api.Data;

public class AlpinarcDbContext(DbContextOptions<AlpinarcDbContext> options) : DbContext(options)
{
    public DbSet<Lodge> Lodges => Set<Lodge>();

    // Each entity is configured in its own IEntityTypeConfiguration<T> class next to the entity.
    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AlpinarcDbContext).Assembly);
}
