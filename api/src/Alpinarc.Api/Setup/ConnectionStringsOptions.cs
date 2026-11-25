using System.ComponentModel.DataAnnotations;

namespace Alpinarc.Api.Setup;

/// <summary>
/// The <c>ConnectionStrings</c> section, validated at startup so a missing connection string
/// fails the deploy instead of the first database call.
/// </summary>
public sealed class ConnectionStringsOptions
{
    public const string SectionName = "ConnectionStrings";

    [Required]
    public string Alpinarc { get; init; } = "";
}
