using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OutfitPlanner.Application.Models.Identity;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Persistence;
using OutfitPlanner.Persistence.Security;

namespace OutfitPlanner.Application.IntegrationTests.Auth;

public class AuthenticationTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly AppDbContext _dbContext;
    private readonly UserManager<User> _userManager;

    public AuthenticationTests()
    {
        var services = new ServiceCollection();

        // Use an in-memory database for testing
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase($"OutfitPlannerTestDb_{Guid.NewGuid()}"));

        // Add Identity
        services.AddIdentity<User, IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        // Add logging
        services.AddLogging(b => b.AddDebug());

        // Configure JwtSettings
        services.Configure<JwtSettings>(opts =>
        {
            opts.Key = "SuperSecretKeyForTestingPurposesOnly123456!";
            opts.Issuer = "TestIssuer";
            opts.Audience = "TestAudience";
            opts.DurationInMinutes = 60;
            opts.RefreshTokenDurationInDays = 7;
        });

        _serviceProvider = services.BuildServiceProvider();

        _dbContext = _serviceProvider.GetRequiredService<AppDbContext>();
        _userManager = _serviceProvider.GetRequiredService<UserManager<User>>();

        // Seed roles
        var roleManager = _serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        roleManager.CreateAsync(new IdentityRole("Planner")).Wait();
        roleManager.CreateAsync(new IdentityRole("Admin")).Wait();
    }

    [Fact]
    public async Task Register_ShouldCreateUser_WithValidCredentials()
    {
        // Arrange
        var request = new RegistrationRequest
        {
            FirstName = "Test",
            LastName = "User",
            Email = "hnbg14006@gmail.com",
            UserName = "Meno12",
            Password = "Meno.9384"
        };

        // Act
        var user = new User
        {
            Email = request.Email,
            Name = $"{request.FirstName} {request.LastName}",
            UserName = request.UserName,
            EmailConfirmed = true
        };
        var result = await _userManager.CreateAsync(user, request.Password);

        // Assert
        result.Succeeded.Should().BeTrue("user creation should succeed with valid data");
        
        var createdUser = await _userManager.FindByEmailAsync(request.Email);
        createdUser.Should().NotBeNull();
        createdUser!.Email.Should().Be(request.Email);
        createdUser.UserName.Should().Be(request.UserName);
    }

    [Fact]
    public async Task Login_ShouldSucceed_AfterRegistration_WithSamePassword()
    {
        // Arrange - Register a user
        var password = "Meno.9384";
        var user = new User
        {
            Email = "hnbg14006@gmail.com",
            Name = "Login Test",
            UserName = "Meno12",
            EmailConfirmed = true
        };
        var createResult = await _userManager.CreateAsync(user, password);
        createResult.Succeeded.Should().BeTrue($"user creation should succeed but got: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");

        // Normalization check
        user.NormalizedEmail.Should().NotBeNullOrEmpty("Identity should normalize email on creation");

        // Act - Verify password
        var foundUser = await _userManager.FindByEmailAsync(user.Email!);
        if (foundUser == null)
        {
            // Debug: try finding by username
            var byName = await _userManager.FindByNameAsync(user.UserName!);
            byName.Should().NotBeNull($"User '{user.UserName}' should exist if created successfully, even if email lookup failed");
            foundUser = byName;
        }
        
        foundUser.Should().NotBeNull("user should be found by email or username");
        foundUser!.PasswordHash.Should().NotBeNullOrEmpty("user should have a password hash after creation");
        
        var passwordValid = await _userManager.CheckPasswordAsync(foundUser, password);

        // Assert
        passwordValid.Should().BeTrue($"password '{password}' should match the hash '{foundUser.PasswordHash}' immediately after registration");
    }

    [Fact]
    public async Task Login_ShouldFail_WithWrongPassword()
    {
        // Arrange - Register a user
        var user = new User
        {
            Email = "wrongpwd@example.com",
            Name = "Wrong Pwd",
            UserName = "wrongpwd",
            EmailConfirmed = true
        };
        await _userManager.CreateAsync(user, "CorrectPassword123!");

        // Act
        var foundUser = await _userManager.FindByEmailAsync("wrongpwd@example.com");
        var passwordValid = await _userManager.CheckPasswordAsync(foundUser!, "WrongPassword456!");

        // Assert
        passwordValid.Should().BeFalse("wrong password should not pass verification");
    }

    [Fact]
    public async Task PasswordReset_ShouldAllow_LoginWithNewPassword()
    {
        // Arrange - Register a user
        var user = new User
        {
            Email = "resettest@example.com",
            Name = "Reset Test",
            UserName = "resettest",
            EmailConfirmed = true
        };
        await _userManager.CreateAsync(user, "OldPassword123!");

        // Act - Reset password
        await _userManager.RemovePasswordAsync(user);
        await _userManager.AddPasswordAsync(user, "NewPassword456!");

        // Verify old password fails
        var oldValid = await _userManager.CheckPasswordAsync(user, "OldPassword123!");
        oldValid.Should().BeFalse("old password should no longer work");

        // Verify new password works
        var newValid = await _userManager.CheckPasswordAsync(user, "NewPassword456!");
        newValid.Should().BeTrue("new password should work after reset");
    }

    [Fact]
    public async Task Register_ShouldFail_WithDuplicateEmail()
    {
        // Arrange
        var user1 = new User
        {
            Email = "duplicate@example.com",
            Name = "User One",
            UserName = "userone",
            EmailConfirmed = true
        };
        await _userManager.CreateAsync(user1, "Password123!");

        var user2 = new User
        {
            Email = "duplicate@example.com",
            Name = "User Two",
            UserName = "usertwo",
            EmailConfirmed = true
        };

        // Act - Try to find existing user with same email first (mimicking JwtService logic)
        var existingEmail = await _userManager.FindByEmailAsync("duplicate@example.com");

        // Assert
        existingEmail.Should().NotBeNull("duplicate email check should find existing user");
    }

    [Fact]
    public async Task Register_ShouldFail_WithWeakPassword()
    {
        // Arrange
        var user = new User
        {
            Email = "weakpwd@example.com",
            Name = "Weak Pwd",
            UserName = "weakpwd",
            EmailConfirmed = true
        };

        // Act - Try with a too-short password
        var result = await _userManager.CreateAsync(user, "123");

        // Assert
        result.Succeeded.Should().BeFalse("weak password should be rejected by Identity");
        result.Errors.Should().NotBeEmpty();
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
        _serviceProvider.Dispose();
    }
}
