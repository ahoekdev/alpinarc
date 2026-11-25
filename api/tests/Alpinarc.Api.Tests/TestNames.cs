namespace Alpinarc.Api.Tests;

/// <summary>
/// Lodge names are unique and every test shares one database, so each lodge a test creates
/// gets a name no other test uses.
/// </summary>
internal static class TestNames
{
    public static string Unique(string name) => $"{name} {Guid.NewGuid():N}";

    public static string Unique(int length) => Guid.NewGuid().ToString("N").PadRight(length, 'a');
}
