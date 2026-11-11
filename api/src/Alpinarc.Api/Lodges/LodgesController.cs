using Alpinarc.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Alpinarc.Api.Lodges;

// [EndpointName] sets the OpenAPI operation id, which Orval turns into the
// web client's function names (listLodges, getLodge, createLodge).
[ApiController]
[Route("lodges")]
public class LodgesController(AlpinarcDbContext db) : ControllerBase
{
    [HttpGet]
    [EndpointName("ListLodges")]
    [ProducesResponseType<IReadOnlyList<LodgeResponse>>(StatusCodes.Status200OK)]
    public async Task<IReadOnlyList<LodgeResponse>> List(CancellationToken cancellationToken)
    {
        var lodges = await db.Lodges
            .AsNoTracking()
            .OrderBy(l => l.Name)
            .ThenBy(l => l.Id)
            .ToListAsync(cancellationToken);

        return [.. lodges.Select(LodgeResponse.From)];
    }

    [HttpGet("{id:guid}")]
    [EndpointName("GetLodge")]
    [ProducesResponseType<LodgeResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LodgeResponse>> Get(Guid id, CancellationToken cancellationToken)
    {
        var lodge = await db.Lodges
            .AsNoTracking()
            .SingleOrDefaultAsync(l => l.Id == id, cancellationToken);

        if (lodge is null)
        {
            return NotFound();
        }

        return LodgeResponse.From(lodge);
    }

    [HttpPost]
    [EndpointName("CreateLodge")]
    [ProducesResponseType<LodgeResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LodgeResponse>> Create(
        CreateLodgeRequest request, CancellationToken cancellationToken)
    {
        // Postgres generates the id and creation time; EF reads them back on save.
        var lodge = new Lodge { Name = request.Name };

        db.Lodges.Add(lodge);

        await db.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(Get), new { id = lodge.Id }, LodgeResponse.From(lodge));
    }
}
