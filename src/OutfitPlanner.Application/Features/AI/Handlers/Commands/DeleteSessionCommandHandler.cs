using MediatR;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Features.AI.Requests.Commands;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Features.AI.Handlers.Commands;

public class DeleteSessionCommandHandler : IRequestHandler<DeleteSessionCommand, BaseCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteSessionCommandHandler> _logger;

    public DeleteSessionCommandHandler(IUnitOfWork unitOfWork, ILogger<DeleteSessionCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<BaseCommandResponse> Handle(DeleteSessionCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseCommandResponse();

        var session = await _unitOfWork.Sessions.GetByIdAsync(request.SessionId);
        if (session == null)
        {
            response.Success = false;
            response.Message = "Session not found";
            return response;
        }

        if (session.UserId != request.UserId)
        {
            response.Success = false;
            response.Message = "Unauthorized";
            return response;
        }

        await _unitOfWork.Sessions.DeleteAsync(session);
        _logger.LogInformation("User {UserId} deleted session {SessionId}", request.UserId, request.SessionId);
        await _unitOfWork.SaveChangesAsync();

        response.Success = true;
        response.Message = "Session deleted";
        return response;
    }
}