using MediatR;
using OutfitPlanner.Application.DTOs.Social;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Application.Features.Social.Requests.Queries;

public class GetOutfitEngagementQuery : IRequest<OutfitEngagementSummaryDto>
{
    public Guid OutfitId { get; set; }
    public string UserId { get; set; } = string.Empty;
}

public class GetOutfitCommentsQuery : IRequest<OutfitPlanner.Application.Responses.PagedResult<OutfitCommentDto>>
{
    public Guid OutfitId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
