using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using GLMS.API.Data;
using GLMS.API.Models;
using Xunit;
using Microsoft.AspNetCore.Hosting;

namespace GLMS.Tests.Integration;

// ── Test Factory ───────────────────────────────────────────
// Creates a test version of the API using an InMemory database

public class GlmsApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(
        Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        // This MUST be first - tells the app it is running in test mode
        // so Program.cs skips the SQL Server registration
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove the SQL Server DbContext options if somehow still present
            var dbContextDescriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<ApiDbContext>));
            if (dbContextDescriptor != null)
                services.Remove(dbContextDescriptor);

            // Remove the DbContext itself if present
            var dbDescriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(ApiDbContext));
            if (dbDescriptor != null)
                services.Remove(dbDescriptor);

            // Add InMemory database
            services.AddDbContext<ApiDbContext>(options =>
                options.UseInMemoryDatabase("SharedTestDb"));

            // Seed test data
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
            db.Database.EnsureCreated();
            SeedData(db);
        });
    }

    private static void SeedData(ApiDbContext db)
    {
        if (db.Clients.Any()) return;

        var client = new Client
        {
            Name = "Test Client Ltd",
            ContactEmail = "test@client.com",
            ContactPhone = "+27123456789",
            Region = "Gauteng"
        };
        db.Clients.Add(client);
        db.SaveChanges();

        db.Contracts.AddRange(
            new Contract
            {
                ClientId = client.Id,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddYears(1),
                Status = ContractStatus.Active,
                ServiceLevel = "Premium"
            },
            new Contract
            {
                ClientId = client.Id,
                StartDate = DateTime.Today.AddYears(-2),
                EndDate = DateTime.Today.AddYears(-1),
                Status = ContractStatus.Expired,
                ServiceLevel = "Standard"
            }
        );
        db.SaveChanges();
    }
}

// ── Shared JSON options used by all tests ──────────────────
// Putting this in a static class means both test classes can use it
public static class TestJsonOptions
{
    public static readonly JsonSerializerOptions Default = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };
}

// ── Contract Tests ─────────────────────────────────────────

public class ContractsApiTests : IClassFixture<GlmsApiFactory>
{
    private readonly HttpClient _client;

    public ContractsApiTests(GlmsApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    // Test 1: GET /api/contracts returns 200 OK
    [Fact]
    public async Task GetContracts_Returns200OK()
    {
        var response = await _client.GetAsync("/api/contracts");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // Test 2: GET /api/contracts returns non-null JSON
    [Fact]
    public async Task GetContracts_ReturnsNonNullJson()
    {
        var response = await _client.GetAsync("/api/contracts");
        var json = await response.Content.ReadAsStringAsync();
        Assert.NotNull(json);
        Assert.NotEmpty(json);
    }

    // Test 3: GET /api/contracts returns at least 2 contracts
    [Fact]
    public async Task GetContracts_ReturnsAtLeastTwoContracts()
    {
        var response = await _client.GetAsync("/api/contracts");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        var contracts = JsonSerializer.Deserialize<List<Contract>>(
            json, TestJsonOptions.Default);
        Assert.NotNull(contracts);
        Assert.True(contracts.Count >= 2);
    }

    // Test 4: GET /api/contracts/{id} returns 200 for valid ID
    [Fact]
    public async Task GetContractById_ValidId_Returns200()
    {
        var response = await _client.GetAsync("/api/contracts");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        var contracts = JsonSerializer.Deserialize<List<Contract>>(
            json, TestJsonOptions.Default);
        Assert.NotNull(contracts);
        Assert.NotEmpty(contracts);

        var firstId = contracts.First().Id;
        var detailResponse = await _client.GetAsync($"/api/contracts/{firstId}");
        Assert.Equal(HttpStatusCode.OK, detailResponse.StatusCode);
    }

    // Test 5: GET /api/contracts/{id} returns 404 for missing ID
    [Fact]
    public async Task GetContractById_InvalidId_Returns404()
    {
        var response = await _client.GetAsync("/api/contracts/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // Test 6: POST /api/contracts creates a contract and returns 201
    [Fact]
    public async Task PostContract_ValidData_Returns201()
    {
        // We know the seeded client has ID 1 so we use it directly
        // This avoids a dependency on the /api/clients endpoint
        var dto = new
        {
            ClientId = 1,
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddYears(1),
            Status = "Draft",
            ServiceLevel = "Gold"
        };

        var json = JsonSerializer.Serialize(dto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/contracts", content);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    // Test 7: PATCH /api/contracts/{id}/status updates status
    [Fact]
    public async Task PatchContractStatus_ValidId_Returns200()
    {
        var response = await _client.GetAsync("/api/contracts");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        var contracts = JsonSerializer.Deserialize<List<Contract>>(
            json, TestJsonOptions.Default);
        Assert.NotNull(contracts);
        Assert.NotEmpty(contracts);

        var id = contracts.First().Id;
        var dto = new { Status = "Active" };
        var content = new StringContent(
            JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(
            HttpMethod.Patch, $"/api/contracts/{id}/status")
        {
            Content = content
        };

        var patchResponse = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, patchResponse.StatusCode);
    }

    // Test 8: Filtering by Expired status only returns Expired contracts
    [Fact]
    public async Task GetContracts_FilterExpired_ReturnsOnlyExpired()
    {
        var response = await _client.GetAsync("/api/contracts?status=Expired");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        var contracts = JsonSerializer.Deserialize<List<Contract>>(
            json, TestJsonOptions.Default);
        Assert.NotNull(contracts);
        Assert.All(contracts, c =>
            Assert.Equal(ContractStatus.Expired, c.Status));
    }
}

// ── Service Request Tests ──────────────────────────────────

public class ServiceRequestsApiTests : IClassFixture<GlmsApiFactory>
{
    private readonly HttpClient _client;

    public ServiceRequestsApiTests(GlmsApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    // Test 9: GET /api/servicerequests returns 200
    [Fact]
    public async Task GetServiceRequests_Returns200()
    {
        var response = await _client.GetAsync("/api/servicerequests");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // Test 10: Cannot create a service request on an Expired contract
    [Fact]
    public async Task PostServiceRequest_OnExpiredContract_Returns422()
    {
        var response = await _client.GetAsync("/api/contracts?status=Expired");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        var contracts = JsonSerializer.Deserialize<List<Contract>>(
            json, TestJsonOptions.Default);
        Assert.NotNull(contracts);
        Assert.NotEmpty(contracts);

        var dto = new
        {
            ContractId = contracts.First().Id,
            Description = "Test on expired contract",
            USDValue = 100.00,
            ZARCost = 1850.00
        };

        var content = new StringContent(
            JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
        var postResponse = await _client.PostAsync("/api/servicerequests", content);

        // 422 = business rule violated (Expired contract)
        Assert.Equal(HttpStatusCode.UnprocessableEntity, postResponse.StatusCode);
    }
}