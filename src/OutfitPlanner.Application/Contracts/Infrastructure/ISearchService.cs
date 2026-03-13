using OutfitPlanner.Application.DTOs.Search;

namespace OutfitPlanner.Application.Contracts.Infrastructure;

public interface ISearchService
{
    Task<SearchResultDto> SearchAsync(string userId, SearchRequest request, CancellationToken cancellationToken = default);
    Task<List<string>> GetSuggestionsAsync(string userId, string partialQuery, CancellationToken cancellationToken = default);
    Task<List<string>> GetRecentSearchesAsync(string userId, CancellationToken cancellationToken = default);
    Task SaveRecentSearchAsync(string userId, string query, CancellationToken cancellationToken = default);
    Task ClearRecentSearchesAsync(string userId, CancellationToken cancellationToken = default);
}
