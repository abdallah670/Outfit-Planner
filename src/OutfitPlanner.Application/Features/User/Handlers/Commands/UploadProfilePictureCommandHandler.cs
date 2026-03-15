using MediatR;
using Microsoft.AspNetCore.Identity;
using OutfitPlanner.Application.Contracts.Infrastructure;
using OutfitPlanner.Application.Exceptions;
using OutfitPlanner.Application.Features.User.Requests.Commands;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Application.Features.User.Handlers.Commands;

public class UploadProfilePictureCommandHandler : IRequestHandler<UploadProfilePictureCommand, BaseCommandResponse>
{
    private readonly UserManager<Domain.Entities.User> _userManager;
    private readonly IImageStorageService _imageStorageService;

    public UploadProfilePictureCommandHandler(
        UserManager<Domain.Entities.User> userManager,
        IImageStorageService imageStorageService)
    {
        _userManager = userManager;
        _imageStorageService = imageStorageService;
    }

    public async Task<BaseCommandResponse> Handle(UploadProfilePictureCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        
        if (user == null)
        {
            throw new NotFoundException(nameof(Domain.Entities.User), request.UserId);
        }
        var savingresult=await _imageStorageService.UploadProfilePictureAsync(request.File.OpenReadStream(),request.File.FileName,request.UserId,cancellationToken);
        if (!savingresult.Success)
        {
            return new BaseCommandResponse
            {
                Success = false,
                Message = savingresult.ErrorMessage ?? "Failed to upload image"
            };
        }


        // Update user
        user.ProfilePictureUrl = savingresult.OriginalPath;
        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            return new BaseCommandResponse
            {
                Success = false,
                Message = string.Join(", ", result.Errors.Select(e => e.Description))
            };
        }

        return new BaseCommandResponse
        {
            Success = true,
            Message = $"Profile picture uploaded successfully|{savingresult.OriginalPath}"
        };
    }
}
