using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Alpinarc.Api.Tours;

public class TourConfiguration : IEntityTypeConfiguration<Tour>
{
    public const int NameMaxLength = 200;

    public const int DescriptionMaxLength = 2000;

    public void Configure(EntityTypeBuilder<Tour> builder)
    {
        builder.Property(t => t.Id).HasDefaultValueSql("uuidv7()");
        // Not unique, unlike lodge names: tours are told apart by id.
        builder.Property(t => t.Name).HasMaxLength(NameMaxLength);
        builder.Property(t => t.Description).HasMaxLength(DescriptionMaxLength);
        builder.Property(t => t.CreatedAt).HasDefaultValueSql("now()");
    }
}

public class TourStageConfiguration : IEntityTypeConfiguration<TourStage>
{
    public void Configure(EntityTypeBuilder<TourStage> builder)
    {
        // The column is "position", not "order": ORDER is a reserved word in SQL.
        builder.HasKey(ts => new { ts.TourId, ts.Position });
        // Named explicitly: there is no DbSet for it, so EF would call it "tour_stage".
        builder.ToTable("tour_stages", table => table.HasCheckConstraint("ck_tour_stages_position", "position >= 1"));

        builder.HasOne(ts => ts.Tour).WithMany(t => t.Stages).HasForeignKey(ts => ts.TourId)
            .OnDelete(DeleteBehavior.Cascade);
        // Restrict: a stage that a tour uses can't be deleted.
        builder.HasOne(ts => ts.Stage).WithMany().HasForeignKey(ts => ts.StageId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
