using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OutfitPlanner.Application.DTOs.Calendar;
using OutfitPlanner.Application.DTOs.Outfit;
using OutfitPlanner.Application.DTOs.Wardrobe;
using OutfitPlanner.Application.Models.Identity;
using OutfitPlanner.Domain.Enums;
using OutfitPlanner.Persistence;
using Xunit;

namespace OutfitPlanner.Application.IntegrationTests.Controllers;

[Collection("Sequential")]
public class CalendarControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly string _testEmail = "testcalendaruser@example.com";
    private readonly string _testPassword = "Test@123";
    private string _jwtToken = string.Empty;

    public CalendarControllerTests(WebApplicationFactory<Program> factory)
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
            UserName = "testcalendaruser",
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

    #region Calendar Events Tests

    [Fact]
    public async Task GetCalendarEvents_WithoutAuthentication_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/calendar/calendar-events?year=2024&month=1");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCalendarEvents_WithAuthentication_ReturnsOk()
    {
        await LoginTestUser();

        var response = await _client.GetAsync("/api/calendar/calendar-events?year=2024&month=1");
        response.EnsureSuccessStatusCode();
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var events = await response.Content.ReadFromJsonAsync<List<CalendarEventItemDto>>();
        events.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateCalendarEvent_WithValidData_ReturnsCreated()
    {
        await LoginTestUser();

        var createRequest = new CreateCalendarEventRequest
        {
            Title = "Team Meeting",
            Description = "Weekly team sync",
            Location = "Conference Room A",
            EventDate = DateTimeOffset.UtcNow.AddDays(1),
            StartTime = TimeSpan.FromHours(10),
            EndTime = TimeSpan.FromHours(11),
            EventType = CalendarEventType.Work,
            Notes = "Bring laptop"
        };

        var response = await _client.PostAsJsonAsync("/api/calendar/calendar-events", createRequest);
        response.EnsureSuccessStatusCode();
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);

        var calendarEvent = await response.Content.ReadFromJsonAsync<CalendarEventItemDto>();
        calendarEvent.Should().NotBeNull();
        calendarEvent!.Title.Should().Be("Team Meeting");
        calendarEvent.EventType.Should().Be(CalendarEventType.Work);
    }

    [Fact]
    public async Task CreateCalendarEvent_WithOutfit_ReturnsCreated()
    {
        await LoginTestUser();

        // Create a clothing item and outfit first
        var clothingItemId = await CreateTestClothingItem("Test Shirt");
        var outfitId = await CreateTestOutfit("Business Casual", clothingItemId);

        var createRequest = new CreateCalendarEventRequest
        {
            Title = "Client Meeting",
            Description = "Important presentation",
            Location = "Main Office",
            EventDate = DateTimeOffset.UtcNow.AddDays(2),
            StartTime = TimeSpan.FromHours(14),
            EndTime = TimeSpan.FromHours(15),
            EventType = CalendarEventType.Meeting,
            OutfitId = outfitId,
            Notes = "Wear formal attire"
        };

        var response = await _client.PostAsJsonAsync("/api/calendar/calendar-events", createRequest);
        response.EnsureSuccessStatusCode();
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);

        var calendarEvent = await response.Content.ReadFromJsonAsync<CalendarEventItemDto>();
        calendarEvent.Should().NotBeNull();
        calendarEvent!.Title.Should().Be("Client Meeting");
        calendarEvent.HasOutfit.Should().BeTrue();
    }

    [Fact]
    public async Task CreateCalendarEvent_WithInvalidData_ReturnsBadRequest()
    {
        await LoginTestUser();

        var createRequest = new CreateCalendarEventRequest
        {
            Title = "", // Empty title - invalid
            EventDate = DateTimeOffset.UtcNow.AddDays(1),
            EventType = CalendarEventType.Work
        };

        var response = await _client.PostAsJsonAsync("/api/calendar/calendar-events", createRequest);
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateCalendarEvent_WithValidData_ReturnsOk()
    {
        await LoginTestUser();

        // Create an event first
        var eventId = await CreateTestCalendarEvent("Original Event");

        var updateRequest = new UpdateCalendarEventItemRequest
        {
            Title = "Updated Event",
            Description = "Updated description",
            Location = "New Location",
            EventType = CalendarEventType.Social
        };

        var response = await _client.PutAsJsonAsync($"/api/calendar/calendar-events/{eventId}", updateRequest);
        response.EnsureSuccessStatusCode();
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        // Verify the update
        var getResponse = await _client.GetAsync($"/api/calendar/calendar-events/by-date?date={DateTimeOffset.UtcNow.AddDays(1):yyyy-MM-dd}");
        var events = await getResponse.Content.ReadFromJsonAsync<List<CalendarEventItemDto>>();
        events.Should().NotBeNull();
        events!.Any(e => e.Title == "Updated Event").Should().BeTrue();
    }

    [Fact]
    public async Task DeleteCalendarEvent_WithValidId_ReturnsNoContent()
    {
        await LoginTestUser();

        // Create an event first
        var eventId = await CreateTestCalendarEvent("Event to Delete");

        // Delete the event
        var deleteResponse = await _client.DeleteAsync($"/api/calendar/calendar-events/{eventId}");
        deleteResponse.EnsureSuccessStatusCode();
        deleteResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);

        // Verify the event is deleted
        var getResponse = await _client.GetAsync($"/api/calendar/calendar-events/{eventId}");
        getResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetCalendarEventsByDate_ReturnsEventsForDate()
    {
        await LoginTestUser();

        var tomorrow = DateTimeOffset.UtcNow.AddDays(1);
        await CreateTestCalendarEvent("Tomorrow's Event", tomorrow);

        var response = await _client.GetAsync($"/api/calendar/calendar-events/by-date?date={tomorrow:yyyy-MM-ddTHH:mm:ssZ}");
        response.EnsureSuccessStatusCode();

        var events = await response.Content.ReadFromJsonAsync<List<CalendarEventItemDto>>();
        events.Should().NotBeNull();
    }

    #endregion

    #region Schedule Outfit Tests

    [Fact]
    public async Task ScheduleOutfit_WithValidData_ReturnsCreated()
    {
        await LoginTestUser();

        // Create a clothing item and outfit first
        var clothingItemId = await CreateTestClothingItem("Test Shirt for Schedule");
        var outfitId = await CreateTestOutfit("Scheduled Outfit", clothingItemId);

        var scheduleRequest = new ScheduleOutfitRequest
        {
            OutfitId = outfitId,
            ScheduledDate = DateTimeOffset.UtcNow.AddDays(3),
            Notes = "Wearing this for the interview"
        };

        var response = await _client.PostAsJsonAsync("/api/calendar/schedule", scheduleRequest);
        response.EnsureSuccessStatusCode();
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);

        var scheduledEvent = await response.Content.ReadFromJsonAsync<CalendarEventDto>();
        scheduledEvent.Should().NotBeNull();
        scheduledEvent!.OutfitId.Should().Be(outfitId);
        scheduledEvent.IsScheduled.Should().BeTrue();
    }

    [Fact]
    public async Task ScheduleOutfit_WithInvalidOutfit_ReturnsBadRequest()
    {
        await LoginTestUser();

        var scheduleRequest = new ScheduleOutfitRequest
        {
            OutfitId = Guid.NewGuid(), // Non-existent outfit
            ScheduledDate = DateTimeOffset.UtcNow.AddDays(3)
        };

        var response = await _client.PostAsJsonAsync("/api/calendar/schedule", scheduleRequest);
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetScheduledOutfits_ReturnsScheduledEvents()
    {
        await LoginTestUser();

        var response = await _client.GetAsync($"/api/calendar/scheduled?year={DateTime.UtcNow.Year}&month={DateTime.UtcNow.Month}");
        response.EnsureSuccessStatusCode();
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var scheduled = await response.Content.ReadFromJsonAsync<List<ScheduledOutfitDto>>();
        scheduled.Should().NotBeNull();
    }

    [Fact]
    public async Task GetMonthlyStats_ReturnsStats()
    {
        await LoginTestUser();

        var response = await _client.GetAsync($"/api/calendar/stats?year={DateTime.UtcNow.Year}&month={DateTime.UtcNow.Month}");
        response.EnsureSuccessStatusCode();
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var stats = await response.Content.ReadFromJsonAsync<MonthlyStatsDto>();
        stats.Should().NotBeNull();
    }

    #endregion

    #region Helper Methods

    private async Task<Guid> CreateTestClothingItem(string name)
    {
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

    private async Task<Guid> CreateTestCalendarEvent(string title, DateTimeOffset? eventDate = null)
    {
        var createRequest = new CreateCalendarEventRequest
        {
            Title = title,
            Description = "Test description",
            EventDate = eventDate ?? DateTimeOffset.UtcNow.AddDays(1),
            StartTime = TimeSpan.FromHours(10),
            EndTime = TimeSpan.FromHours(11),
            EventType = CalendarEventType.General,
            Notes = "Test notes"
        };

        var response = await _client.PostAsJsonAsync("/api/calendar/calendar-events", createRequest);
        response.EnsureSuccessStatusCode();

        var calendarEvent = await response.Content.ReadFromJsonAsync<CalendarEventItemDto>();
        return calendarEvent!.Id;
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
