using Alpinarc.Api.Data;
using Alpinarc.Api.Setup;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace Alpinarc.Api.Stages;

// See LodgesController for why the attributes are there.
[ApiController]
[Route("stages")]
[Produces("application/json")]
public class StagesController(AlpinarcDbContext db) : ControllerBase
{
    [HttpGet]
    [EndpointName("ListStages")]
    [ProducesResponseType<IReadOnlyList<StageResponse>>(StatusCodes.Status200OK)]
    public async Task<IReadOnlyList<StageResponse>> List(CancellationToken cancellationToken)
    {
        return await db.Stages
            .OrderBy(s => s.FromLodge.Name)
            .ThenBy(s => s.ToLodge.Name)
            .ThenBy(s => s.Id)
            .Select(StageResponse.Projection)
            .ToListAsync(cancellationToken);
    }

    [HttpGet("{id:guid}")]
    [EndpointName("GetStage")]
    [ProducesResponseType<StageResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StageResponse>> Get(Guid id, CancellationToken cancellationToken)
    {
        var stage = await db.Stages
            .Where(s => s.Id == id)
            .Select(StageResponse.Projection)
            .SingleOrDefaultAsync(cancellationToken);

        if (stage is null)
        {
            return NotFound();
        }

        return stage;
    }

    [HttpPost]
    [EndpointName("CreateStage")]
    [EnableRateLimiting(RateLimitPolicies.Writes)]
    [Consumes("application/json")]
    [ProducesResponseType<StageResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<StageResponse>> Create(
        CreateStageRequest request, CancellationToken cancellationToken)
    {
        // Loaded rather than only checked, so the response can include their names.
        var lodges = await db.Lodges
            .Where(l => l.Id == request.FromLodgeId || l.Id == request.ToLodgeId)
            .ToDictionaryAsync(l => l.Id, cancellationToken);

        // Errors are keyed by JSON property name, like the validation errors, so a form can
        // show them on the field.
        if (!lodges.TryGetValue(request.FromLodgeId, out var fromLodge))
        {
            ModelState.AddModelError("fromLodgeId", "This lodge doesn't exist.");
        }

        if (!lodges.TryGetValue(request.ToLodgeId, out var toLodge))
        {
            ModelState.AddModelError("toLodgeId", "This lodge doesn't exist.");
        }
        else if (request.ToLodgeId == request.FromLodgeId)
        {
            ModelState.AddModelError("toLodgeId", "A stage must end at a different lodge than it starts at.");
        }

        if (fromLodge is null || toLodge is null || !ModelState.IsValid)
        {
            return ValidationProblem();
        }

        // Postgres generates the id and creation time; EF reads them back on save.
        var stage = new Stage
        {
            FromLodge = fromLodge,
            ToLodge = toLodge,
            DistanceMeters = request.DistanceMeters,
            AscentMeters = request.AscentMeters,
            DescentMeters = request.DescentMeters,
            DurationMinutes = request.DurationMinutes,
        };

        db.Stages.Add(stage);
        await db.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(Get), new { id = stage.Id }, StageResponse.From(stage));
    }
}
