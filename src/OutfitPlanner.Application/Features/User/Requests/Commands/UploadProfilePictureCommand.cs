using MediatR;
using Microsoft.AspNetCore.Http;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Application.Features.User.Requests.Commands;

public class UploadProfilePictureCommand : IRequest<BaseCommandResponse>
{
    public string UserId { get; set; } = string.Empty;
    public IFormFile File { get; set; } = null!;
}
