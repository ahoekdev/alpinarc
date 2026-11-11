using System.Net;
using Microsoft.AspNetCore.Hosting;

namespace Alpinarc.Api.Tests;

[Collection(ApiCollection.Name)]
public class HealthEndpointTests(ApiFactory factory)
{
    [Theory]
    [InlineData("/health")]
    [InlineData("/health/live")]
    [InlineData("/health/ready")]
    public async Task Health_ReturnsHealthy(string path)
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync(path);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }

    [Theory]
    [InlineData("/health", HttpStatusCode.OK, "Healthy")]
    [InlineData("/health/live", HttpStatusCode.OK, "Healthy")]
    [InlineData("/health/ready", HttpStatusCode.ServiceUnavailable, "Unhealthy")]
    public async Task Health_WhenDatabaseIsUnreachable_OnlyReadyIsUnhealthy(
        string path, HttpStatusCode expectedStatus, string expectedBody)
    {
        // Nothing listens on port 1, so connecting to the database fails right away.
        using var unreachableDb = factory.WithWebHostBuilder(builder => builder.UseSetting(
            "ConnectionStrings:Alpinarc", "Host=localhost;Port=1;Database=x;Username=x;Password=x"));
        using var client = unreachableDb.CreateClient();

        var response = await client.GetAsync(path);

        Assert.Equal(expectedStatus, response.StatusCode);
        Assert.Equal(expectedBody, await response.Content.ReadAsStringAsync());
    }
}
