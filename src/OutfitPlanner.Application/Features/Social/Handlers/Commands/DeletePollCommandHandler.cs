using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Features.Social.Requests.Commands;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Application.Features.Social.Handlers.Commands;

public class DeletePollCommandHandler : IRequestHandler<DeletePollCommand, BaseCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeletePollCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<BaseCommandResponse> Handle(DeletePollCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseCommandResponse();

        var poll = await _unitOfWork.ValidationPolls.GetByIdAsync(request.Id);
        if (poll == null)
        {
            response.Success = false;
            response.Message = "Poll not found";
            return response;
        }

        if (poll.UserId != request.UserId)
        {
            response.Success = false;
            response.Message = "You are not authorized to delete this poll";
            return response;
        }

        await _unitOfWork.ValidationPolls.RemoveAsync(poll);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        response.Success = true;
        response.Message = "Poll deleted successfully";
        response.Id = poll.Id;

        return response;
    }
}
