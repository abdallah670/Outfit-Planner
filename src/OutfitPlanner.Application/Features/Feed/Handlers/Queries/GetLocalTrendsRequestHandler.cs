using AutoMapper;
using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.DTOs.Social;
using OutfitPlanner.Application.Features.Social.Requests.Queries;

namespace OutfitPlanner.Application.Features.Social.Handlers.Queries;

public class GetLocalTrendsRequestHandler : IRequestHandler<GetLocalTrendsRequest, TrendingDataDto>
{
    private readonly IValidationPollRepository _validationPollRepository;
    private readonly IMapper _mapper;

    public GetLocalTrendsRequestHandler(IValidationPollRepository validationPollRepository, IMapper mapper)
    {
        _validationPollRepository = validationPollRepository;
        _mapper = mapper;
    }

    public async Task<TrendingDataDto> Handle(GetLocalTrendsRequest request, CancellationToken cancellationToken)
    {
        var activePolls = await _validationPollRepository.GetActivePollsAsync();
        
        var result = new TrendingDataDto
        {
            TopPolls = activePolls.Select(p => new TopPollDto
            {
                PollId = p.Id,
                Question = p.Question,
                TotalVotes = p.Votes?.Count ?? 0,
                EngagementRate = 0.0
            }).ToList(),
            Trends = new List<TrendItemDto>
            {
                new() { Id = Guid.NewGuid(), Title = "Summer Style", Description = "Trending summer outfits", Category = "Seasonal", PopularityScore = 95, TrendingSince = DateTimeOffset.UtcNow },
                new() { Id = Guid.NewGuid(), Title = "Casual Wear", Description = "Trending casual looks", Category = "Style", PopularityScore = 88, TrendingSince = DateTimeOffset.UtcNow },
                new() { Id = Guid.NewGuid(), Title = "Office Outfits", Description = "Professional wear trends", Category = "Work", PopularityScore = 82, TrendingSince = DateTimeOffset.UtcNow }
            },
            GeneratedAt = DateTimeOffset.UtcNow
        };

        return result;
    }
}
