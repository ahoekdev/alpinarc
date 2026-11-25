using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Alpinarc.Api.Stages;

public class StageConfiguration : IEntityTypeConfiguration<Stage>
{
    public void Configure(EntityTypeBuilder<Stage> builder)
    {
        builder.Property(s => s.Id).HasDefaultValueSql("uuidv7()");
        builder.Property(s => s.CreatedAt).HasDefaultValueSql("now()");

        // Restrict: a lodge that a stage starts or ends at can't be deleted.
        builder.HasOne(s => s.FromLodge).WithMany().HasForeignKey(s => s.FromLodgeId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(s => s.ToLodge).WithMany().HasForeignKey(s => s.ToLodgeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Not unique: several stages between the same two lodges are variants, e.g. a glacier
        // route. The index also covers the from_lodge_id foreign key; EF adds one for to_lodge_id.
        builder.HasIndex(s => new { s.FromLodgeId, s.ToLodgeId });

        // The same ranges as the [Range]s on CreateStageRequest, as a last line of defence.
        builder.ToTable(table =>
        {
            table.HasCheckConstraint("ck_stages_different_lodges", "from_lodge_id <> to_lodge_id");
            table.HasCheckConstraint("ck_stages_distance_meters",
                $"distance_meters BETWEEN {StageLimits.MinDistanceMeters} AND {StageLimits.MaxDistanceMeters}");
            table.HasCheckConstraint("ck_stages_ascent_meters",
                $"ascent_meters BETWEEN {StageLimits.MinElevationMeters} AND {StageLimits.MaxElevationMeters}");
            table.HasCheckConstraint("ck_stages_descent_meters",
                $"descent_meters BETWEEN {StageLimits.MinElevationMeters} AND {StageLimits.MaxElevationMeters}");
            table.HasCheckConstraint("ck_stages_duration_minutes",
                $"duration_minutes BETWEEN {StageLimits.MinDurationMinutes} AND {StageLimits.MaxDurationMinutes}");
        });
    }
}
