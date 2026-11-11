using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Alpinarc.Api.Lodges;

public class LodgeConfiguration : IEntityTypeConfiguration<Lodge>
{
    public void Configure(EntityTypeBuilder<Lodge> lodge)
    {
        lodge.HasKey(l => l.Id);
        // Postgres generates the id: uuidv7() is time-ordered, so it indexes well (Postgres 18+).
        lodge.Property(l => l.Id).HasDefaultValueSql("uuidv7()");
        lodge.Property(l => l.Name).IsRequired().HasMaxLength(200);
        // Postgres sets the creation time, so the value the API returns after an insert
        // is exactly the stored one (timestamptz has microsecond precision, .NET has 100 ns).
        lodge.Property(l => l.CreatedAt).HasDefaultValueSql("now()");
    }
}
