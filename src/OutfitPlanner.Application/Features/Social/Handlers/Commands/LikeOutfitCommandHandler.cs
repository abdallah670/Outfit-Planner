using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Application.Features.Social.Handlers.Commands;

public class LikeOutfitCommandHandler : IRequestHandler<LikeOutfitCommand, BaseCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public LikeOutfitCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<BaseCommandResponse> Handle(LikeOutfitCommand request, CancellationToken cancellationToken)
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

        var success = await _unitOfWork.OutfitEngagement.LikeAsync(request.OutfitId, request.UserId);
        
        response.Success = true;
        response.Message = "Outfit liked successfully";
        return response;
    }
}
