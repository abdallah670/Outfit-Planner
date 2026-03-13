using Microsoft.EntityFrameworkCore;
using OutfitPlanner.Application.Contracts.Infrastructure;
using OutfitPlanner.Application.DTOs.Search;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Domain.Enums;
using OutfitPlanner.Persistence;

namespace OutfitPlanner.Infrastructure.Services;

public class SearchService : ISearchService
{
    private readonly AppDbContext _context;
    private static readonly Dictionary<string, List<string>> _recentSearchesCache = new();

    public SearchService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<SearchResultDto> SearchAsync(string userId, SearchRequest request, CancellationToken cancellationToken = default)
    {
        var result = new SearchResultDto();
        var query = request.Query?.ToLower() ?? "";
        var keywords = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        // Search Outfits
        if (request.Type == SearchType.All || request.Type == SearchType.Outfits)
        {
            var outfitQuery = _context.Outfits
                .AsNoTracking()
                .Where(o => o.UserId == userId && o.Status != OutfitStatus.Deleted)
                .AsQueryable();

            // Apply text search
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

            var outfits = await outfitQuery
                .OrderBy(o => o.Name)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            result.Outfits = outfits.Select(o => new OutfitSearchResultDto
            {
                Id = o.Id,
                Name = o.Name,
                ImageUrl = o.ImageUrl,
                Tags = new List<string>(), // Can be populated from related entities
                Occasion = o.Occasion.ToString(),
                Season = o.Season.ToString(),
                RelevanceScore = CalculateRelevanceScore(o.Name, null, query)
            }).ToList();
        }

        // Search Wardrobe Items
        if (request.Type == SearchType.All || request.Type == SearchType.Wardrobe)
        {
            var wardrobeQuery = _context.ClothingItems
                .AsNoTracking()
                .Where(c => c.UserId == userId && c.IsActive)
                .AsQueryable();

            // Apply text search
            if (!string.IsNullOrWhiteSpace(query))
            {
                wardrobeQuery = wardrobeQuery.Where(c =>
                    EF.Functions.Like(c.Name.ToLower(), $"%{query}%") ||
                    EF.Functions.Like(c.Brand.ToLower(), $"%{query}%") ||
                    EF.Functions.Like(c.Category.ToLower(), $"%{query}%"));
            }

            // Apply category filter
            if (request.Categories?.Any() == true)
            {
                wardrobeQuery = wardrobeQuery.Where(c => request.Categories.Contains(c.Category));
            }

            // Apply color filter
            if (!string.IsNullOrWhiteSpace(request.Color))
            {
                wardrobeQuery = wardrobeQuery.Where(c =>
                    EF.Functions.Like(c.PrimaryColor.ToLower(), $"%{request.Color.ToLower()}%"));
            }

            var items = await wardrobeQuery
                .OrderBy(c => c.Name)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            result.WardrobeItems = items.Select(c => new WardrobeItemSearchResultDto
            {
                Id = c.Id,
                Name = c.Name,
                ImageUrl = c.ImageUrl,
                Brand = c.Brand,
                Category = c.Category,
                PrimaryColor = c.PrimaryColor,
                RelevanceScore = CalculateRelevanceScore(c.Name, c.Brand + " " + c.Category, query)
            }).ToList();
        }

        result.TotalResults = result.Outfits.Count + result.WardrobeItems.Count;

        // Build facets
        result.Facets = await BuildFacetsAsync(userId, request.Type, cancellationToken);

        return result;
    }

    public Task<List<string>> GetSuggestionsAsync(string userId, string partialQuery, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(partialQuery) || partialQuery.Length < 2)
            return Task.FromResult(new List<string>());

        var lowerQuery = partialQuery.ToLower();

        // Get suggestions from outfit names and wardrobe item names
        var suggestions = new List<string>();

        // This could be optimized with a dedicated suggestions cache or index
        // For now, return empty list - can be implemented with more sophisticated logic
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

        if (!_recentSearchesCache.ContainsKey(userId))
        {
            _recentSearchesCache[userId] = new List<string>();
        }

        var searches = _recentSearchesCache[userId];
        
        // Remove if already exists (to move to top)
        searches.Remove(query);
        
        // Add to beginning
        searches.Insert(0, query);
        
        // Keep only last 20 searches
        if (searches.Count > 20)
        {
            searches.RemoveAt(searches.Count - 1);
        }

        return Task.CompletedTask;
    }

    public Task ClearRecentSearchesAsync(string userId, CancellationToken cancellationToken = default)
    {
        _recentSearchesCache.Remove(userId);
        return Task.CompletedTask;
    }

    private double CalculateRelevanceScore(string name, string? description, string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return 1.0;

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

        if (type == SearchType.All || type == SearchType.Wardrobe)
        {
            // Category facets
            var categories = await _context.ClothingItems
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
                .Where(c => c.UserId == userId && c.IsActive)
                .GroupBy(c => c.PrimaryColor)
                .Select(g => new { Color = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            foreach (var col in colors)
            {
                facets[$"color_{col.Color}"] = col.Count;
            }
        }

        return facets;
    }
}
