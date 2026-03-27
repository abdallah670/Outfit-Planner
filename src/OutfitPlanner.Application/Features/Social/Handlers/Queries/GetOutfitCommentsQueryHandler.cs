using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.DTOs.Social;
using OutfitPlanner.Application.Features.Social.Requests.Queries;

namespace OutfitPlanner.Application.Features.Social.Handlers.Queries;

public class GetOutfitCommentsQueryHandler : IRequestHandler<GetOutfitCommentsQuery, OutfitPlanner.Application.Responses.PagedResult<OutfitCommentDto>>
{
    private readonly IOutfitEngagementRepository _repository;

    public GetOutfitCommentsQueryHandler(IOutfitEngagementRepository repository)
    {
        _repository = repository;
    }

    public async Task<OutfitPlanner.Application.Responses.PagedResult<OutfitCommentDto>> Handle(GetOutfitCommentsQuery request, CancellationToken cancellationToken)
    {
        var comments = await _repository.GetCommentsAsync(request.OutfitId, request.Page, request.PageSize);

        var dtos = comments.Items.Select(c => new OutfitCommentDto
        {
            Id = c.Id,
            OutfitId = c.OutfitId,
            UserId = c.UserId,
            UserName = c.User?.UserName ?? "Anonymous",
            UserAvatarUrl = c.User?.ProfilePictureUrl ?? string.Empty,
            Content = c.Content,
            CreatedAt = c.CreatedAt,
            IsDeleted = c.IsDeleted
        }).ToList();

        return new OutfitPlanner.Application.Responses.PagedResult<OutfitCommentDto>
        {
            Items = dtos,
            TotalCount = comments.TotalCount,
            Page = comments.Page,
            PageSize = comments.PageSize
        };
    }
}
