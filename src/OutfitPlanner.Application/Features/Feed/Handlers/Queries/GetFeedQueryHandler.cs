using AutoMapper;
using MediatR;
using OutfitPlanner.Application.Common;
using OutfitPlanner.Application.Contracts.Persistence;
using OutfitPlanner.Application.DTOs.Feed;
using OutfitPlanner.Application.Features.Feed.Requests.Queries;
using OutfitPlanner.Domain.Enums;

namespace OutfitPlanner.Application.Features.Feed.Handlers.Queries;

/// <summary>
/// Handler for GetFeedQuery with cursor-based pagination
/// </summary>
public class GetFeedQueryHandler : IRequestHandler<GetFeedQuery, CursorPagination.CursorPagedResult<FeedPostDto>>
{
    private readonly IFeedPostRepository _feedPostRepository;
    private readonly IMapper _mapper;

    public GetFeedQueryHandler(
        IFeedPostRepository feedPostRepository,
        IMapper mapper)
    {
        _feedPostRepository = feedPostRepository;
        _mapper = mapper;
    }

    public async Task<CursorPagination.CursorPagedResult<FeedPostDto>> Handle(GetFeedQuery request, CancellationToken cancellationToken)
    {
        PostType? postType = Enum.TryParse<PostType>(request.PostType, true, out var pt) ? pt : null;
        var visibility = Enum.TryParse<Visibility>(request.Visibility, true, out var v) ? v : Visibility.Public;

        var result = await _feedPostRepository.GetFeedAsync(
            request.UserId,
            request.Cursor,
            request.PageSize,
            request.SortBy,
            visibility,
            postType);

        var dtos = _mapper.Map<List<FeedPostDto>>(result.Items);

        return new CursorPagination.CursorPagedResult<FeedPostDto>
        {
            Items = dtos,
            NextCursor = result.NextCursor,
            HasMore = result.HasMore,
            PageSize = result.PageSize
        };
    }
}
