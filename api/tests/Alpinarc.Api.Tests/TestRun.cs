namespace Alpinarc.Api.Tests;

/// <summary>
/// The running test's cancellation token, passed to every async call so a cancelled or
/// timed-out test run stops right away (xunit rule xUnit1051).
/// </summary>
internal static class TestRun
{
    public static CancellationToken Token => TestContext.Current.CancellationToken;
}
