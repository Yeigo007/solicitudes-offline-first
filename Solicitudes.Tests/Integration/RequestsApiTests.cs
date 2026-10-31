using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Solicitudes.Infrastructure.Persistence;
using Xunit;

namespace Solicitudes.Tests.Integration;

[Collection(ApiTestCollection.Name)]
public sealed class RequestsApiTests : IAsyncLifetime
{
    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    public RequestsApiTests(ApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public Task InitializeAsync() => _factory.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private static object NewRequest(string? type = "TEXT", string? payload = " hola mundo ")
        => new
        {
            clientRequestId = Guid.NewGuid(),
            name = "Prueba integración",
            payload,
            type
        };

    [Fact]
    public async Task Post_Then_GetById_PersistsRequestInPostgres()
    {
        var response = await _client.PostAsJsonAsync("/api/requests", NewRequest());
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var id = json.RootElement.GetProperty("id").GetGuid();

        var get = await _client.GetAsync($"/api/requests/{id}");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);

        using var result = JsonDocument.Parse(await get.Content.ReadAsStringAsync());
        Assert.Equal("Prueba integración", result.RootElement.GetProperty("name").GetString());

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SolicitudesDbContext>();
        Assert.True(await db.Requests.AnyAsync(x => x.Id == id));
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenRequestDoesNotExist()
    {
        var response = await _client.GetAsync($"/api/requests/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Post_ReturnsBadRequest_WhenNameIsEmpty()
    {
        var request = new
        {
            clientRequestId = Guid.NewGuid(),
            name = "",
            payload = "contenido",
            type = "TEXT"
        };

        var response = await _client.PostAsJsonAsync("/api/requests", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_IsIdempotent_ForSameClientRequestId()
    {
        var clientRequestId = Guid.NewGuid();
        var request = new
        {
            clientRequestId,
            name = "Idempotente",
            payload = "contenido",
            type = "TEXT"
        };

        var first = await _client.PostAsJsonAsync("/api/requests", request);
        var second = await _client.PostAsJsonAsync("/api/requests", request);

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Created, second.StatusCode);

        using var firstJson = JsonDocument.Parse(await first.Content.ReadAsStringAsync());
        using var secondJson = JsonDocument.Parse(await second.Content.ReadAsStringAsync());
        Assert.Equal(
            firstJson.RootElement.GetProperty("id").GetGuid(),
            secondJson.RootElement.GetProperty("id").GetGuid());

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SolicitudesDbContext>();
        Assert.Equal(1, await db.Requests.CountAsync(x => x.ClientRequestId == clientRequestId));
    }

    [Fact]
    public async Task Process_TransformsText_AndPersistsProcessedStatus()
    {
        var create = await _client.PostAsJsonAsync("/api/requests", NewRequest());
        create.EnsureSuccessStatusCode();

        using var created = JsonDocument.Parse(await create.Content.ReadAsStringAsync());
        var id = created.RootElement.GetProperty("id").GetGuid();

        var process = await _client.PostAsync($"/api/requests/{id}/process", null);
        Assert.Equal(HttpStatusCode.OK, process.StatusCode);

        using var processed = JsonDocument.Parse(await process.Content.ReadAsStringAsync());
        Assert.Equal(1, processed.RootElement.GetProperty("status").GetInt32());
        Assert.Equal("HOLA MUNDO", processed.RootElement.GetProperty("processedPayload").GetString());
    }

    [Fact]
    public async Task ProcessPending_ProcessesPendingRequests()
    {
        await _client.PostAsJsonAsync("/api/requests", NewRequest("TEXT", " uno "));
        await _client.PostAsJsonAsync("/api/requests", NewRequest("TEXT", " dos "));

        var response = await _client.PostAsync("/api/requests/process-pending", null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(2, json.RootElement.GetProperty("total").GetInt32());
        Assert.Equal(2, json.RootElement.GetProperty("processed").GetInt32());
        Assert.Equal(0, json.RootElement.GetProperty("failed").GetInt32());
    }
}
