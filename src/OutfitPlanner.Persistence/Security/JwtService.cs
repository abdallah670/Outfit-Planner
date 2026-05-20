using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OutfitPlanner.Application.Contracts.Identity;
using OutfitPlanner.Application.Contracts.Infrastructure;
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
    private readonly IEmailService _emailService;
    private readonly ILogger<JwtService> _logger;

    public JwtService(UserManager<User> userManager, SignInManager<User> signInManager, IOptions<JwtSettings> jwtSettings, IEmailService emailService, ILogger<JwtService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtSettings = jwtSettings.Value;
        _emailService = emailService;
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
            new Claim(CustomClaimTypes.Uid, user.Id),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName!),
            new Claim("email_confirmed", user.EmailConfirmed.ToString().ToLower())
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

        var result = await _signInManager.PasswordSignInAsync(user.UserName!, request.Password, false, lockoutOnFailure: true);
        
        if (result.IsLockedOut)
        {
            throw new Exception("Account is locked due to multiple failed login attempts. Please try again later.");
        }
        
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
                EmailConfirmed = false,
                Role = Domain.Enums.UserRole.Planner
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

            // Generate email verification token
            var verificationToken = GenerateSecureToken();
            user.EmailVerificationToken = verificationToken;
            user.EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24);
            await _userManager.UpdateAsync(user);

            // Send verification email
            try
            {
                await _emailService.SendVerificationEmailAsync(user.Email!, user.UserName!, verificationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send verification email to {Email} during registration. Continuing registration.", user.Email);
            }

            _logger.LogInformation("Successfully registered user {Email}. Verification email sent.", user.Email);

            return new RegistrationResponse 
            { 
                UserId = user.Id,
                Email = user.Email!,
                UserName = user.UserName!,
                RequiresEmailVerification = true
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
        try
        {
            _logger.LogInformation("Attempting to refresh token for token: {TokenPrefix}...", 
                token?.Length > 20 ? token[..20] : token);

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(refreshToken))
            {
                throw new Exception("Token and refresh token are required.");
            }

            ClaimsPrincipal principal;
            try
            {
                principal = GetPrincipalFromExpiredToken(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate expired token");
                throw new Exception("Invalid token format.");
            }

            var username = principal?.Identity?.Name;
            if (string.IsNullOrEmpty(username))
            {
                _logger.LogWarning("Could not extract username from token");
                throw new Exception("Invalid token claims.");
            }

            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                _logger.LogWarning("User not found for username: {Username}", username);
                throw new Exception("User not found.");
            }

            if (user.RefreshToken != refreshToken)
            {
                _logger.LogWarning("Refresh token mismatch for user: {Username}", username);
                throw new Exception("Invalid refresh token.");
            }

            if (user.RefreshTokenExpiration <= DateTime.UtcNow)
            {
                _logger.LogWarning("Refresh token expired for user: {Username}. Expiration: {Expiration}", 
                    username, user.RefreshTokenExpiration);
                throw new Exception("Refresh token has expired. Please login again.");
            }

            var newJwtToken = await GenerateToken(user);
            user.RefreshToken = GenerateRefreshToken();
            user.RefreshTokenExpiration = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenDurationInDays);
            await _userManager.UpdateAsync(user);

            _logger.LogInformation("Token refreshed successfully for user: {Username}", username);

            return new AuthResponse
            {
                Id = user.Id,
                Token = newJwtToken,
                Email = user.Email!,
                UserName = user.UserName!,
                RefreshToken = user.RefreshToken
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<AuthResponse> SocialLogin(string email, string name, string provider, string providerId, string? profilePictureUrl = null)
    {
        // Try to find user by email
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            // Create new user for social login
            var username = email.Split('@')[0] + "_" + provider.ToLower();
            
            // Ensure username is unique
            var existingUser = await _userManager.FindByNameAsync(username);
            var counter = 1;
            var originalUsername = username;
            while (existingUser != null)
            {
                username = $"{originalUsername}{counter}";
                existingUser = await _userManager.FindByNameAsync(username);
                counter++;
            }

            user = new User
            {
                Email = email,
                Name = name,
                UserName = username,
                EmailConfirmed = true,
                ProfilePictureUrl = profilePictureUrl,
                Role = Domain.Enums.UserRole.Planner
            };

            // Generate a random secure password (users won't use this, they use social login)
            var randomPassword = GenerateSecureRandomPassword();
            var result = await _userManager.CreateAsync(user, randomPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("Failed to create social login user: {Errors}", errors);
                throw new Exception($"Failed to create user: {errors}");
            }

            // Assign default role
            if (!await _userManager.IsInRoleAsync(user, "Planner"))
            {
                await _userManager.AddToRoleAsync(user, "Planner");
            }

            _logger.LogInformation("Created new user via {Provider} social login: {Email}", provider, email);
        }
        else
        {
            // For existing users, the external login is already linked via ASP.NET Identity's external login system
            _logger.LogInformation("Existing user logged in via {Provider} social login: {Email}", provider, email);
        }

        // Update last login
        user.LastLogin = DateTimeOffset.UtcNow;
        
        // Generate tokens
        var jwtSecurityToken = await GenerateToken(user);
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

    private string GenerateSecureRandomPassword()
    {
        // Generate a secure random password for social login users
        // They won't use this password - they'll login via social providers
        var length = 32;
        var randomNumber = new byte[length];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
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

    public async Task LinkExternalAccount(string userId, string email, string provider, string providerId, string? profilePictureUrl = null)
    {
        // Find the user by ID
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            throw new Exception($"User with ID {userId} not found");
        }

        // Check if this external login is already linked
        var existingLogins = await _userManager.GetLoginsAsync(user);
        var existingLogin = existingLogins.FirstOrDefault(l => l.LoginProvider == provider);
        
        if (existingLogin != null)
        {
            _logger.LogInformation("External account {Provider} is already linked to user {UserId}", provider, userId);
            return; // Already linked
        }

        // Create the external login info
        var externalLogin = new UserLoginInfo(provider, providerId, provider);
        
        // Add the external login to the user
        var result = await _userManager.AddLoginAsync(user, externalLogin);
        
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("Failed to link external account: {Errors}", errors);
            throw new Exception($"Failed to link external account: {errors}");
        }

        // Update profile picture if provided and user doesn't have one
        if (!string.IsNullOrEmpty(profilePictureUrl) && string.IsNullOrEmpty(user.ProfilePictureUrl))
        {
            user.ProfilePictureUrl = profilePictureUrl;
            await _userManager.UpdateAsync(user);
        }

        _logger.LogInformation("Successfully linked {Provider} account to user {UserId}", provider, userId);
    }

    // Email Verification Methods
    public async Task VerifyEmailAsync(string email, string token)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            throw new Exception("User not found.");
        }

        if (user.EmailConfirmed)
        {
            throw new Exception("Email is already verified.");
        }

        if (user.EmailVerificationToken != token)
        {
            throw new Exception("Invalid verification token.");
        }

        if (user.EmailVerificationTokenExpiry < DateTime.UtcNow)
        {
            throw new Exception("Verification token has expired. Please request a new one.");
        }

        user.EmailConfirmed = true;
        user.EmailVerificationToken = null;
        user.EmailVerificationTokenExpiry = null;
        
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            throw new Exception("Failed to verify email.");
        }

        _logger.LogInformation("Email verified successfully for user {Email}", email);
    }

    public async Task ResendVerificationEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            throw new Exception("User not found.");
        }

        if (user.EmailConfirmed)
        {
            throw new Exception("Email is already verified.");
        }

        // Generate new verification token
        var verificationToken = GenerateSecureToken();
        user.EmailVerificationToken = verificationToken;
        user.EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24);
        await _userManager.UpdateAsync(user);

        // Send verification email
        try
        {
            await _emailService.SendVerificationEmailAsync(user.Email!, user.UserName!, verificationToken);
            _logger.LogInformation("Verification email resent to {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send verification email to {Email}. SMTP may not be configured. Verification token: {Token}", email, verificationToken);
        }
    }

    // Password Reset Methods
    public async Task ForgotPasswordAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            // Don't reveal if user exists
            _logger.LogWarning("Password reset requested for non-existent email: {Email}", email);
            return;
        }

        // Generate reset token
        var resetToken = GenerateSecureToken();
        user.PasswordResetToken = resetToken;
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
        await _userManager.UpdateAsync(user);

        // Send password reset email
        try
        {
            await _emailService.SendPasswordResetEmailAsync(user.Email!, user.UserName!, resetToken);
            _logger.LogInformation("Password reset email sent to {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send password reset email to {Email}. SMTP may not be configured.", email);
        }

        // Always log the reset link for development purposes
        _logger.LogInformation("Password reset link for {Email}: http://localhost:4200/reset-password?token={Token}&email={Email}",
            email, resetToken, email);
        Console.WriteLine($"\n\n=== PASSWORD RESET LINK ===\nhttp://localhost:4200/reset-password?token={resetToken}&email={email}\n===========================\n");
    }

    public async Task ResetPasswordAsync(string email, string token, string newPassword)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            throw new Exception("Invalid request.");
        }

        if (user.PasswordResetToken != token)
        {
            throw new Exception("Invalid reset token.");
        }

        if (user.PasswordResetTokenExpiry < DateTime.UtcNow)
        {
            throw new Exception("Reset token has expired. Please request a new one.");
        }

        // Validate the custom token matches what's stored in DB (already done above)
        // Now generate an ASP.NET Identity-compatible token and reset the password
        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);
        
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("Password reset failed: {Errors}", errors);
            throw new Exception($"Failed to reset password: {errors}");
        }

        // Clear reset token
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;
        await _userManager.UpdateAsync(user);

        _logger.LogInformation("Password reset successfully for user {Email}", email);
    }

    private string GenerateSecureToken()
    {
        return RandomNumberGenerator.GetInt32(100000, 999999).ToString();
    }
}
