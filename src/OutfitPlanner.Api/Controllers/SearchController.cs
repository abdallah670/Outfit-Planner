using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutfitPlanner.Application.Contracts.Infrastructure;
using OutfitPlanner.Application.DTOs.Search;
using System.Security.Claims;

namespace OutfitPlanner.Api.Controllers;

[ApiController]
[Route("api/search")]
[Authorize]
public class SearchController : ControllerBase
{
    private readonly ISearchService _searchService;
    private readonly ILogger<SearchController> _logger;

    public SearchController(ISearchService searchService, ILogger<SearchController> logger)
    {
        _searchService = searchService;
        _logger = logger;
    }
    private string GetUserId() => User.FindFirstValue("uid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    /// <summary>
    /// Search outfits and wardrobe items
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(SearchResultDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SearchResultDto>> Search(
        [FromQuery] string? q,
        [FromQuery] SearchType type = SearchType.All,
        [FromQuery] string? categories = null,
        [FromQuery] string? seasons = null,
        [FromQuery] string? occasions = null,
        [FromQuery] string? color = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var request = new SearchRequest
        {
            Query = q ?? string.Empty,
            Type = type,
            Categories = ParseList(categories),
            Seasons = ParseList(seasons),
            Occasions = ParseList(occasions),
            Color = color,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            Page = page,
            PageSize = pageSize
        };

        _logger.LogInformation("User {UserId} searching for: {Query} with {FilterCount} filters", 
            userId, request.Query, request.Categories.Count + request.Seasons.Count + request.Occasions.Count);

        var result = await _searchService.SearchAsync(userId, request);

        // Save to recent searches if query is not empty
        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            await _searchService.SaveRecentSearchAsync(userId, request.Query);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get search suggestions based on partial query
    /// </summary>
    [HttpGet("suggestions")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<string>>> GetSuggestions([FromQuery] string q)
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var suggestions = await _searchService.GetSuggestionsAsync(userId, q);
        return Ok(suggestions);
    }

    /// <summary>
    /// Get user's recent searches
    /// </summary>
    [HttpGet("recent")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<string>>> GetRecentSearches()
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var recentSearches = await _searchService.GetRecentSearchesAsync(userId);
        return Ok(recentSearches);
    }

    /// <summary>
    /// Clear user's recent searches
    /// </summary>
    [HttpDelete("recent")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ClearRecentSearches()
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        await _searchService.ClearRecentSearchesAsync(userId);
        return NoContent();
    }

    private static List<string> ParseList(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new List<string>();

        return value.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList();
    }
}
