using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.DTOs.Social;
using OutfitPlanner.Application.Features.Social.Requests.Queries;

namespace OutfitPlanner.Application.Features.Social.Handlers.Queries;

public class GetOutfitEngagementQueryHandler : IRequestHandler<GetOutfitEngagementQuery, OutfitEngagementSummaryDto>
{
    private readonly IOutfitEngagementRepository _repository;

    public GetOutfitEngagementQueryHandler(IOutfitEngagementRepository repository)
    {
        _repository = repository;
    }

    public async Task<OutfitEngagementSummaryDto> Handle(GetOutfitEngagementQuery request, CancellationToken cancellationToken)
    {
        var likeCount = await _repository.GetLikeCountAsync(request.OutfitId);
        var commentCount = await _repository.GetCommentCountAsync(request.OutfitId);
        var userHasLiked = !string.IsNullOrEmpty(request.UserId) && 
                           await _repository.HasUserLikedAsync(request.OutfitId, request.UserId);

        return new OutfitEngagementSummaryDto
        {
            OutfitId = request.OutfitId,
            LikeCount = likeCount,
            CommentCount = commentCount,
            ShareCount = 0, // Not implemented yet
            UserHasLiked = userHasLiked
        };
    }
}
