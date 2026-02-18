using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OutfitPlanner.Application.Contracts.Identity;
using OutfitPlanner.Application.Models.Identity;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Application.Constants;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace OutfitPlanner.Persistence.Security;

public class JwtService : IJWTService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<JwtService> _logger;

    public JwtService(UserManager<User> userManager, SignInManager<User> signInManager, IOptions<JwtSettings> jwtSettings, ILogger<JwtService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
    }

    public async Task<string> GenerateToken(User user)
    {
        var userClaims = await _userManager.GetClaimsAsync(user);
        var roles = await _userManager.GetRolesAsync(user);
        var roleClaims = roles.Select(role => new Claim(ClaimTypes.Role, role)).ToList();

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserName!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(CustomClaimTypes.Uid, user.Id)
        }
        .Union(userClaims)
        .Union(roleClaims);

        var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

        var jwtSecurityToken = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
                signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
    }

    public async Task<AuthResponse> Login(AuthRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
        {
            throw new Exception($"User with {request.Email} not found.");
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);

        if (!isPasswordValid)
        {
            _logger.LogWarning("Password verification failed for user {Email}", request.Email);
            throw new Exception($"Credentials for '{request.Email}' are not valid.");
        }

        var result = await _signInManager.PasswordSignInAsync(user.UserName!, request.Password, false, lockoutOnFailure: false);
        
        if (!result.Succeeded)
        {
            _logger.LogWarning("SignInManager.PasswordSignInAsync failed for user {Email}. IsLockedOut: {IsLockedOut}, IsNotAllowed: {IsNotAllowed}, RequiresTwoFactor: {RequiresTwoFactor}", 
                request.Email, result.IsLockedOut, result.IsNotAllowed, result.RequiresTwoFactor);
            throw new Exception($"Credentials for '{request.Email}' are not valid.");
        }

        var jwtSecurityToken = await GenerateToken(user);

        // Handle Refresh Token
        user.RefreshToken = GenerateRefreshToken();
        user.RefreshTokenExpiration = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenDurationInDays);
        await _userManager.UpdateAsync(user);

        return new AuthResponse
        {
            Id = user.Id,
            Token = jwtSecurityToken,
            Email = user.Email!,
            UserName = user.UserName!,
            RefreshToken = user.RefreshToken
        };
    }

    public async Task<RegistrationResponse> Register(RegistrationRequest request)
    {
        try 
        {
            var existingUser = await _userManager.FindByNameAsync(request.UserName);
            if (existingUser != null)
            {
                throw new Exception($"Username '{request.UserName}' already exists.");
            }

            var user = new User
            {
                Email = request.Email,
                Name = $"{request.FirstName} {request.LastName}",
                UserName = request.UserName,
                EmailConfirmed = true
            };

            var existingEmail = await _userManager.FindByEmailAsync(request.Email);
            if (existingEmail != null)
            {
                throw new Exception($"Email {request.Email} already exists.");
            }

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogWarning("User creation failed: {Errors}", errors);
                throw new Exception(errors);
            }

            if(!await _userManager.IsInRoleAsync(user, "Planner"))
            {
                await _userManager.AddToRoleAsync(user, "Planner");
            }
            
            bool isfirstuser = await _userManager.Users.CountAsync() == 1;
            if (isfirstuser)
            {
                await _userManager.AddToRoleAsync(user, "Admin");
            }

            // Generate tokens for Auto-Login
            var jwtToken = await GenerateToken(user);
            user.RefreshToken = GenerateRefreshToken();
            user.RefreshTokenExpiration = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenDurationInDays);
            await _userManager.UpdateAsync(user);

            _logger.LogInformation("Successfully registered and auto-logged in user {Email}", user.Email);

            return new RegistrationResponse 
            { 
                UserId = user.Id,
                Token = jwtToken,
                RefreshToken = user.RefreshToken,
                Email = user.Email!,
                UserName = user.UserName!
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Critical error during user registration logic");
            throw;
        }
    }

    public async Task<AuthResponse> RefreshToken(string token, string refreshToken)
    {
        var principal = GetPrincipalFromExpiredToken(token);
        var username = principal.Identity!.Name;

        var user = await _userManager.FindByNameAsync(username!);
        if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiration <= DateTime.UtcNow)
        {
            throw new Exception("Invalid refresh token or token expired.");
        }

        var newJwtToken = await GenerateToken(user);
        user.RefreshToken = GenerateRefreshToken();
        await _userManager.UpdateAsync(user);

        return new AuthResponse
        {
            Id = user.Id,
            Token = newJwtToken,
            Email = user.Email!,
            UserName = user.UserName!,
            RefreshToken = user.RefreshToken
        };
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false, // You might want to validate this
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key)),
            ValidateLifetime = false // Important to be false as we are checking an expired token
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
        var jwtSecurityToken = securityToken as JwtSecurityToken;
        if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            throw new SecurityTokenException("Invalid token");

        return principal;
    }
}
