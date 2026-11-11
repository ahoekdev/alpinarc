using System.ComponentModel.DataAnnotations;

namespace Alpinarc.Api.Lodges;

// Request and response shapes for the lodges endpoints. Kept separate from the Lodge entity
// so the OpenAPI document (and the generated web client) stays stable when the entity grows.

public sealed class CreateLodgeRequest
{
    // [Required] also rejects empty and whitespace-only strings.
    [Required]
    [MaxLength(200)]
    public required string Name { get; init; }
}

public sealed record LodgeResponse(Guid Id, string Name, DateTimeOffset CreatedAt)
{
    public static LodgeResponse From(Lodge lodge) => new(lodge.Id, lodge.Name, lodge.CreatedAt);
}
