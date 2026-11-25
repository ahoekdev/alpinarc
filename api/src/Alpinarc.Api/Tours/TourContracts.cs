using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using Alpinarc.Api.Stages;

namespace Alpinarc.Api.Tours;

// Request and response shapes for the tours endpoints, kept separate from the Tour entity.

public sealed class CreateTourRequest
{
    // Trimmed when the JSON is read, before validation; see CreateLodgeRequest.Name.
    [Required]
    [MinLength(1)]
    [MaxLength(TourConfiguration.NameMaxLength)]
    public required string Name { get; init => field = value?.Trim()!; }

    // Trimmed too, and an empty or whitespace-only description is stored as null.
    [MaxLength(TourConfiguration.DescriptionMaxLength)]
    public string? Description { get; init => field = string.IsNullOrWhiteSpace(value) ? null : value.Trim(); }

    /// <summary>
    /// The stages in the tour's order; may be empty, and may hold the same stage more than
    /// once. Clients never send positions.
    /// </summary>
    [Required]
    public required IReadOnlyList<Guid> StageIds { get; init; }
}

/// <summary>
/// Sums over a tour's stages; all 0 for an empty tour. <see langword="long"/>, because a tour
/// has no limit on its number of stages.
/// </summary>
public sealed record TourTotals(long DistanceMeters, long AscentMeters, long DescentMeters, long DurationMinutes)
{
    public static TourTotals From(IEnumerable<Stage> stages)
    {
        var list = stages.ToList();
        return new TourTotals(
            list.Sum(s => (long)s.DistanceMeters),
            list.Sum(s => (long)s.AscentMeters),
            list.Sum(s => (long)s.DescentMeters),
            list.Sum(s => (long)s.DurationMinutes));
    }
}

public sealed record TourStageResponse(int Position, StageResponse Stage);

public sealed record TourResponse(
    Guid Id,
    string Name,
    string? Description,
    DateTimeOffset CreatedAt,
    IReadOnlyList<TourStageResponse> Stages,
    TourTotals Totals)
{
    /// <summary>
    /// For a tour that is already loaded, with its stages and their lodges. The stages may be
    /// in any order; the response has them by position.
    /// </summary>
    public static TourResponse From(Tour tour)
    {
        var stages = tour.Stages.OrderBy(ts => ts.Position).ToList();

        return new TourResponse(
            tour.Id,
            tour.Name,
            tour.Description,
            tour.CreatedAt,
            [.. stages.Select(ts => new TourStageResponse(ts.Position, StageResponse.From(ts.Stage)))],
            TourTotals.From(stages.Select(ts => ts.Stage)));
    }
}

public sealed record TourSummaryResponse(
    Guid Id,
    string Name,
    string? Description,
    DateTimeOffset CreatedAt,
    int StageCount,
    TourTotals Totals)
{
    /// <summary>
    /// For queries: EF translates it to SQL, so the count and totals are computed in the
    /// database. A sum over no stages is 0.
    /// </summary>
    public static readonly Expression<Func<Tour, TourSummaryResponse>> Projection =
        tour => new TourSummaryResponse(
            tour.Id,
            tour.Name,
            tour.Description,
            tour.CreatedAt,
            tour.Stages.Count,
            new TourTotals(
                tour.Stages.Sum(ts => (long)ts.Stage.DistanceMeters),
                tour.Stages.Sum(ts => (long)ts.Stage.AscentMeters),
                tour.Stages.Sum(ts => (long)ts.Stage.DescentMeters),
                tour.Stages.Sum(ts => (long)ts.Stage.DurationMinutes)));
}
