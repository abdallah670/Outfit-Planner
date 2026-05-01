using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OutfitPlanner.Application.Common; // added
using OutfitPlanner.Application.Contracts.Persistence;
using OutfitPlanner.Application.DTOs.Feed;
using OutfitPlanner.Application.Features.Feed.Requests.Queries;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Domain.Enums;


namespace OutfitPlanner.Application.Features.Feed.Handlers.Queries;

public class GetUserFeedQueryHandler : IRequestHandler<GetUserFeedQuery, CursorPagination.CursorPagedResult<FeedPostDto>>
{
    private readonly IFeedPostRepository _feedPostRepository;
    private readonly IMapper _mapper;

    public GetUserFeedQueryHandler(IFeedPostRepository feedPostRepository, IMapper mapper)
    {
        _feedPostRepository = feedPostRepository;
        _mapper = mapper;
    }

    public async Task<CursorPagination.CursorPagedResult<FeedPostDto>> Handle(GetUserFeedQuery request, CancellationToken cancellationToken)
    {
        var result = await _feedPostRepository.GetFeedAsync(
            request.UserId,
            request.Cursor,
            request.PageSize,
            null,
            Visibility.Public,
            null);

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
