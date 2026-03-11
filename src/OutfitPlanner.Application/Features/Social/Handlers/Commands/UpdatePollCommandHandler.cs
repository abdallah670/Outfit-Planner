using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Features.Social.Requests.Commands;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Features.Social.Handlers.Commands;

public class UpdatePollCommandHandler : IRequestHandler<UpdatePollCommand, BaseCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePollCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<BaseCommandResponse> Handle(UpdatePollCommand request, CancellationToken cancellationToken)
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
            response.Message = "You are not authorized to update this poll";
            return response;
        }

        if (request.Request.Question != null)
            poll.Question = request.Request.Question;
        if (request.Request.Context != null)
            poll.Context = request.Request.Context;
        if (request.Request.ExpiresAt.HasValue)
            poll.ExpiresAt = request.Request.ExpiresAt.Value;
        
        await _unitOfWork.ValidationPolls.UpdateAsync(poll);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        response.Success = true;
        response.Message = "Poll updated successfully";
        response.Id = poll.Id;

        return response;
    }
}
