namespace Alpinarc.Api.Lodges;

/// <summary>A mountain hut.</summary>
public class Lodge
{
    public Guid Id { get; init; }

    public required string Name { get; set; }

    public DateTimeOffset CreatedAt { get; init; }
}
