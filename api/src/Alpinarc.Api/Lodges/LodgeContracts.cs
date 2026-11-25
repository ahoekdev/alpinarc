using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace Alpinarc.Api.Lodges;

// Request and response shapes for the lodges endpoints. Kept separate from the Lodge entity
// so the OpenAPI document (and the generated web client) stays stable when the entity grows.

public sealed class CreateLodgeRequest
{
    // [Required] also rejects empty and whitespace-only strings. [MinLength(1)] only adds
    // minLength to the OpenAPI document, for the generated Zod schema.
    [Required]
    [MinLength(1)]
    [MaxLength(200)]
    public required string Name { get; init; }
}

public sealed record LodgeResponse(Guid Id, string Name, DateTimeOffset CreatedAt)
{
    /// <summary>For queries: EF translates it to SQL, so only these columns are read.</summary>
    public static readonly Expression<Func<Lodge, LodgeResponse>> Projection =
        lodge => new LodgeResponse(lodge.Id, lodge.Name, lodge.CreatedAt);

    /// <summary>For an entity that is already loaded, such as one just saved.</summary>
    public static LodgeResponse From(Lodge lodge) => new(lodge.Id, lodge.Name, lodge.CreatedAt);
}
