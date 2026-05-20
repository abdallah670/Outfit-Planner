using AutoMapper;
using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Contracts.Persistence;
using OutfitPlanner.Application.DTOs.Feed;
using OutfitPlanner.Application.Exceptions;
using OutfitPlanner.Application.Features.Feed.Requests.Queries;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace OutfitPlanner.Application.Features.Feed.Handlers.Queries;

/// <summary>
/// Handler for GetPollByIdRequest
/// </summary>
public class GetPollByIdRequestHandler : IRequestHandler<GetPollByIdRequest, ValidationPollDto>
{
    private readonly IValidationPollRepository _validationPollRepository;
    private readonly IFeedPostRepository _feedPostRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetPollByIdRequestHandler> _logger;

    public GetPollByIdRequestHandler(
        IValidationPollRepository validationPollRepository,
        IFeedPostRepository feedPostRepository,
        IUserRepository userRepository,
        IMapper mapper,
        ILogger<GetPollByIdRequestHandler> logger)
    {
        _validationPollRepository = validationPollRepository;
        _feedPostRepository = feedPostRepository;
        _userRepository = userRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ValidationPollDto> Handle(GetPollByIdRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var poll = await _validationPollRepository.GetFirstOrDefaultAsync(
                p => p.Id == request.Id,
                query => query.Include(p => p.Options).ThenInclude(o => o.Outfit)
            );
            
            if (poll == null)
            {
                // Try searching by FeedPostId in case the ID provided in the route was a FeedPostId
                var feedPostById = await _feedPostRepository.GetByIdAsync(request.Id);
                if (feedPostById != null && feedPostById.PollId.HasValue)
                {
                    poll = await _validationPollRepository.GetFirstOrDefaultAsync(
                        p => p.Id == feedPostById.PollId.Value,
                        query => query.Include(p => p.Options).ThenInclude(o => o.Outfit)
                    );
                }
            }

            if (poll == null)
            {
                _logger.LogInformation("Poll with ID {Id} not found", request.Id);
                throw new NotFoundException("Poll", request.Id);
            }

            _logger.LogInformation("Retrieved poll {Id} for user {UserId}", request.Id, request.UserId);
            var pollDto = _mapper.Map<ValidationPollDto>(poll);

            // Fetch associated FeedPost to get tags and tagged users
            var feedPost = await _feedPostRepository.GetByPollIdAsync(poll.Id);
            if (feedPost == null)
            {
                // Fallback: the input request.Id is the FeedPostId
                feedPost = await _feedPostRepository.GetByIdAsync(request.Id);
            }

            if (feedPost != null)
            {
                if (feedPost.Tags != null && feedPost.Tags.Any())
                {
                    pollDto.Tags = feedPost.Tags;
                    var taggedUsers = await _userRepository.GetTaggedUsersAsync(feedPost.Tags);
                    pollDto.TaggedUsers = _mapper.Map<List<TaggedUserDto>>(taggedUsers);
                }
            }

            return pollDto;
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting poll {Id}", request.Id);
            throw new BadRequestException("Error retrieving poll");
        }
    }
}
