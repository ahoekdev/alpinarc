using System.ComponentModel.DataAnnotations;

namespace Alpinarc.Api;

/// <summary>The origins of the web app, which the CORS policy allows. Validated at startup.</summary>
public sealed class WebCorsOptions : IValidatableObject
{
    public const string SectionName = "Cors";

    // Without a valid origin the browser blocks every call from the web app, so fail at startup instead.
    [Required]
    [MinLength(1)]
    public string[] AllowedOrigins { get; init; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) =>
        AllowedOrigins
            .Where(origin => !Uri.TryCreate(origin, UriKind.Absolute, out _))
            .Select(origin => new ValidationResult(
                $"'{origin}' is not an absolute URL.", [nameof(AllowedOrigins)]));
}
