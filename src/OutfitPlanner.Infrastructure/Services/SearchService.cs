using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using OutfitPlanner.Application.Contracts.Infrastructure;
using OutfitPlanner.Application.DTOs.Search;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Domain.Enums;
using OutfitPlanner.Persistence;
using System.Collections.Concurrent;

namespace OutfitPlanner.Infrastructure.Services;

public class SearchService : ISearchService
{
    private readonly AppDbContext _context;
    private readonly IMemoryCache _cache;
    private static readonly ConcurrentDictionary<string, List<string>> _recentSearchesCache = new();

    // Cache settings
    private static readonly TimeSpan SearchResultCacheDuration = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan FacetCacheDuration = TimeSpan.FromMinutes(10);

    public SearchService(AppDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<SearchResultDto> SearchAsync(string userId, SearchRequest request, CancellationToken cancellationToken = default)
    {
        // Generate cache key based on search parameters
        var cacheKey = $"search:{userId}:{request.Type}:{request.Query?.ToLower() ?? ""}:{string.Join(",", request.Categories)}:{string.Join(",", request.Seasons)}:{string.Join(",", request.Occasions)}:{request.Color}:{request.MinPrice}:{request.MaxPrice}:{request.Page}";

        // Try to get from cache
        if (_cache.TryGetValue(cacheKey, out SearchResultDto? cachedResult) && cachedResult != null)
        {
            return cachedResult;
        }

        var result = new SearchResultDto();
        var query = request.Query?.ToLower() ?? "";

        // Run searches sequentially to avoid DbContext concurrency issues
        // DbContext is not thread-safe, so parallel execution causes issues
        if (request.Type == SearchType.All || request.Type == SearchType.Outfits)
        {
            await SearchOutfitsAsync(userId, request, query, result, cancellationToken);
        }

        if (request.Type == SearchType.All || request.Type == SearchType.Wardrobe)
        {
            await SearchWardrobeItemsAsync(userId, request, query, result, cancellationToken);
        }

        result.TotalResults = result.Outfits.Count + result.WardrobeItems.Count;

        // Cache facets separately (they don't change often)
        result.Facets = await GetCachedFacetsAsync(userId, request.Type, cancellationToken);

        // Cache the result
        _cache.Set(cacheKey, result, SearchResultCacheDuration);

        return result;
    }

    private async Task SearchOutfitsAsync(string userId, SearchRequest request, string query, SearchResultDto result, CancellationToken cancellationToken)
    {
        // Use compiled query for better performance
        var outfitQuery = _context.Outfits
            .AsNoTracking()
            .Where(o => o.UserId == userId)
            .AsQueryable();

        // Apply text search (only if query provided)
        if (!string.IsNullOrWhiteSpace(query))
        {
            outfitQuery = outfitQuery.Where(o =>
                EF.Functions.Like(o.Name.ToLower(), $"%{query}%"));
        }

        // Apply season filter
        if (request.Seasons?.Any() == true)
        {
            var seasonEnums = request.Seasons
                .Select(s => Enum.TryParse<Season>(s, true, out var season) ? (Season?)season : null)
                .Where(s => s.HasValue)
                .Select(s => s!.Value)
                .ToList();

            if (seasonEnums.Any())
            {
                outfitQuery = outfitQuery.Where(o => seasonEnums.Contains(o.Season));
            }
        }

        // Apply occasion filter (new)
        if (request.Occasions?.Any() == true)
        {
            var occasionEnums = request.Occasions
                .Select(o => Enum.TryParse<OccasionType>(o, true, out var occasion) ? (OccasionType?)occasion : null)
                .Where(o => o.HasValue)
                .Select(o => o!.Value)
                .ToList();

            if (occasionEnums.Any())
            {
                outfitQuery = outfitQuery.Where(o => occasionEnums.Contains(o.Occasion));
            }
        }

        // Apply pagination with Skip and Take
        var skip = (request.Page - 1) * request.PageSize;
        var outfits = await outfitQuery
            .OrderBy(o => o.Name)
            .Skip(skip)
            .Take(request.PageSize)
            .Select(o => new OutfitSearchResultDto
            {
                Id = o.Id,
                Name = o.Name,
                ImageUrl = o.ImageUrl,
                Occasion = o.Occasion.ToString(),
                Season = o.Season.ToString(),
                RelevanceScore = string.IsNullOrWhiteSpace(query) ? 100.0 : CalculateRelevanceScore(o.Name, null, query)
            })
            .ToListAsync(cancellationToken);

        result.Outfits = outfits;
    }

    private async Task SearchWardrobeItemsAsync(string userId, SearchRequest request, string query, SearchResultDto result, CancellationToken cancellationToken)
    {
        var wardrobeQuery = _context.ClothingItems
            .AsNoTracking()
            .Where(c => c.UserId == userId && c.IsActive)
            .AsQueryable();

        // Apply text search (only if query provided)
        if (!string.IsNullOrWhiteSpace(query))
        {
            wardrobeQuery = wardrobeQuery.Where(c =>
                EF.Functions.Like(c.Name.ToLower(), $"%{query}%") ||
                EF.Functions.Like(c.Brand.ToLower(), $"%{query}%") ||
                EF.Functions.Like(c.Category.ToLower(), $"%{query}%"));
        }

        // Apply category filter (case-insensitive)
        if (request.Categories?.Any() == true)
        {
            var categoriesLower = request.Categories.Select(c => c.ToLower()).ToList();
            wardrobeQuery = wardrobeQuery.Where(c => categoriesLower.Contains(c.Category.ToLower()));
        }

        // Apply color filter
        if (!string.IsNullOrWhiteSpace(request.Color))
        {
            wardrobeQuery = wardrobeQuery.Where(c =>
                EF.Functions.Like(c.PrimaryColor.ToLower(), $"%{request.Color.ToLower()}%"));
        }

        // Note: Price filtering is disabled because PurchasePrice is a Money value object
        // EF Core cannot translate value object property access in LINQ queries
        // To enable price filtering, you'd need to either:
        // 1. Add a separate decimal PriceAmount column to the entity
        // 2. Use a raw SQL query for price filtering

        // Apply pagination with Skip and Take
        var skip = (request.Page - 1) * request.PageSize;
        var items = await wardrobeQuery
            .OrderBy(c => c.Name)
            .Skip(skip)
            .Take(request.PageSize)
            .Select(c => new WardrobeItemSearchResultDto
            {
                Id = c.Id,
                Name = c.Name,
                ImageUrl = c.ImageUrl,
                Brand = c.Brand,
                Category = c.Category,
                PrimaryColor = c.PrimaryColor,
                RelevanceScore = string.IsNullOrWhiteSpace(query) ? 100.0 : CalculateRelevanceScore(c.Name, c.Brand + " " + c.Category, query)
            })
            .ToListAsync(cancellationToken);

        result.WardrobeItems = items;
    }

    private async Task<Dictionary<string, int>> GetCachedFacetsAsync(string userId, SearchType type, CancellationToken cancellationToken)
    {
        var cacheKey = $"facets:{userId}:{type}";

        if (_cache.TryGetValue(cacheKey, out Dictionary<string, int>? cachedFacets) && cachedFacets != null)
        {
            return cachedFacets;
        }

        var facets = await BuildFacetsAsync(userId, type, cancellationToken);
        _cache.Set(cacheKey, facets, FacetCacheDuration);

        return facets;
    }

    public Task<List<string>> GetSuggestionsAsync(string userId, string partialQuery, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(partialQuery) || partialQuery.Length < 2)
            return Task.FromResult(new List<string>());

        var lowerQuery = partialQuery.ToLower();

        // Get suggestions from recent searches cache (fast, no DB call)
        var suggestions = new List<string>();

        if (_recentSearchesCache.TryGetValue(userId, out var recentSearches))
        {
            suggestions = recentSearches
                .Where(s => s.ToLower().Contains(lowerQuery))
                .Take(5)
                .ToList();
        }

        return Task.FromResult(suggestions);
    }

