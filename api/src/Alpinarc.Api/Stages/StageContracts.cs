using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using Alpinarc.Api.Lodges;

namespace Alpinarc.Api.Stages;

// Request and response shapes for the stages endpoints, kept separate from the Stage entity.

public sealed class CreateStageRequest
{
    // `required` makes the JSON deserializer reject a request that leaves a property out,
    // instead of reading it as Guid.Empty or 0.
    public required Guid FromLodgeId { get; init; }

    public required Guid ToLodgeId { get; init; }

    [Range(StageLimits.MinDistanceMeters, StageLimits.MaxDistanceMeters)]
    public required int DistanceMeters { get; init; }

    [Range(StageLimits.MinElevationMeters, StageLimits.MaxElevationMeters)]
    public required int AscentMeters { get; init; }

    [Range(StageLimits.MinElevationMeters, StageLimits.MaxElevationMeters)]
    public required int DescentMeters { get; init; }

    [Range(StageLimits.MinDurationMinutes, StageLimits.MaxDurationMinutes)]
    public required int DurationMinutes { get; init; }
}

public sealed record StageResponse(
    Guid Id,
    LodgeSummary FromLodge,
    LodgeSummary ToLodge,
    int DistanceMeters,
    int AscentMeters,
    int DescentMeters,
    int DurationMinutes,
    DateTimeOffset CreatedAt)
{
    /// <summary>For queries: EF translates it to SQL, joining the two lodges.</summary>
    public static readonly Expression<Func<Stage, StageResponse>> Projection =
        stage => new StageResponse(
            stage.Id,
            new LodgeSummary(stage.FromLodge.Id, stage.FromLodge.Name),
            new LodgeSummary(stage.ToLodge.Id, stage.ToLodge.Name),
            stage.DistanceMeters,
            stage.AscentMeters,
            stage.DescentMeters,
            stage.DurationMinutes,
            stage.CreatedAt);

    /// <summary>For a stage that is already loaded, with its two lodges.</summary>
    public static StageResponse From(Stage stage) => new(
        stage.Id,
        LodgeSummary.From(stage.FromLodge),
        LodgeSummary.From(stage.ToLodge),
        stage.DistanceMeters,
        stage.AscentMeters,
        stage.DescentMeters,
        stage.DurationMinutes,
        stage.CreatedAt);
}
