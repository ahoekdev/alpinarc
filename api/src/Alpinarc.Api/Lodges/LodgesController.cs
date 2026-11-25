using Alpinarc.Api.Data;
using Alpinarc.Api.Setup;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Alpinarc.Api.Lodges;

// [EndpointName] sets the OpenAPI operation id, which Orval turns into the
// web client's function names (listLodges, getLodge, createLodge).
// [Produces]/[Consumes] limit the OpenAPI document to JSON; without them it also
// lists text/plain and text/json, and Orval generates a duplicate type for each.
[ApiController]
[Route("lodges")]
[Produces("application/json")]
public class LodgesController(AlpinarcDbContext db) : ControllerBase
{
    [HttpGet]
    [EndpointName("ListLodges")]
    [ProducesResponseType<IReadOnlyList<LodgeResponse>>(StatusCodes.Status200OK)]
    public async Task<IReadOnlyList<LodgeResponse>> List(CancellationToken cancellationToken)
    {
        return await db.Lodges
            .OrderBy(l => l.Name)
            .ThenBy(l => l.Id)
            .Select(LodgeResponse.Projection)
            .ToListAsync(cancellationToken);
    }

    [HttpGet("{id:guid}")]
    [EndpointName("GetLodge")]
    [ProducesResponseType<LodgeResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LodgeResponse>> Get(Guid id, CancellationToken cancellationToken)
    {
        var lodge = await db.Lodges
            .Where(l => l.Id == id)
            .Select(LodgeResponse.Projection)
            .SingleOrDefaultAsync(cancellationToken);

        if (lodge is null)
        {
            return NotFound();
        }

        return lodge;
    }

    [HttpPost]
    [EndpointName("CreateLodge")]
    [EnableRateLimiting(RateLimitPolicies.Writes)]
    [Consumes("application/json")]
    [ProducesResponseType<LodgeResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<LodgeResponse>> Create(
        CreateLodgeRequest request, CancellationToken cancellationToken)
    {
        // Postgres generates the id and creation time; EF reads them back on save.
        var lodge = new Lodge { Name = request.Name };

        db.Lodges.Add(lodge);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: LodgeConfiguration.NameIndex,
        })
        {
            // Keyed by "name" like a validation error, so a form can show it on the field.
            ModelState.AddModelError("name", "A lodge with this name already exists.");
            return ValidationProblem(statusCode: StatusCodes.Status409Conflict, modelStateDictionary: ModelState);
        }

        return CreatedAtAction(nameof(Get), new { id = lodge.Id }, LodgeResponse.From(lodge));
    }
}
