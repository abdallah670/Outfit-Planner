using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Application.Features.Social.Handlers.Commands;

public class UnlikeOutfitCommandHandler : IRequestHandler<UnlikeOutfitCommand, BaseCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public UnlikeOutfitCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<BaseCommandResponse> Handle(UnlikeOutfitCommand request, CancellationToken cancellationToken)
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

        await _unitOfWork.OutfitEngagement.UnlikeAsync(request.OutfitId, request.UserId);
        
        response.Success = true;
        response.Message = "Outfit unliked successfully";
        return response;
    }
}
