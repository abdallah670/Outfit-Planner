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

        // Validate file
        if (request.File == null || request.File.Length == 0)
        {
            return new BaseCommandResponse
            {
                Success = false,
                Message = "No file provided"
            };
        }

        // Check file type
        var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/webp" };
        if (!allowedTypes.Contains(request.File.ContentType.ToLower()))
        {
            return new BaseCommandResponse
            {
                Success = false,
                Message = "Invalid file type. Only JPEG, PNG, and WebP are allowed."
            };
        }

        // Check file size (max 5MB)
        if (request.File.Length > 5 * 1024 * 1024)
        {
            return new BaseCommandResponse
            {
                Success = false,
                Message = "File size exceeds 5MB limit"
            };
        }

        // Upload image
        using var stream = request.File.OpenReadStream();
        var fileName = $"profile-pictures/{request.UserId}_{Guid.NewGuid()}{Path.GetExtension(request.File.FileName)}";
        var uploadResult = await _imageStorageService.UploadImageAsync(stream, fileName, request.UserId, cancellationToken);

        if (!uploadResult.Success)
        {
            return new BaseCommandResponse
            {
                Success = false,
                Message = uploadResult.ErrorMessage ?? "Failed to upload image"
            };
        }

        // Delete old profile picture if exists
        if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
        {
            try
            {
                await _imageStorageService.DeleteImageAsync(user.ProfilePictureUrl, cancellationToken);
            }
            catch
            {
                // Ignore errors when deleting old image
            }
        }

        // Update user
        user.ProfilePictureUrl = uploadResult.OriginalPath;
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
            Message = $"Profile picture uploaded successfully|{uploadResult.OriginalPath}"
        };
    }
}
