using OutfitPlanner.Application.DTOs.Outfit;
using OutfitPlanner.Application.DTOs.Wardrobe;

namespace OutfitPlanner.Application.DTOs.Search;

public class SearchRequest
{
    public string Query { get; set; } = string.Empty;
    public SearchType Type { get; set; } = SearchType.All;
    public List<string> Categories { get; set; } = new();
    public List<string> Seasons { get; set; } = new();
    public List<string> Occasions { get; set; } = new();
    public string? Color { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public enum SearchType
{
    All,
    Outfits,
    Wardrobe,
    Social
}

public class SearchResultDto
{
    public List<OutfitSearchResultDto> Outfits { get; set; } = new();
    public List<WardrobeItemSearchResultDto> WardrobeItems { get; set; } = new();
    public int TotalResults { get; set; }
    public Dictionary<string, int> Facets { get; set; } = new();
}

public class OutfitSearchResultDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public List<string> Tags { get; set; } = new();
    public string? Occasion { get; set; }
    public string? Season { get; set; }
    public double RelevanceScore { get; set; }
}

public class WardrobeItemSearchResultDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string Brand { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string PrimaryColor { get; set; } = string.Empty;
    public double RelevanceScore { get; set; }
}
