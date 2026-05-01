using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OutfitPlanner.Application.DTOs.User;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Application.Contracts.Persistence;
using OutfitPlanner.Application.Features.User.Requests.Queries;

namespace OutfitPlanner.Application.Features.User.Handlers.Queries;

public class GetPublicUserProfileQueryHandler(
    UserManager<OutfitPlanner.Domain.Entities.User> userManager,
    IFollowRepository followRepository) : IRequestHandler<GetPublicUserProfileQuery, PublicUserProfileDto?>
{
    public async Task<PublicUserProfileDto?> Handle(GetPublicUserProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId);
        if (user == null) return null;

        // Get counts efficiently
        var wardrobeItemCount = await userManager.Users
            .Where(u => u.Id == request.UserId)
            .SelectMany(u => u.ClothingItems)
            .CountAsync();

        var outfitCount = await userManager.Users
            .Where(u => u.Id == request.UserId)
            .SelectMany(u => u.Outfits)
            .CountAsync();

        var totalWears = await userManager.Users
            .Where(u => u.Id == request.UserId)
            .SelectMany(u => u.Outfits)
            .SumAsync(o => o.TimesWorn);

        var followersCount = await followRepository.GetFollowersCountAsync(request.UserId);
        var followingCount = await followRepository.GetFollowingCountAsync(request.UserId);

        // Style profile (if exists)
        PublicUserStyleProfileDto? styleProfile = null;
        if (user.StyleProfile != null)
        {
            styleProfile = new PublicUserStyleProfileDto
            {
                Style = user.StyleProfile.Style,
                PreferredColors = user.StyleProfile.PreferredColors ?? new(),
                FitPreferences = user.StyleProfile.FitPreferences,
                ComfortPriority = user.StyleProfile.ComfortPriority,
                AcceptsTrends = user.StyleProfile.AcceptsTrends
            };
        }

        return new PublicUserProfileDto
        {
            Id = user.Id,
            Name = user.Name ?? user.UserName ?? string.Empty,
            UserName = user.UserName ?? string.Empty,
            ProfilePictureUrl = user.ProfilePictureUrl,
            Bio = user.Bio,
            CreatedAt = user.CreatedAt,
            WardrobeItemCount = wardrobeItemCount,
            OutfitCount = outfitCount,
            TotalWears = totalWears,
            FollowersCount = followersCount,
            FollowingCount = followingCount,
            StyleProfile = styleProfile
        };
    }
}
