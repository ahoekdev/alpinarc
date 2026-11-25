using Alpinarc.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Alpinarc.Api.Lodges;

public class LodgeConfiguration : IEntityTypeConfiguration<Lodge>
{
    /// <summary>The unique index on the name, named by the snake_case naming convention.</summary>
    public const string NameIndex = "ix_lodges_name";

    public void Configure(EntityTypeBuilder<Lodge> builder)
    {
        // Id is the key and Name (a non-nullable string) is required by EF Core convention.
        // Postgres generates the id: uuidv7() is time-ordered, so it indexes well (Postgres 18+).
        builder.Property(l => l.Id).HasDefaultValueSql("uuidv7()");
        // Names are unique, ignoring case: "Foo Hut" and "foo hut" are the same lodge.
        builder.Property(l => l.Name).HasMaxLength(200)
            .UseCollation(AlpinarcDbContext.CaseInsensitiveCollation);
        builder.HasIndex(l => l.Name).IsUnique().HasDatabaseName(NameIndex);
        // Postgres sets the creation time, so the value the API returns after an insert
        // is exactly the stored one (timestamptz has microsecond precision, .NET has 100 ns).
        builder.Property(l => l.CreatedAt).HasDefaultValueSql("now()");
    }
}
