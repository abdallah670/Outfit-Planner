using AutoMapper;
using MediatR;
using OutfitPlanner.Application.Common;
using OutfitPlanner.Application.Contracts.Persistence;
using OutfitPlanner.Application.DTOs.Feed;
using OutfitPlanner.Application.Features.Feed.Requests.Queries;

namespace OutfitPlanner.Application.Features.Feed.Handlers.Queries;

/// <summary>
/// Handler for GetPostCommentsQuery with cursor-based pagination
/// </summary>
public class GetPostCommentsQueryHandler : IRequestHandler<GetPostCommentsQuery, CursorPagination.CursorPagedResult<PostCommentDto>>
{
    private readonly IPostCommentRepository _postCommentRepository;
    private readonly IMapper _mapper;

    public GetPostCommentsQueryHandler(
        IPostCommentRepository postCommentRepository,
        IMapper mapper)
    {
        _postCommentRepository = postCommentRepository;
        _mapper = mapper;
    }

    public async Task<CursorPagination.CursorPagedResult<PostCommentDto>> Handle(GetPostCommentsQuery request, CancellationToken cancellationToken)
    {
        var result = await _postCommentRepository.GetRootCommentsCursorAsync(
            request.PostId,
            request.Cursor,
            request.PageSize);

        var dtos = _mapper.Map<List<PostCommentDto>>(result.Items);

        return new CursorPagination.CursorPagedResult<PostCommentDto>
        {
            Items = dtos,
            NextCursor = result.NextCursor,
            HasMore = result.HasMore,
            PageSize = result.PageSize
        };
    }
}
