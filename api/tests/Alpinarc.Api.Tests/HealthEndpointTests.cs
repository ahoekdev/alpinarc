using System.Net;

namespace Alpinarc.Api.Tests;

[Collection(ApiCollection.Name)]
public class HealthEndpointTests(ApiFactory factory)
{
    [Fact]
    public async Task Health_ReturnsHealthy()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }
}
