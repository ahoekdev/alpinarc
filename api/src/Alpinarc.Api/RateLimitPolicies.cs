namespace Alpinarc.Api;

/// <summary>Names of the rate-limiting policies registered in Program.cs.</summary>
public static class RateLimitPolicies
{
    /// <summary>Anonymous write endpoints, limited per minute by <c>RateLimiting:WritesPerMinute</c>.</summary>
    public const string Writes = "writes";
}
