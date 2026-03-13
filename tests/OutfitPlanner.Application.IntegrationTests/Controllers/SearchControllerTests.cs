using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using OutfitPlanner.Application.DTOs.Search;
using Xunit;

namespace OutfitPlanner.Application.IntegrationTests.Controllers;

public class SearchControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public SearchControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Search_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/search?q=test");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Search_WithAuth_AndQuery_ShouldReturnResults()
    {
        // Arrange
        await _client.AuthenticateAsync();

        // Act
        var response = await _client.GetAsync("/api/search?q=summer");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<SearchResultDto>();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Search_WithFilters_ShouldReturnFilteredResults()
    {
        // Arrange
        await _client.AuthenticateAsync();

        // Act
        var response = await _client.GetAsync("/api/search?q=&type=Outfits&categories=Tops&seasons=Summer&color=Blue");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<SearchResultDto>();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetRecentSearches_WithAuth_ShouldReturnList()
    {
        // Arrange
        await _client.AuthenticateAsync();

        // Act
        var response = await _client.GetAsync("/api/search/recent");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<string>>();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ClearRecentSearches_WithAuth_ShouldReturnNoContent()
    {
        // Arrange
        await _client.AuthenticateAsync();

        // Act
        var response = await _client.DeleteAsync("/api/search/recent");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task GetSuggestions_WithShortQuery_ShouldReturnEmpty()
    {
        // Arrange
        await _client.AuthenticateAsync();

        // Act
        var response = await _client.GetAsync("/api/search/suggestions?q=a");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<string>>();
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Search_WithPagination_ShouldRespectPageSize()
    {
        // Arrange
        await _client.AuthenticateAsync();

        // Act
        var response = await _client.GetAsync("/api/search?q=&page=1&pageSize=5");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<SearchResultDto>();
        result.Should().NotBeNull();
        // Results should not exceed page size
        (result.Outfits.Count + result.WardrobeItems.Count).Should().BeLessThanOrEqualTo(5);
    }
}
