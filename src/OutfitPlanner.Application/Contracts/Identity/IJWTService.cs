using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Application.Models.Identity;

namespace OutfitPlanner.Application.Contracts.Identity;

public interface IJWTService
{
    Task<string> GenerateToken(User user);
    Task<AuthResponse> Login(AuthRequest request);
    Task<RegistrationResponse> Register(RegistrationRequest request);
    Task<AuthResponse> RefreshToken(string token, string refreshToken);
    Task<AuthResponse> SocialLogin(string email, string name, string provider, string providerId, string? profilePictureUrl = null);
}
