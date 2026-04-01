using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Features.Feed.Requests.Commands;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Domain.Enums;

namespace OutfitPlanner.Application.Features.Feed.Handlers.Commands;

public class ClosePollCommandHandler : IRequestHandler<ClosePollCommand, BaseCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public ClosePollCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<BaseCommandResponse> Handle(ClosePollCommand request, CancellationToken cancellationToken)
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
            response.Message = "You are not authorized to close this poll";
            return response;
        }

        poll.Status = PollStatus.Closed;
        
        await _unitOfWork.ValidationPolls.UpdateAsync(poll);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        response.Success = true;
        response.Message = "Poll closed successfully";
        response.Id = poll.Id;

        return response;
    }
}
