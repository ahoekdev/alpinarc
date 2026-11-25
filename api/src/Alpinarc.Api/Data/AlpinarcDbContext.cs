using Alpinarc.Api.Lodges;
using Microsoft.EntityFrameworkCore;

namespace Alpinarc.Api.Data;

public sealed class AlpinarcDbContext(DbContextOptions<AlpinarcDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Compares text ignoring case ("Foo" equals "foo") but not accents. Non-deterministic ICU
    /// collation, so equality, unique indexes and sorting on a column that uses it ignore case.
    /// </summary>
    public const string CaseInsensitiveCollation = "case_insensitive";

    public DbSet<Lodge> Lodges => Set<Lodge>();

    // Each entity is configured in its own IEntityTypeConfiguration<T> class next to the entity.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasCollation(
            CaseInsensitiveCollation, locale: "und-u-ks-level2", provider: "icu", deterministic: false);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AlpinarcDbContext).Assembly);
    }
}
