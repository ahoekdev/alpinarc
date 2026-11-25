using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace Alpinarc.Api.Tests;

// Error responses use the ProblemDetails shape the OpenAPI document promises.
[Collection(ApiCollection.Name)]
public class ErrorResponseTests(ApiFactory factory)
{
    [Theory]
    [InlineData("/does-not-exist")]
    [InlineData("/lodges/not-a-guid")]
    public async Task UnknownRoute_ReturnsProblemDetails404(string path)
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync(path);

        await AssertProblemDetails(response, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UnhandledException_ReturnsProblemDetails500()
    {
        // Production, so the exception handler runs instead of the developer exception page.
        // Nothing listens on port 1, so the database query throws.
        using var unreachableDb = factory.WithWebHostBuilder(builder => builder
            .UseEnvironment("Production")
            .UseSetting("ConnectionStrings:Alpinarc", "Host=localhost;Port=1;Database=x;Username=x;Password=x"));
        using var client = unreachableDb.CreateClient();

        var response = await client.GetAsync("/lodges");

        var problem = await AssertProblemDetails(response, HttpStatusCode.InternalServerError);
        Assert.Null(problem.Detail);
    }

    private static async Task<ProblemDetails> AssertProblemDetails(
        HttpResponseMessage response, HttpStatusCode expectedStatus)
    {
        Assert.Equal(expectedStatus, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal((int)expectedStatus, problem.Status);
        return problem;
    }
}
