using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;

namespace Alpinarc.Api.Tests;

// Railway terminates HTTPS at its edge and forwards plain HTTP with X-Forwarded-Proto.
[Collection(ApiCollection.Name)]
public class ForwardedHeadersTests(ApiFactory factory)
{
    [Theory]
    [InlineData(null, "http")]
    [InlineData("https", "https")]
    public async Task Create_LocationHeader_UsesForwardedScheme(string? forwardedProto, string expectedScheme)
    {
        using var production = factory.WithWebHostBuilder(builder => builder.UseEnvironment("Production"));
        using var client = production.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/lodges")
        {
            Content = JsonContent.Create(new { name = TestNames.Unique("Forwarded Hut") }),
        };
        if (forwardedProto is not null)
        {
            request.Headers.Add("X-Forwarded-Proto", forwardedProto);
        }

        var response = await client.SendAsync(request, TestRun.Token);

        response.EnsureSuccessStatusCode();
        Assert.Equal(expectedScheme, response.Headers.Location?.Scheme);
    }
}
