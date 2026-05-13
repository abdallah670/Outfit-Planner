using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.DTOs.Feed;
using OutfitPlanner.Application.Features.Feed.Requests.Commands;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace OutfitPlanner.Application.Features.Feed.Handlers.Commands;

public class CreatePollPostCommandHandler : IRequestHandler<CreatePollPostCommand, BaseCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOutfitRepository _outfitRepository;
    private readonly ILogger<CreatePollPostCommandHandler> _logger;

    public CreatePollPostCommandHandler(
        Contracts.Persistence.IValidationPollRepository validationPolls,
        IUnitOfWork unitOfWork,
        IOutfitRepository outfitRepository,
        ILogger<CreatePollPostCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _outfitRepository = outfitRepository;
        _logger = logger;
    }

    public async Task<BaseCommandResponse> Handle(CreatePollPostCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseCommandResponse();

        try
        {
            if (string.IsNullOrWhiteSpace(request.Question))
            {
                response.Success = false;
                response.Message = "Question is required";
                return response;
            }

            if (request.ExpiresAt <= DateTimeOffset.UtcNow)
            {
                response.Success = false;
                response.Message = "Expiration date must be in the future";
                return response;
            }

            if (request.OutfitIds == null || request.OutfitIds.Count < 2)
            {
                response.Success = false;
                response.Message = "At least 2 outfit options are required";
                return response;
            }

            var poll = new ValidationPoll
            {
                UserId = request.UserId,
                Question = request.Question,
                Context = "{}",
                ExpiresAt = request.ExpiresAt,
                Status = PollStatus.Active,
                Options = new List<PollOption>()
            };

            for (int i = 0; i < request.OutfitIds.Count; i++)
            {
                var outfit = await _outfitRepository.GetByIdAsync(request.OutfitIds[i]);
                if (outfit == null)
                {
                    response.Success = false;
                    response.Message = $"Outfit with ID {request.OutfitIds[i]} not found";
                    return response;
                }

                poll.Options.Add(new PollOption
                {
                    OutfitId = outfit.Id,
                   
                    DisplayOrder = i
                });
            }

            await _unitOfWork.ValidationPolls.AddAsync(poll);

            var feedPost = new FeedPost
            {
                UserId = request.UserId,
                PostType = PostType.Poll,
                PollId = poll.Id,
                Poll = poll,
                Caption = null,
                Tags = request.Tags,
                Visibility = request.Visibility,
                LikesCount = 0,
                CommentsCount = 0,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _unitOfWork.FeedPosts.AddAsync(feedPost);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            response.Id = feedPost.Id;
            response.Success = true;
            response.Message = "Poll post created successfully";

            _logger.LogInformation("Poll post {PostId} with poll {PollId} created by user {UserId}", 
                feedPost.Id, poll.Id, request.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating poll post for user {UserId}", request.UserId);
            response.Success = false;
            response.Message = "Error creating poll post";
            response.Errors.Add(ex.Message);
        }

        return response;
    }
}
