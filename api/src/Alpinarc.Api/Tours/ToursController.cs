using Alpinarc.Api.Data;
using Alpinarc.Api.Setup;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace Alpinarc.Api.Tours;

// See LodgesController for why the attributes are there.
[ApiController]
[Route("tours")]
[Produces("application/json")]
public class ToursController(AlpinarcDbContext db) : ControllerBase
{
    [HttpGet]
    [EndpointName("ListTours")]
    [ProducesResponseType<IReadOnlyList<TourSummaryResponse>>(StatusCodes.Status200OK)]
    public async Task<IReadOnlyList<TourSummaryResponse>> List(CancellationToken cancellationToken)
    {
        return await db.Tours
            .OrderBy(t => t.Name)
            .ThenBy(t => t.Id)
            .Select(TourSummaryResponse.Projection)
            .ToListAsync(cancellationToken);
    }

    [HttpGet("{id:guid}")]
    [EndpointName("GetTour")]
    [ProducesResponseType<TourResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TourResponse>> Get(Guid id, CancellationToken cancellationToken)
    {
        // Loaded rather than projected, so the response is built by the same code as Create's.
        var tour = await db.Tours
            .AsNoTracking()
            .AsSplitQuery()
            .Include(t => t.Stages).ThenInclude(ts => ts.Stage).ThenInclude(s => s.FromLodge)
            .Include(t => t.Stages).ThenInclude(ts => ts.Stage).ThenInclude(s => s.ToLodge)
            .SingleOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (tour is null)
        {
            return NotFound();
        }

        return TourResponse.From(tour);
    }

    [HttpPost]
    [EndpointName("CreateTour")]
    [EnableRateLimiting(RateLimitPolicies.Writes)]
    [Consumes("application/json")]
    [ProducesResponseType<TourResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<TourResponse>> Create(
        CreateTourRequest request, CancellationToken cancellationToken)
    {
        // One query for all the stages, each id once. Loaded with their lodges, for the response.
        var stageIds = request.StageIds.Distinct().ToList();
        var stages = await db.Stages
            .Include(s => s.FromLodge)
            .Include(s => s.ToLodge)
            .Where(s => stageIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, cancellationToken);

        // Keyed by the index in the request, so a form can show the error on that stage.
        // Whether the stages connect is not checked.
        for (var i = 0; i < request.StageIds.Count; i++)
        {
            if (!stages.ContainsKey(request.StageIds[i]))
            {
                ModelState.AddModelError($"stageIds[{i}]", "This stage doesn't exist.");
            }
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem();
        }

        // Postgres generates the id and creation time; EF reads them back on save and fills
        // in the tour stages' tour_id.
        var tour = new Tour
        {
            Name = request.Name,
            Description = request.Description,
            Stages = [.. request.StageIds.Select((id, index) => new TourStage { Position = index + 1, Stage = stages[id] })],
        };

        db.Tours.Add(tour);
        await db.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(Get), new { id = tour.Id }, TourResponse.From(tour));
    }
}
