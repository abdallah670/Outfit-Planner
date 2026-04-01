using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OutfitPlanner.Application.DTOs.Feed;
using OutfitPlanner.Application.DTOs.Wardrobe;
using OutfitPlanner.Application.Models.Identity;
using OutfitPlanner.Persistence;
using Xunit;

namespace OutfitPlanner.Application.IntegrationTests.Controllers;

[Collection("Sequential")]
public class SocialControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly string _testEmail = "testsocialuser@example.com";
    private readonly string _testPassword = "Test@123";
    private string _jwtToken = string.Empty;

    public SocialControllerTests(WebApplicationFactory<Program> factory)
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
            UserName = "testsocialuser",
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
    public async Task GetPolls_WithoutAuthentication_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/social/polls");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetPolls_WithAuthentication_ReturnsOk()
    {
        await LoginTestUser();

        var response = await _client.GetAsync("/api/social/polls");
        response.EnsureSuccessStatusCode();
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var polls = await response.Content.ReadFromJsonAsync<List<ValidationPollDto>>();
        polls.Should().NotBeNull();
    }

    [Fact]
    public async Task CreatePoll_WithValidData_ReturnsCreated()
    {
        await LoginTestUser();

        var createRequest = new CreatePollDto
        {
            Question = "Which outfit looks better for a date?",
            Context = "Looking for suggestions for Friday night",
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
            Options = new List<CreatePollOptionDto>
            {
                new() { Description = "Blue dress with heels", DisplayOrder = 1 },
                new() { Description = "Casual jeans with blouse", DisplayOrder = 2 }
            }
        };

        var response = await _client.PostAsJsonAsync("/api/social/polls", createRequest);
        response.EnsureSuccessStatusCode();
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("Poll created successfully");
    }

    [Fact]
    public async Task CreatePoll_WithLessThanTwoOptions_ReturnsBadRequest()
    {
        await LoginTestUser();

        var createRequest = new CreatePollDto
        {
            Question = "Which outfit looks better?",
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
            Options = new List<CreatePollOptionDto>
            {
                new() { Description = "Only one option", DisplayOrder = 1 }
            }
        };

        var response = await _client.PostAsJsonAsync("/api/social/polls", createRequest);
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreatePoll_WithExpiredDate_ReturnsBadRequest()
    {
        await LoginTestUser();

        var createRequest = new CreatePollDto
        {
            Question = "Which outfit looks better?",
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(-1), // Already expired
            Options = new List<CreatePollOptionDto>
            {
                new() { Description = "Option 1", DisplayOrder = 1 },
                new() { Description = "Option 2", DisplayOrder = 2 }
            }
        };

        var response = await _client.PostAsJsonAsync("/api/social/polls", createRequest);
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetPollById_WithValidId_ReturnsOk()
    {
        await LoginTestUser();

        // Create a poll first
        var pollId = await CreateTestPoll("GetById Test Poll");

        // Get the poll by ID
        var getResponse = await _client.GetAsync($"/api/social/polls/{pollId}");
        getResponse.EnsureSuccessStatusCode();
        getResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var poll = await getResponse.Content.ReadFromJsonAsync<ValidationPollDto>();
        poll.Should().NotBeNull();
        poll!.Question.Should().Be("GetById Test Poll");
    }

    [Fact]
    public async Task GetPollById_WithInvalidId_ReturnsNotFound()
    {
        await LoginTestUser();

        var invalidId = Guid.NewGuid();
        var response = await _client.GetAsync($"/api/social/polls/{invalidId}");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task VoteOnPoll_WithValidData_ReturnsOk()
    {
        await LoginTestUser();

        // Create a poll first
        var pollId = await CreateTestPoll("Vote Test Poll");

        // Get the poll to retrieve option IDs
        var getResponse = await _client.GetAsync($"/api/social/polls/{pollId}");
        var poll = await getResponse.Content.ReadFromJsonAsync<ValidationPollDto>();
        var optionId = poll!.Options.First().Id;

        // Cast a vote
        var voteRequest = new CastVoteDto
        {
            OptionId = optionId,
            Rating = 4,
            Comment = "Looks great!",
            IsAnonymous = false
        };

        var voteResponse = await _client.PostAsJsonAsync($"/api/social/polls/{pollId}/vote", voteRequest);
        voteResponse.EnsureSuccessStatusCode();
        voteResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var voteResult = await voteResponse.Content.ReadFromJsonAsync<BaseCommandResponse>();
        voteResult.Should().NotBeNull();
        voteResult!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task VoteOnPoll_WithExpiredPoll_ReturnsBadRequest()
    {
        await LoginTestUser();

        // Create an expired poll
        var createRequest = new CreatePollDto
        {
            Question = "Expired Poll Test",
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(-1), // Already expired
            Options = new List<CreatePollOptionDto>
            {
                new() { Description = "Option 1", DisplayOrder = 1 },
                new() { Description = "Option 2", DisplayOrder = 2 }
            }
        };

        var createResponse = await _client.PostAsJsonAsync("/api/social/polls", createRequest);
        var createResult = await createResponse.Content.ReadFromJsonAsync<BaseCommandResponse>();
        var pollId = createResult!.Id;

        // Get the poll to retrieve option IDs
        var getResponse = await _client.GetAsync($"/api/social/polls/{pollId}");
        var poll = await getResponse.Content.ReadFromJsonAsync<ValidationPollDto>();
        var optionId = poll!.Options.First().Id;

        // Try to vote
        var voteRequest = new CastVoteDto
        {
            OptionId = optionId,
            Rating = 3
        };

        var voteResponse = await _client.PostAsJsonAsync($"/api/social/polls/{pollId}/vote", voteRequest);
        voteResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task VoteOnPoll_DoubleVote_ReturnsBadRequest()
    {
        await LoginTestUser();

        // Create a poll
        var pollId = await CreateTestPoll("Double Vote Test Poll");

        // Get the poll to retrieve option IDs
        var getResponse = await _client.GetAsync($"/api/social/polls/{pollId}");
        var poll = await getResponse.Content.ReadFromJsonAsync<ValidationPollDto>();
        var option1Id = poll!.Options.First().Id;
        var option2Id = poll.Options.Skip(1).First().Id;

        // First vote
        var voteRequest1 = new CastVoteDto
        {
            OptionId = option1Id,
            Rating = 5
        };

        var voteResponse1 = await _client.PostAsJsonAsync($"/api/social/polls/{pollId}/vote", voteRequest1);
        voteResponse1.EnsureSuccessStatusCode();

        // Second vote on different option (should fail)
        var voteRequest2 = new CastVoteDto
        {
            OptionId = option2Id,
            Rating = 3
        };

        var voteResponse2 = await _client.PostAsJsonAsync($"/api/social/polls/{pollId}/vote", voteRequest2);
        voteResponse2.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetLocalTrends_WithoutAuthentication_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/social/trends/local");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetLocalTrends_WithAuthentication_ReturnsOk()
    {
        await LoginTestUser();

        var response = await _client.GetAsync("/api/social/trends/local");
        response.EnsureSuccessStatusCode();
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var trends = await response.Content.ReadFromJsonAsync<TrendingDataDto>();
        trends.Should().NotBeNull();
        trends!.Trends.Should().NotBeEmpty();
        trends.TopPolls.Should().NotBeEmpty();
    }

    [Fact]
    public async Task FullPollLifecycle_CreateGetVoteGetVerifyVoteCounts()
    {
        await LoginTestUser();

        // Step 1: Create a poll
        var createRequest = new CreatePollDto
        {
            Question = "Lifecycle Test Poll",
            Context = "Testing the full poll lifecycle",
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
            Options = new List<CreatePollOptionDto>
            {
                new() { Description = "Option A", DisplayOrder = 1 },
                new() { Description = "Option B", DisplayOrder = 2 }
            }
        };

        var createResponse = await _client.PostAsJsonAsync("/api/social/polls", createRequest);
        createResponse.EnsureSuccessStatusCode();
        createResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);

        var createResult = await createResponse.Content.ReadFromJsonAsync<BaseCommandResponse>();
        createResult.Should().NotBeNull();
        createResult!.Success.Should().BeTrue();
        var pollId = createResult.Id;

        // Step 2: Get the poll and verify it exists
        var getResponse = await _client.GetAsync($"/api/social/polls/{pollId}");
        getResponse.EnsureSuccessStatusCode();
        getResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var poll = await getResponse.Content.ReadFromJsonAsync<ValidationPollDto>();
        poll.Should().NotBeNull();
        poll!.Question.Should().Be("Lifecycle Test Poll");
        poll.TotalVotes.Should().Be(0);
        poll.Options.Should().HaveCount(2);

        var option1Id = poll.Options.First().Id;
        var option2Id = poll.Options.Skip(1).First().Id;

        // Step 3: Cast a vote
        var voteRequest = new CastVoteDto
        {
            OptionId = option1Id,
            Rating = 5,
            Comment = "Great choice!"
        };

        var voteResponse = await _client.PostAsJsonAsync($"/api/social/polls/{pollId}/vote", voteRequest);
        voteResponse.EnsureSuccessStatusCode();
        voteResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        // Step 4: Get the poll again and verify vote counts
        getResponse = await _client.GetAsync($"/api/social/polls/{pollId}");
        poll = await getResponse.Content.ReadFromJsonAsync<ValidationPollDto>();
        poll.Should().NotBeNull();
        poll!.TotalVotes.Should().Be(1);

        var option1 = poll.Options.First(o => o.Id == option1Id);
        option1.VoteCount.Should().Be(1);

        var option2 = poll.Options.First(o => o.Id == option2Id);
        option2.VoteCount.Should().Be(0);

        // Step 5: Verify poll appears in the list
        var listResponse = await _client.GetAsync("/api/social/polls");
        listResponse.EnsureSuccessStatusCode();
        var polls = await listResponse.Content.ReadFromJsonAsync<List<ValidationPollDto>>();
        polls.Should().NotBeNull();
        polls!.Any(p => p.Id == pollId).Should().BeTrue();
    }

    #region Helper Methods

    private async Task<Guid> CreateTestPoll(string question)
    {
        var createRequest = new CreatePollDto
        {
            Question = question,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
            Options = new List<CreatePollOptionDto>
            {
                new() { Description = "Test Option 1", DisplayOrder = 1 },
                new() { Description = "Test Option 2", DisplayOrder = 2 }
            }
        };

        var response = await _client.PostAsJsonAsync("/api/social/polls", createRequest);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<BaseCommandResponse>();
        return result!.Id;
    }

    #endregion
}

/// <summary>
/// DTO for trending data response (mirrors the controller DTO)
/// </summary>
public class TrendingDataDto
{
    public List<TrendItemDto> Trends { get; set; } = new();
    public List<TopPollDto> TopPolls { get; set; } = new();
    public DateTimeOffset GeneratedAt { get; set; }
}

/// <summary>
/// DTO for a single trend item
/// </summary>
public class TrendItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int PopularityScore { get; set; }
    public DateTimeOffset TrendingSince { get; set; }
}

/// <summary>
/// DTO for top poll information in trends
/// </summary>
public class TopPollDto
{
    public Guid PollId { get; set; }
    public string Question { get; set; } = string.Empty;
    public int TotalVotes { get; set; }
    public double EngagementRate { get; set; }
}

/// <summary>
/// Response from command operations
/// </summary>
public class BaseCommandResponse
{
    public Guid Id { get; set; }
    public bool Success { get; set; } = true;
    public string Message { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();
}
