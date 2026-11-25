using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using Alpinarc.Api.Lodges;
using Microsoft.AspNetCore.Mvc;

namespace Alpinarc.Api.Tests;

// The database is shared by every test in the run, so no test may assume the lodges table is empty.
[Collection(ApiCollection.Name)]
public class LodgesEndpointTests(ApiFactory factory)
{
    [Fact]
    public async Task Create_ThenGetById_ReturnsSameLodge()
    {
        using var client = factory.CreateClient();

        var name = TestNames.Unique("Refuge du Goûter");

        var createResponse = await client.PostAsJsonAsync("/lodges", new { name });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<LodgeResponse>();
        Assert.NotNull(created);
        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.Equal(name, created.Name);
        Assert.Equal($"/lodges/{created.Id}", createResponse.Headers.Location?.AbsolutePath);

        var fetched = await client.GetFromJsonAsync<LodgeResponse>(createResponse.Headers.Location);

        Assert.Equal(created, fetched);
    }

    [Theory]
    [InlineData("""{ "name": "" }""")]
    [InlineData("""{ "name": "   " }""")]
    [InlineData("""{ "name": null }""")]
    [InlineData("""{ }""")]
    public async Task Create_WithoutName_Returns400(string json)
    {
        using var client = factory.CreateClient();

        var response = await client.PostAsync(
            "/lodges", new StringContent(json, Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Create_InvalidName_KeysErrorByJsonPropertyName(string? name)
    {
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/lodges", new { name });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Contains("name", problem.Errors.Keys);
    }

    [Theory]
    [InlineData(200, HttpStatusCode.Created)]
    [InlineData(201, HttpStatusCode.BadRequest)]
    public async Task Create_NameLength_IsLimitedTo200(int length, HttpStatusCode expected)
    {
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/lodges", new { name = TestNames.Unique(length) });

        Assert.Equal(expected, response.StatusCode);
    }

    [Fact]
    public async Task Create_TrimsName()
    {
        using var client = factory.CreateClient();

        var name = TestNames.Unique("Cabane de Moiry");

        var created = await CreateLodge(client, $"  {name} \t\n");
        var fetched = await client.GetFromJsonAsync<LodgeResponse>($"/lodges/{created.Id}");

        Assert.Equal(name, created.Name);
        Assert.Equal(name, fetched?.Name);
    }

    [Fact]
    public async Task Create_NameLength_IsCheckedAfterTrimming()
    {
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/lodges", new { name = $"  {TestNames.Unique(200)}  " });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Theory]
    [InlineData("{0}")]
    [InlineData("  {0}  ")]
    public async Task Create_DuplicateName_Returns409KeyedByName(string duplicateFormat)
    {
        using var client = factory.CreateClient();
        var name = TestNames.Unique("Monte Rosa Hut");
        await CreateLodge(client, name);

        var response = await client.PostAsJsonAsync(
            "/lodges", new { name = string.Format(CultureInfo.InvariantCulture, duplicateFormat, name) });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal((int)HttpStatusCode.Conflict, problem.Status);
        Assert.Contains("name", problem.Errors.Keys);
    }

    [Fact]
    public async Task Create_NameDifferingOnlyInCase_Returns409()
    {
        using var client = factory.CreateClient();
        var name = TestNames.Unique("Monte Rosa Hut");
        await CreateLodge(client, name);

        var response = await client.PostAsJsonAsync("/lodges", new { name = name.ToUpperInvariant() });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Create_NameDifferingOnlyInAccents_IsAllowed()
    {
        using var client = factory.CreateClient();
        var suffix = Guid.NewGuid().ToString("N");
        await CreateLodge(client, $"Refuge du Gouter {suffix}");

        var response = await client.PostAsJsonAsync("/lodges", new { name = $"Refuge du Goûter {suffix}" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task GetById_UnknownId_Returns404()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync($"/lodges/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetById_NonGuidId_Returns404()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/lodges/not-a-guid");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task List_IsSortedByName()
    {
        using var client = factory.CreateClient();
        // Created in alphabetical order, so sorting newest first would put them the other way round.
        var aiguille = await CreateLodge(client, TestNames.Unique("Aiguille Hut"));
        var zermatt = await CreateLodge(client, TestNames.Unique("Zermatt Hut"));

        var lodges = await client.GetFromJsonAsync<List<LodgeResponse>>("/lodges");

        Assert.NotNull(lodges);
        var zermattIndex = lodges.FindIndex(l => l.Id == zermatt.Id);
        var aiguilleIndex = lodges.FindIndex(l => l.Id == aiguille.Id);
        Assert.True(zermattIndex >= 0 && aiguilleIndex >= 0, "Both created lodges should be listed");
        Assert.True(aiguilleIndex < zermattIndex, "Lodges should be sorted by name");
    }

    private static async Task<LodgeResponse> CreateLodge(HttpClient client, string name)
    {
        var response = await client.PostAsJsonAsync("/lodges", new { name });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<LodgeResponse>())!;
    }
}
