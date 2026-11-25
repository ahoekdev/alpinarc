using System.Net;
using System.Net.Http.Json;
using Alpinarc.Api.Tours;
using Microsoft.AspNetCore.Mvc;

namespace Alpinarc.Api.Tests;

// The database is shared by every test in the run, so no test may assume the tours table is empty.
[Collection(ApiCollection.Name)]
public class ToursEndpointTests(ApiFactory factory)
{
    [Fact]
    public async Task Create_ThenGetById_ReturnsStagesInRequestOrder_WithTotals()
    {
        using var client = factory.CreateClient();
        var first = await TestData.CreateStage(client, 8200, 650, 1320, 210);
        var second = await TestData.CreateStage(client, 5000, 900, 100, 180);
        var third = await TestData.CreateStage(client, 12000, 400, 400, 240);
        var name = TestNames.Unique("Zillertal Höhenweg");

        // Deliberately not in creation order.
        var createResponse = await client.PostAsJsonAsync("/tours", new
        {
            name,
            description = "Five days through the Zillertal Alps.",
            stageIds = new[] { second.Id, third.Id, first.Id },
        }, TestRun.Token);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<TourResponse>(TestRun.Token);
        Assert.NotNull(created);
        Assert.Equal(name, created.Name);
        Assert.Equal("Five days through the Zillertal Alps.", created.Description);
        Assert.Equal($"/tours/{created.Id}", createResponse.Headers.Location?.AbsolutePath);

        var fetched = await client.GetFromJsonAsync<TourResponse>(createResponse.Headers.Location, TestRun.Token);

        Assert.NotNull(fetched);
        foreach (var tour in new[] { created, fetched })
        {
            Assert.Equal([1, 2, 3], tour.Stages.Select(s => s.Position));
            Assert.Equal([second, third, first], tour.Stages.Select(s => s.Stage));
            Assert.Equal(new TourTotals(25200, 1950, 1820, 630), tour.Totals);
        }

        Assert.Equal(created.Id, fetched.Id);
        Assert.Equal(created.CreatedAt, fetched.CreatedAt);
    }

    [Fact]
    public async Task Create_WithoutStages_ReturnsEmptyTourWithZeroTotals()
    {
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/tours", new { name = TestNames.Unique("Empty"), stageIds = Array.Empty<Guid>() }, TestRun.Token);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<TourResponse>(TestRun.Token);
        Assert.NotNull(created);
        var fetched = await client.GetFromJsonAsync<TourResponse>($"/tours/{created.Id}", TestRun.Token);
        Assert.NotNull(fetched);
        Assert.Empty(fetched.Stages);
        Assert.Equal(new TourTotals(0, 0, 0, 0), fetched.Totals);
        Assert.Null(fetched.Description);
    }

    [Fact]
    public async Task Create_StagesThatDontConnect_AreAccepted()
    {
        using var client = factory.CreateClient();
        // Each stage is between two new lodges, so the first doesn't end where the second starts.
        var first = await TestData.CreateStage(client);
        var second = await TestData.CreateStage(client);

        var tour = await TestData.CreateTour(client, first, second);

        Assert.NotEqual(first.ToLodge.Id, second.FromLodge.Id);
        Assert.Equal(2, tour.Stages.Count);
    }

    [Fact]
    public async Task Create_SameStageTwice_IsAllowed()
    {
        using var client = factory.CreateClient();
        var stage = await TestData.CreateStage(client, 1000, 100, 50, 30);

        var tour = await TestData.CreateTour(client, stage, stage);

        Assert.Equal([stage.Id, stage.Id], tour.Stages.Select(s => s.Stage.Id));
        Assert.Equal(new TourTotals(2000, 200, 100, 60), tour.Totals);
    }

    [Fact]
    public async Task TwoTours_CanShareAStage()
    {
        using var client = factory.CreateClient();
        var shared = await TestData.CreateStage(client);
        var other = await TestData.CreateStage(client);

        var first = await TestData.CreateTour(client, shared);
        var second = await TestData.CreateTour(client, other, shared);

        Assert.Equal(shared.Id, first.Stages[0].Stage.Id);
        Assert.Equal(shared.Id, second.Stages[1].Stage.Id);
    }

