using MediatR;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Features.Social.Requests.Commands;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Domain.Entities;
using System.Linq;

namespace OutfitPlanner.Application.Features.Social.Handlers.Commands;

public class LikeVoteCommentCommandHandler : IRequestHandler<LikeVoteCommentCommand, BaseCommandResponse>
{
    private readonly IVoteCommentRepository _voteCommentRepository;
    private readonly IGenericRepository<VoteCommentLike> _voteCommentLikeRepository;
    private readonly ILogger<LikeVoteCommentCommandHandler> _logger;

    public LikeVoteCommentCommandHandler(
        IVoteCommentRepository voteCommentRepository,
        IGenericRepository<VoteCommentLike> voteCommentLikeRepository,
        ILogger<LikeVoteCommentCommandHandler> logger)
    {
        _voteCommentRepository = voteCommentRepository;
        _voteCommentLikeRepository = voteCommentLikeRepository;
        _logger = logger;
    }

    public async Task<BaseCommandResponse> Handle(LikeVoteCommentCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseCommandResponse();

        try
        {
            var comment = await _voteCommentRepository.GetByIdAsync(request.CommentId);
            if (comment == null)
            {
                response.Success = false;
                response.Message = "Comment not found";
                response.Errors.Add("Comment not found");
                return response;
            }

            var likes = await _voteCommentLikeRepository.GetAllAsync();
            var existingLike = likes.FirstOrDefault(l => l.CommentId == request.CommentId && l.UserId == request.UserId);

            if (existingLike != null)
            {
                await _voteCommentLikeRepository.RemoveAsync(existingLike);
                response.Success = true;
                response.Message = "Comment unliked";
            }
            else
            {
                var newLike = new VoteCommentLike
                {
                    CommentId = request.CommentId,
                    UserId = request.UserId
                };
                await _voteCommentLikeRepository.AddAsync(newLike);
                response.Success = true;
                response.Message = "Comment liked";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error liking/unliking comment");
            response.Success = false;
            response.Message = "Error modifying like";
            response.Errors.Add(ex.Message);
        }

        return response;
    }
}
