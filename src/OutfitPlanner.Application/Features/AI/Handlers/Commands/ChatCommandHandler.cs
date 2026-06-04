using MediatR;
using OutfitPlanner.Application.Contracts.Infrastructure;
using OutfitPlanner.Application.Features.AI.Requests.Commands;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Application.Features.AI.Handlers.Commands;

public class ChatCommandHandler : IRequestHandler<ChatCommand, BaseCommandResponse>
{
    private readonly IChatService _chatService;

    public ChatCommandHandler(IChatService chatService)
    {
        _chatService = chatService;
    }

    public async Task<BaseCommandResponse> Handle(ChatCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseCommandResponse();

        var chatRequest = new ChatRequest
        {
            UserId = request.UserId,
            Message = request.Message,
            SessionId = request.SessionId
        };

        var chatResponse = await _chatService.ProcessMessageAsync(chatRequest, cancellationToken);

        response.Success = true;
        response.Message = chatResponse.Message;
        response.Id = chatResponse.SessionId;

        return response;
    }
}