    [Fact]
    public async Task Create_UnknownStageId_Returns400KeyedByIndex()
    {
        using var client = factory.CreateClient();
        var stage = await TestData.CreateStage(client);

        var response = await client.PostAsJsonAsync("/tours", new
        {
            name = TestNames.Unique("Tour"),
            stageIds = new[] { stage.Id, Guid.NewGuid(), stage.Id, Guid.NewGuid() },
        }, TestRun.Token);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestRun.Token);
        Assert.NotNull(problem);
        Assert.Equal(["stageIds[1]", "stageIds[3]"], problem.Errors.Keys.Order());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Create_WithoutName_Returns400KeyedByName(string? name)
    {
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/tours", new { name, stageIds = Array.Empty<Guid>() }, TestRun.Token);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestRun.Token);
        Assert.NotNull(problem);
        Assert.Contains("name", problem.Errors.Keys);
    }

    [Fact]
    public async Task Create_WithoutStageIds_Returns400()
    {
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/tours", new { name = TestNames.Unique("Tour") }, TestRun.Token);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData(200, HttpStatusCode.Created)]
    [InlineData(201, HttpStatusCode.BadRequest)]
    public async Task Create_NameLength_IsLimitedTo200(int length, HttpStatusCode expected)
    {
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/tours", new { name = TestNames.Unique(length), stageIds = Array.Empty<Guid>() }, TestRun.Token);

        Assert.Equal(expected, response.StatusCode);
    }

    [Theory]
    [InlineData(2000, HttpStatusCode.Created)]
    [InlineData(2001, HttpStatusCode.BadRequest)]
    public async Task Create_DescriptionLength_IsLimitedTo2000(int length, HttpStatusCode expected)
    {
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/tours", new
        {
            name = TestNames.Unique("Tour"),
            description = new string('a', length),
            stageIds = Array.Empty<Guid>(),
        }, TestRun.Token);

        Assert.Equal(expected, response.StatusCode);
    }

    [Theory]
    [InlineData("  A long day.  ", "A long day.")]
    [InlineData("   ", null)]
    [InlineData("", null)]
    public async Task Create_TrimsDescription_AndStoresBlankAsNull(string description, string? expected)
    {
        using var client = factory.CreateClient();
        var name = TestNames.Unique("Tour");

        var response = await client.PostAsJsonAsync(
            "/tours", new { name = $"  {name} ", description, stageIds = Array.Empty<Guid>() }, TestRun.Token);

        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<TourResponse>(TestRun.Token);
        var fetched = await client.GetFromJsonAsync<TourResponse>($"/tours/{created!.Id}", TestRun.Token);
        Assert.Equal(name, fetched?.Name);
        Assert.Equal(expected, fetched?.Description);
    }

    [Fact]
    public async Task GetById_UnknownId_Returns404()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync($"/tours/{Guid.NewGuid()}", TestRun.Token);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task List_ShowsCreatedTour_WithStageCountAndTotals()
    {
        using var client = factory.CreateClient();
        var first = await TestData.CreateStage(client, 8200, 650, 1320, 210);
        var second = await TestData.CreateStage(client, 5000, 900, 100, 180);
        var tour = await TestData.CreateTour(client, first, second, first);
        var empty = await TestData.CreateTour(client);

        var tours = await client.GetFromJsonAsync<List<TourSummaryResponse>>("/tours", TestRun.Token);

        Assert.NotNull(tours);
        var listed = Assert.Single(tours, t => t.Id == tour.Id);
        Assert.Equal(tour.Name, listed.Name);
        Assert.Equal(3, listed.StageCount);
        Assert.Equal(new TourTotals(21400, 2200, 2740, 600), listed.Totals);
        Assert.Equal(tour.Totals, listed.Totals);

        var listedEmpty = Assert.Single(tours, t => t.Id == empty.Id);
        Assert.Equal(0, listedEmpty.StageCount);
        Assert.Equal(new TourTotals(0, 0, 0, 0), listedEmpty.Totals);
    }

    [Fact]
    public async Task List_IsSortedByName()
    {
        using var client = factory.CreateClient();
        var suffix = Guid.NewGuid().ToString("N");
        var zillertal = await PostTour(client, $"Zillertal {suffix}");
        var alta = await PostTour(client, $"Alta Via {suffix}");

        var tours = await client.GetFromJsonAsync<List<TourSummaryResponse>>("/tours", TestRun.Token);

        Assert.NotNull(tours);
        var ids = tours.Select(t => t.Id).ToList();
        Assert.True(ids.IndexOf(alta.Id) < ids.IndexOf(zillertal.Id), "Tours should be sorted by name");
    }

    private static async Task<TourResponse> PostTour(HttpClient client, string name)
    {
        var response = await client.PostAsJsonAsync("/tours", new { name, stageIds = Array.Empty<Guid>() }, TestRun.Token);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TourResponse>(TestRun.Token))!;
    }
}
