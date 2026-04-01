using MediatR;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Features.Social.Requests.Commands;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Features.Social.Handlers.Commands;

public class AddVoteCommentCommandHandler : IRequestHandler<AddVoteCommentCommand, BaseCommandResponse>
{
    private readonly IVoteRepository _voteRepository;
    private readonly IVoteCommentRepository _voteCommentRepository;
    private readonly ILogger<AddVoteCommentCommandHandler> _logger;

    public AddVoteCommentCommandHandler(
        IVoteRepository voteRepository,
        IVoteCommentRepository voteCommentRepository,
        ILogger<AddVoteCommentCommandHandler> logger)
    {
        _voteRepository = voteRepository;
        _voteCommentRepository = voteCommentRepository;
        _logger = logger;
    }

    public async Task<BaseCommandResponse> Handle(AddVoteCommentCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseCommandResponse();

        try
        {
            var vote = await _voteRepository.GetByIdAsync(request.VoteId);
            if (vote == null)
            {
                response.Success = false;
                response.Message = "Vote not found";
                response.Errors.Add("Vote not found");
                return response;
            }

            if (request.ParentCommentId.HasValue)
            {
                var parent = await _voteCommentRepository.GetByIdAsync(request.ParentCommentId.Value);
                if (parent == null || parent.VoteId != request.VoteId)
                {
                    response.Success = false;
                    response.Message = "Parent comment not found or belongs to different vote";
                    response.Errors.Add("Invalid parent comment");
                    return response;
                }
            }

            var comment = new VoteComment
            {
                VoteId = request.VoteId,
                UserId = request.UserId,
                ParentCommentId = request.ParentCommentId,
                Content = request.Content
            };

            await _voteCommentRepository.AddAsync(comment);

            response.Success = true;
            response.Id = comment.Id;
            response.Message = "Comment added successfully";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding comment on vote {VoteId} by user {UserId}", request.VoteId, request.UserId);
            response.Success = false;
            response.Message = "Error adding comment";
            response.Errors.Add(ex.Message);
        }

        return response;
    }
}
