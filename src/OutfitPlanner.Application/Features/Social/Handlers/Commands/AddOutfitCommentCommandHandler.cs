using AutoMapper;
using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.DTOs.Social;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Features.Social.Handlers.Commands;

public class AddOutfitCommentCommandHandler : IRequestHandler<AddOutfitCommentCommand, BaseCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AddOutfitCommentCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<BaseCommandResponse> Handle(AddOutfitCommentCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseCommandResponse();

        var outfit = await _unitOfWork.Outfits.GetByIdAsync(request.OutfitId);
        if (outfit == null)
        {
            response.Success = false;
            response.Message = "Outfit not found";
            response.Errors = new List<string> { "Outfit not found" };
            return response;
        }

        if (string.IsNullOrWhiteSpace(request.Content))
        {
            response.Success = false;
            response.Message = "Comment content cannot be empty";
            response.Errors = new List<string> { "Comment content cannot be empty" };
            return response;
        }

        var comment = new OutfitComment
        {
            OutfitId = request.OutfitId,
            UserId = request.UserId,
            Content = request.Content,
            ParentCommentId = request.ParentCommentId
        };

        var createdComment = await _unitOfWork.OutfitEngagement.AddCommentAsync(comment);

        response.Success = true;
        response.Message = "Comment added successfully";
        response.Id = createdComment.Id;

        return response;
    }
}
