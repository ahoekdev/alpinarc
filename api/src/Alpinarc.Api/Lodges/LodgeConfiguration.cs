using Alpinarc.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Alpinarc.Api.Lodges;

public class LodgeConfiguration : IEntityTypeConfiguration<Lodge>
{
    /// <summary>The unique index on the name, named by the snake_case naming convention.</summary>
    public const string NameIndex = "ix_lodges_name";

    public void Configure(EntityTypeBuilder<Lodge> lodge)
    {
        lodge.HasKey(l => l.Id);
        // Postgres generates the id: uuidv7() is time-ordered, so it indexes well (Postgres 18+).
        lodge.Property(l => l.Id).HasDefaultValueSql("uuidv7()");
        // Names are unique, ignoring case: "Foo Hut" and "foo hut" are the same lodge.
        lodge.Property(l => l.Name).IsRequired().HasMaxLength(200)
            .UseCollation(AlpinarcDbContext.CaseInsensitiveCollation);
        lodge.HasIndex(l => l.Name).IsUnique().HasDatabaseName(NameIndex);
        // Postgres sets the creation time, so the value the API returns after an insert
        // is exactly the stored one (timestamptz has microsecond precision, .NET has 100 ns).
        lodge.Property(l => l.CreatedAt).HasDefaultValueSql("now()");
    }
}
