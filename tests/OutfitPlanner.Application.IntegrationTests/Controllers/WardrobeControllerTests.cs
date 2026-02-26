using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OutfitPlanner.Application.DTOs.Wardrobe;
using OutfitPlanner.Application.Models.Identity;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Persistence;
using Xunit;

namespace OutfitPlanner.Application.IntegrationTests.Controllers;

[Collection("Sequential")]
public class WardrobeControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly string _testEmail = "testuser@example.com";
    private readonly string _testPassword = "Test@123";
    private string _jwtToken = string.Empty;

    public WardrobeControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                var dbConnectionDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(System.Data.Common.DbConnection));

                if (dbConnectionDescriptor != null)
                {
                    services.Remove(dbConnectionDescriptor);
                }

                // Create open SqliteConnection so the memory database doesn't disappear
                services.AddSingleton<System.Data.Common.DbConnection>(container =>
                {
                    var connection = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
                    connection.Open();
                    return connection;
                });

                services.AddDbContext<AppDbContext>((container, options) =>
                {
                    var connection = container.GetRequiredService<System.Data.Common.DbConnection>();
                    options.UseSqlite(connection);
                });
            });
        });
        
        _client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var sp = scope.ServiceProvider;
        var db = sp.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();
    }

    [Fact]
    public async Task RegisterTestUser()
    {
        var registerRequest = new RegistrationRequest
        {
            Email = _testEmail,
            Password = _testPassword,
            UserName = "testuser",
            FirstName = "Test",
            LastName = "User"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Registration failed with status {response.StatusCode}: {errorContent}");
        }

        var registerResponse = await response.Content.ReadFromJsonAsync<RegistrationResponse>();
        registerResponse.Should().NotBeNull();
        registerResponse.UserId.Should().NotBeNullOrEmpty();
        registerResponse.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task LoginTestUser()
    {
        await RegisterTestUser();

        var loginRequest = new AuthRequest
        {
            Email = _testEmail,
            Password = _testPassword
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        response.EnsureSuccessStatusCode();

        var loginResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        loginResponse.Should().NotBeNull();
        loginResponse.Token.Should().NotBeNullOrEmpty();

        _jwtToken = loginResponse.Token!;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _jwtToken);
    }

    [Fact]
    public async Task GetAllClothingItems_WithoutAuthentication_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/wardrobe");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAllClothingItems_WithAuthentication_ReturnsOk()
    {
        await LoginTestUser();

        var response = await _client.GetAsync("/api/wardrobe");
        response.EnsureSuccessStatusCode();
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateClothingItem_WithValidData_ReturnsCreated()
    {
        await LoginTestUser();

        var createRequest = new CreateClothingItemDto
        {
            Name = "Test T-Shirt",
            Type = "Top",
            Category = "Casual",
            PrimaryColor = "Blue",
            Size = "M",
            Brand = "Test Brand",
            Fabric = "Cotton",
            PurchasePrice = 29.99M,
            Currency = "USD"
        };

        var response = await _client.PostAsJsonAsync("/api/wardrobe", createRequest);
        response.EnsureSuccessStatusCode();
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("Test T-Shirt");
    }

    [Fact]
    public async Task CreateClothingItem_WithoutName_ReturnsBadRequest()
    {
        await LoginTestUser();

        var createRequest = new CreateClothingItemDto
        {
            Type = "Top",
            Category = "Casual",
            PrimaryColor = "Blue",
            Size = "M",
            Brand = "Test Brand",
            Fabric = "Cotton",
            PurchasePrice = 29.99M,
            Currency = "USD"
        };

        var response = await _client.PostAsJsonAsync("/api/wardrobe", createRequest);
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetClothingItemById_WithValidId_ReturnsOk()
    {
        await LoginTestUser();

        var createRequest = new CreateClothingItemDto
        {
            Name = "Get Test Item",
            Type = "Bottom",
            Category = "Formal",
            PrimaryColor = "Black",
            Size = "L",
            Brand = "Test Brand",
            Fabric = "Wool",
            PurchasePrice = 49.99M,
            Currency = "USD"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/wardrobe", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var responseContent = await createResponse.Content.ReadAsStringAsync();
        
        var id = ExtractIdFromResponse(responseContent);
        
        var getResponse = await _client.GetAsync($"/api/wardrobe/{id}");
        getResponse.EnsureSuccessStatusCode();
        getResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetClothingItemById_WithInvalidId_ReturnsNotFound()
    {
        await LoginTestUser();

        var invalidId = Guid.NewGuid();
        var response = await _client.GetAsync($"/api/wardrobe/{invalidId}");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateClothingItem_WithValidData_ReturnsOk()
    {
        await LoginTestUser();

        var createRequest = new CreateClothingItemDto
        {
            Name = "Update Test Item",
            Type = "Dress",
            Category = "Formal",
            PrimaryColor = "Red",
            Size = "S",
            Brand = "Test Brand",
            Fabric = "Silk",
            PurchasePrice = 99.99M,
            Currency = "USD"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/wardrobe", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var responseContent = await createResponse.Content.ReadAsStringAsync();
        var id = ExtractIdFromResponse(responseContent);

        var updateRequest = new UpdateClothingItemDto
        {
            Name = "Updated Test Item",
            Type = "Dress",
            Category = "Formal",
            PrimaryColor = "Blue",
            Size = "S",
            Brand = "Test Brand",
            Fabric = "Silk",
            PurchasePrice = 109.99M,
            Currency = "USD"
        };

        var updateResponse = await _client.PutAsJsonAsync($"/api/wardrobe/{id}", updateRequest);
        updateResponse.EnsureSuccessStatusCode();
        updateResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteClothingItem_WithValidId_ReturnsOk()
    {
        await LoginTestUser();

        var createRequest = new CreateClothingItemDto
        {
            Name = "Delete Test Item",
            Type = "Shoes",
            Category = "Casual",
            PrimaryColor = "Brown",
            Size = "10",
            Brand = "Test Brand",
            Fabric = "Leather",
            PurchasePrice = 79.99M,
            Currency = "USD"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/wardrobe", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var responseContent = await createResponse.Content.ReadAsStringAsync();
        var id = ExtractIdFromResponse(responseContent);

        var deleteResponse = await _client.DeleteAsync($"/api/wardrobe/{id}");
        deleteResponse.EnsureSuccessStatusCode();
        deleteResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var getResponse = await _client.GetAsync($"/api/wardrobe/{id}");
        getResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RecordWear_WithValidData_ReturnsOk()
    {
        await LoginTestUser();

        var createRequest = new CreateClothingItemDto
        {
            Name = "Wear Test Item",
            Type = "Accessory",
            Category = "Casual",
            PrimaryColor = "Silver",
            Size = "One Size",
            Brand = "Test Brand",
            Fabric = "Metal",
            PurchasePrice = 24.99M,
            Currency = "USD"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/wardrobe", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var responseContent = await createResponse.Content.ReadAsStringAsync();
        var id = ExtractIdFromResponse(responseContent);

        var recordWearRequest = new RecordWearDto
        {
            ClothingItemId = id,
            WornAt = DateTime.Now.AddDays(-1),
            DurationMinutes = 120,
            WeatherCondition = "Sunny",
            Rating = 4
        };

        var recordWearResponse = await _client.PostAsJsonAsync($"/api/wardrobe/{id}/wear", recordWearRequest);
        recordWearResponse.EnsureSuccessStatusCode();
        recordWearResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task QuickWear_WithValidId_ReturnsOk()
    {
        await LoginTestUser();

        var createRequest = new CreateClothingItemDto
        {
            Name = "Quick Wear Test Item",
            Type = "Jacket",
            Category = "Outdoor",
            PrimaryColor = "Green",
            Size = "L",
            Brand = "Test Brand",
            Fabric = "Nylon",
            PurchasePrice = 89.99M,
            Currency = "USD"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/wardrobe", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var responseContent = await createResponse.Content.ReadAsStringAsync();
        var id = ExtractIdFromResponse(responseContent);

        var quickWearResponse = await _client.PostAsync($"/api/wardrobe/{id}/wear/quick", null);
        quickWearResponse.EnsureSuccessStatusCode();
        quickWearResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetClothingItemsByCategory_WithValidCategory_ReturnsOk()
    {
        await LoginTestUser();

        var createRequest = new CreateClothingItemDto
        {
            Name = "Category Test Item",
            Type = "Top",
            Category = "Workout",
            PrimaryColor = "Black",
            Size = "M",
            Brand = "Test Brand",
            Fabric = "Polyester",
            PurchasePrice = 39.99M,
            Currency = "USD"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/wardrobe", createRequest);
        createResponse.EnsureSuccessStatusCode();

        var getResponse = await _client.GetAsync($"/api/wardrobe/category/Workout");
        getResponse.EnsureSuccessStatusCode();
        getResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetClothingItemsByCategory_WithInvalidCategory_ReturnsOk_WithEmptyList()
    {
        await LoginTestUser();

        var getResponse = await _client.GetAsync($"/api/wardrobe/category/NonExistentCategory");
        getResponse.EnsureSuccessStatusCode();
        getResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var content = await getResponse.Content.ReadAsStringAsync();
        content.Should().Contain("[]");
    }

    private Guid ExtractIdFromResponse(string responseContent)
    {
        const string idPrefix = "\"Id\":\"";
        var idIndex = responseContent.IndexOf(idPrefix);
        if (idIndex < 0)
            throw new Exception("Id not found in response");

        idIndex += idPrefix.Length;
        var idLength = 36; // GUID length
        var idStr = responseContent.Substring(idIndex, idLength);
        return Guid.Parse(idStr);
    }
}
