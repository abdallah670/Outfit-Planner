using OutfitPlanner.Application.Common;
using MediatR;
using OutfitPlanner.Application.DTOs.Feed;


namespace OutfitPlanner.Application.Features.Feed.Requests.Queries;

public class GetTrendingOutfitsRequest : IRequest<CursorPagination.CursorPagedResult<TrendingOutfitDto>>
{
    public string? Cursor { get; set; }
    public string UserId { get; set; }
    public int PageSize { get; set; } = 20;
}

