using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OutfitPlanner.Application.DTOs.Outfit;
using OutfitPlanner.Application.DTOs.Wardrobe;
using OutfitPlanner.Application.Models.Identity;
using OutfitPlanner.Persistence;
using Xunit;

namespace OutfitPlanner.Application.IntegrationTests.Controllers;

[Collection("Sequential")]
public class OutfitControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly string _testEmail = "testoutfituser@example.com";
    private readonly string _testPassword = "Test@123";
    private string _jwtToken = string.Empty;

    public OutfitControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove all DbContext-related registrations
                var descriptorsToRemove = services.Where(d => 
                    d.ServiceType.Name.Contains("DbContext") ||
                    d.ServiceType.Name.Contains("DbOptions") ||
                    d.ServiceType == typeof(Microsoft.Extensions.Options.IOptionsFactory<Microsoft.EntityFrameworkCore.DbContextOptions<AppDbContext>>) ||
                    d.ServiceType == typeof(Microsoft.EntityFrameworkCore.Infrastructure.IDbContextOptionsConfiguration<AppDbContext>)
                ).ToList();

                foreach (var descriptor in descriptorsToRemove)
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
            UserName = "testoutfituser",
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
        registerResponse!.UserId.Should().NotBeNullOrEmpty();
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
        loginResponse!.Token.Should().NotBeNullOrEmpty();

        _jwtToken = loginResponse.Token!;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _jwtToken);
    }

    [Fact]
    public async Task GetAllOutfits_WithoutAuthentication_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/outfits");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAllOutfits_WithAuthentication_ReturnsOk()
    {
        await LoginTestUser();

        var response = await _client.GetAsync("/api/outfits");
        response.EnsureSuccessStatusCode();
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateOutfit_WithValidData_ReturnsCreated()
    {
        await LoginTestUser();

        // First create a clothing item to use in the outfit
        var clothingItemId = await CreateTestClothingItem("Test Shirt for Outfit");

        var createRequest = new CreateOutfitDto
        {
            Name = "Casual Weekend Outfit",
            Occasion = "Casual",
            WeatherCondition = "Sunny",
            Season = "Summer",
            Items = new List<CreateOutfitItemDto>
            {
                new()
                {
                    ClothingItemId = clothingItemId,
                    Role = "Primary",
                    LayeringOrder = 1,
                    IsEssential = true
                }
            }
        };

        var response = await _client.PostAsJsonAsync("/api/outfits", createRequest);
        response.EnsureSuccessStatusCode();
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("Casual Weekend Outfit");
    }

    [Fact]
    public async Task CreateOutfit_WithoutItems_ReturnsBadRequest()
    {
        await LoginTestUser();

        var createRequest = new CreateOutfitDto
        {
            Name = "Invalid Outfit",
            Occasion = "Casual",
            WeatherCondition = "Sunny",
            Season = "Summer",
            Items = new List<CreateOutfitItemDto>() // Empty items list
        };

        var response = await _client.PostAsJsonAsync("/api/outfits", createRequest);
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetOutfitById_WithValidId_ReturnsOk()
    {
        await LoginTestUser();

        // Create an outfit first
        var clothingItemId = await CreateTestClothingItem("Test Shirt for GetById");
        var outfitId = await CreateTestOutfit("GetById Test Outfit", clothingItemId);

        // Get the outfit by ID
        var getResponse = await _client.GetAsync($"/api/outfits/{outfitId}");
        getResponse.EnsureSuccessStatusCode();
        getResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var outfit = await getResponse.Content.ReadFromJsonAsync<OutfitDto>();
        outfit.Should().NotBeNull();
        outfit!.Name.Should().Be("GetById Test Outfit");
    }

    [Fact]
    public async Task GetOutfitById_WithInvalidId_ReturnsNotFound()
    {
        await LoginTestUser();

        var invalidId = Guid.NewGuid();
        var response = await _client.GetAsync($"/api/outfits/{invalidId}");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateOutfit_WithValidData_ReturnsOk()
    {
        await LoginTestUser();

        // Create an outfit first
        var clothingItemId = await CreateTestClothingItem("Test Shirt for Update");
        var outfitId = await CreateTestOutfit("Original Outfit Name", clothingItemId);

        // Update the outfit
        var updateRequest = new UpdateOutfitDto
        {
            Name = "Updated Outfit Name",
            Occasion = "Formal",
            WeatherCondition = "Cloudy",
            Season = "Winter",
            ComfortRating = 5,
            StyleRating = 4
        };

        var updateResponse = await _client.PutAsJsonAsync($"/api/outfits/{outfitId}", updateRequest);
        updateResponse.EnsureSuccessStatusCode();
        updateResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        // Verify the update
        var getResponse = await _client.GetAsync($"/api/outfits/{outfitId}");
        var outfit = await getResponse.Content.ReadFromJsonAsync<OutfitDto>();
        outfit.Should().NotBeNull();
        outfit!.Name.Should().Be("Updated Outfit Name");
        outfit.Occasion.Should().Be("Formal");
    }

    [Fact]
    public async Task DeleteOutfit_WithValidId_ReturnsNoContent()
    {
        await LoginTestUser();

        // Create an outfit first
        var clothingItemId = await CreateTestClothingItem("Test Shirt for Delete");
        var outfitId = await CreateTestOutfit("Outfit to Delete", clothingItemId);

        // Delete the outfit
        var deleteResponse = await _client.DeleteAsync($"/api/outfits/{outfitId}");
        deleteResponse.EnsureSuccessStatusCode();
        deleteResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);

        // Verify the outfit is no longer accessible (soft delete verification)
        var getResponse = await _client.GetAsync($"/api/outfits/{outfitId}");
        getResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task FullOutfitLifecycle_CreateGetUpdateDeleteVerifySoftDelete()
    {
        await LoginTestUser();

        // Step 1: Create a clothing item (required for outfit)
        var clothingItemId = await CreateTestClothingItem("Lifecycle Test Shirt");

        // Step 2: Create an outfit
        var createRequest = new CreateOutfitDto
        {
            Name = "Lifecycle Test Outfit",
            Occasion = "Casual",
            WeatherCondition = "Sunny",
            Season = "Summer",
            Items = new List<CreateOutfitItemDto>
            {
                new()
                {
                    ClothingItemId = clothingItemId,
                    Role = "Primary",
                    LayeringOrder = 1,
                    IsEssential = true
                }
            }
        };

        var createResponse = await _client.PostAsJsonAsync("/api/outfits", createRequest);
        createResponse.EnsureSuccessStatusCode();
        createResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);

        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        var outfitId = ExtractIdFromResponse(createResponseContent);
        outfitId.Should().NotBe(Guid.Empty);

        // Step 3: Get the outfit and verify it exists
        var getResponse = await _client.GetAsync($"/api/outfits/{outfitId}");
        getResponse.EnsureSuccessStatusCode();
        getResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var outfit = await getResponse.Content.ReadFromJsonAsync<OutfitDto>();
        outfit.Should().NotBeNull();
        outfit!.Name.Should().Be("Lifecycle Test Outfit");
        outfit.Occasion.Should().Be("Casual");

        // Step 4: Update the outfit
        var updateRequest = new UpdateOutfitDto
        {
            Name = "Updated Lifecycle Outfit",
            Occasion = "BusinessCasual",
            WeatherCondition = "Cloudy",
            Season = "Spring",
            ComfortRating = 4,
            StyleRating = 5
        };

        var updateResponse = await _client.PutAsJsonAsync($"/api/outfits/{outfitId}", updateRequest);
        updateResponse.EnsureSuccessStatusCode();
        updateResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        // Verify the update was applied
        getResponse = await _client.GetAsync($"/api/outfits/{outfitId}");
        outfit = await getResponse.Content.ReadFromJsonAsync<OutfitDto>();
        outfit.Should().NotBeNull();
        outfit!.Name.Should().Be("Updated Lifecycle Outfit");
        outfit.Occasion.Should().Be("BusinessCasual");
        outfit.ComfortRating.Should().Be(4);
        outfit.StyleRating.Should().Be(5);

        // Step 5: Delete the outfit
        var deleteResponse = await _client.DeleteAsync($"/api/outfits/{outfitId}");
        deleteResponse.EnsureSuccessStatusCode();
        deleteResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);

        // Step 6: Verify soft delete - outfit should not be found
        getResponse = await _client.GetAsync($"/api/outfits/{outfitId}");
        getResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);

        // Also verify outfit doesn't appear in the list
        var listResponse = await _client.GetAsync("/api/outfits");
        listResponse.EnsureSuccessStatusCode();
        var outfits = await listResponse.Content.ReadFromJsonAsync<List<OutfitListDto>>();
        outfits.Should().NotBeNull();
        outfits!.Any(o => o.Id == outfitId).Should().BeFalse("deleted outfit should not appear in the list");
    }

    [Fact]
    public async Task RecordWear_WithValidData_ReturnsOk()
    {
        await LoginTestUser();

        // Create an outfit first
        var clothingItemId = await CreateTestClothingItem("Test Shirt for Wear");
        var outfitId = await CreateTestOutfit("Outfit for Wear Test", clothingItemId);

        var recordWearDto = new RecordOutfitWearDto
        {
            WornAt = DateTime.UtcNow,
            WeatherCondition = "Sunny"
        };

        var response = await _client.PostAsJsonAsync($"/api/outfits/{outfitId}/wear", recordWearDto);
        response.EnsureSuccessStatusCode();
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task GenerateSuggestions_WithValidCriteria_ReturnsOk()
    {
        await LoginTestUser();

        // Create clothing items and an outfit
        var clothingItemId = await CreateTestClothingItem("Test Shirt for Suggestions");
        await CreateTestOutfit("Outfit for Suggestions", clothingItemId);

        var suggestionsDto = new OutfitSuggestionsDto
        {
            Occasion = "Casual",
            WeatherCondition = "Sunny",
            Season = "Summer",
            MaxSuggestions = 3
        };

        var response = await _client.PostAsJsonAsync("/api/outfits/generate", suggestionsDto);
        response.EnsureSuccessStatusCode();
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var suggestions = await response.Content.ReadFromJsonAsync<List<OutfitDto>>();
        suggestions.Should().NotBeNull();
    }

    #region Helper Methods

    private async Task<Guid> CreateTestClothingItem(string name)
    {
        // Create multipart form content since WardrobeController expects [FromForm]
        var content = new MultipartFormDataContent();
        content.Add(new StringContent(name), "Name");
        content.Add(new StringContent("Top"), "Type");
        content.Add(new StringContent("Casual"), "Category");
        content.Add(new StringContent("Blue"), "PrimaryColor");
        content.Add(new StringContent("M"), "Size");
        content.Add(new StringContent("Test Brand"), "Brand");
        content.Add(new StringContent("Cotton"), "Fabric");
        content.Add(new StringContent("29.99"), "PurchasePrice");
        content.Add(new StringContent("USD"), "Currency");

        var response = await _client.PostAsync("/api/wardrobe", content);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        return ExtractIdFromResponse(responseContent);
    }

    private async Task<Guid> CreateTestOutfit(string name, Guid clothingItemId)
    {
        var createRequest = new CreateOutfitDto
        {
            Name = name,
            Occasion = "Casual",
            WeatherCondition = "Sunny",
            Season = "Summer",
            Items = new List<CreateOutfitItemDto>
            {
                new()
                {
                    ClothingItemId = clothingItemId,
                    Role = "Primary",
                    LayeringOrder = 1,
                    IsEssential = true
                }
            }
        };

        var response = await _client.PostAsJsonAsync("/api/outfits", createRequest);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        return ExtractIdFromResponse(responseContent);
    }

    private Guid ExtractIdFromResponse(string responseContent)
    {
        try
        {
            // Try to deserialize as OutfitDto first
            var outfit = System.Text.Json.JsonSerializer.Deserialize<OutfitDto>(responseContent, 
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (outfit?.Id != Guid.Empty)
                return outfit.Id;
        }
        catch { }

        try
        {
            // Try to deserialize as a generic object with Id property
            using var doc = System.Text.Json.JsonDocument.Parse(responseContent);
            var root = doc.RootElement;
            
            if (root.TryGetProperty("id", out var idProp))
            {
                return Guid.Parse(idProp.GetString()!);
            }
            
            if (root.TryGetProperty("Id", out var idPropPascal))
            {
                return Guid.Parse(idPropPascal.GetString()!);
            }
        }
        catch { }

        throw new Exception($"Id not found in response. Response content: {responseContent}");
    }

    #endregion
}
