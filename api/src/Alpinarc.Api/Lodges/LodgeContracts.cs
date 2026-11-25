using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace Alpinarc.Api.Lodges;

// Request and response shapes for the lodges endpoints. Kept separate from the Lodge entity
// so the OpenAPI document (and the generated web client) stays stable when the entity grows.

public sealed class CreateLodgeRequest
{
    // [Required] also rejects empty and whitespace-only strings. [MinLength(1)] only adds
    // minLength to the OpenAPI document, for the generated Zod schema.
    //
    // Trimmed when the JSON is read, before validation, so the length limit applies to the
    // trimmed name and "  Foo  " is stored as "Foo".
    [Required]
    [MinLength(1)]
    [MaxLength(200)]
    public required string Name { get; init => field = value?.Trim()!; }
}

public sealed record LodgeResponse(Guid Id, string Name, DateTimeOffset CreatedAt)
{
    /// <summary>For queries: EF translates it to SQL, so only these columns are read.</summary>
    public static readonly Expression<Func<Lodge, LodgeResponse>> Projection =
        lodge => new LodgeResponse(lodge.Id, lodge.Name, lodge.CreatedAt);

    /// <summary>For an entity that is already loaded, such as one just saved.</summary>
    public static LodgeResponse From(Lodge lodge) => new(lodge.Id, lodge.Name, lodge.CreatedAt);
}

/// <summary>A lodge as part of another response, such as the start or end of a stage.</summary>
public sealed record LodgeSummary(Guid Id, string Name)
{
    public static LodgeSummary From(Lodge lodge) => new(lodge.Id, lodge.Name);
}
