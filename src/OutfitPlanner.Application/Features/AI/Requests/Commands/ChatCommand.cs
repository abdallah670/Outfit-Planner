using MediatR;
using Microsoft.AspNetCore.Http;
using OutfitPlanner.Application.Contracts.Infrastructure;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Application.Features.AI.Requests.Commands;

public class ChatCommand : IRequest<BaseCommandResponse>
{
    public string UserId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Guid? SessionId { get; set; }
    public List<IFormFile>? UploadedImages { get; set; }
    public List<string>? Images { get; set; }
}