    public Task<List<string>> GetRecentSearchesAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (_recentSearchesCache.TryGetValue(userId, out var searches))
        {
            return Task.FromResult(searches.Take(10).ToList());
        }

        return Task.FromResult(new List<string>());
    }

    public Task SaveRecentSearchAsync(string userId, string query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Task.CompletedTask;

        _recentSearchesCache.AddOrUpdate(
            userId,
            new List<string> { query },
            (key, existing) =>
            {
                var updated = new List<string>(existing);
                updated.Remove(query); // Remove if exists
                updated.Insert(0, query); // Add to top
                return updated.Take(20).ToList(); // Keep last 20
            });

        return Task.CompletedTask;
    }

    public Task ClearRecentSearchesAsync(string userId, CancellationToken cancellationToken = default)
    {
        _recentSearchesCache.TryRemove(userId, out _);
        return Task.CompletedTask;
    }

    // Clear search cache for a user (call this when data changes)
    public void ClearUserSearchCache(string userId)
    {
        // Clear facet caches - these are the known keys we can clear
        // Note: Dynamic search result cache keys cannot be enumerated from IMemoryCache
        // For production, consider using a separate cache key tracking mechanism
        _cache.Remove($"facets:{userId}:All");
        _cache.Remove($"facets:{userId}:Outfits");
        _cache.Remove($"facets:{userId}:Wardrobe");
    }

    private double CalculateRelevanceScore(string name, string? description, string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return 100.0;

        var lowerName = name.ToLower();
        var lowerDesc = description?.ToLower() ?? "";
        var lowerQuery = query.ToLower();

        // Exact match gets highest score
        if (lowerName == lowerQuery)
            return 100.0;

        // Starts with query gets high score
        if (lowerName.StartsWith(lowerQuery))
            return 80.0;

        // Contains query gets medium score
        if (lowerName.Contains(lowerQuery))
            return 60.0;

        // Description contains query gets lower score
        if (lowerDesc.Contains(lowerQuery))
            return 40.0;

        return 10.0;
    }

    private async Task<Dictionary<string, int>> BuildFacetsAsync(string userId, SearchType type, CancellationToken cancellationToken)
    {
        var facets = new Dictionary<string, int>();

        // Build facets - simplified to avoid threading issues with DbContext
        if (type == SearchType.All || type == SearchType.Wardrobe)
        {
            // Category facets
            var categories = await _context.ClothingItems
                .AsNoTracking()
                .Where(c => c.UserId == userId && c.IsActive)
                .GroupBy(c => c.Category)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            foreach (var cat in categories)
            {
                facets[$"category_{cat.Category}"] = cat.Count;
            }

            // Color facets
            var colors = await _context.ClothingItems
                .AsNoTracking()
                .Where(c => c.UserId == userId && c.IsActive)
                .GroupBy(c => c.PrimaryColor)
                .Select(g => new { Color = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            foreach (var col in colors)
            {
                facets[$"color_{col.Color}"] = col.Count;
            }
        }

        if (type == SearchType.All || type == SearchType.Outfits)
        {
            // Occasion facets
            var occasions = await _context.Outfits
                .AsNoTracking()
                .Where(o => o.UserId == userId)
                .GroupBy(o => o.Occasion)
                .Select(g => new { Occasion = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            foreach (var occ in occasions)
            {
                facets[$"occasion_{occ.Occasion}"] = occ.Count;
            }

            // Season facets
            var seasons = await _context.Outfits
                .AsNoTracking()
                .Where(o => o.UserId == userId)
                .GroupBy(o => o.Season)
                .Select(g => new { Season = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            foreach (var season in seasons)
            {
                facets[$"season_{season.Season}"] = season.Count;
            }
        }

        return facets;
    }
}
