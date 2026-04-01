using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Contracts.Identity;
using OutfitPlanner.Application.Contracts.Persistence;
using OutfitPlanner.Application.Models.Identity;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Persistence.Data;
using OutfitPlanner.Persistence.Repositories;
using OutfitPlanner.Persistence.Security;
using System.Linq;
using System.Text;

namespace OutfitPlanner.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

        // Add Identity only if it hasn't been registered yet to avoid duplicate authentication schemes
        if (!services.Any(sd => sd.ServiceType == typeof(UserManager<User>)))
        {
            services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();
        }

        var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();

        // register JWT authentication
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings!.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
                ClockSkew = TimeSpan.FromMinutes(5) // Allow 5 minutes clock skew
            };

          
        });

        services.AddScoped<IJWTService, JwtService>();
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IClothingItemRepository, ClothingItemRepository>();
        services.AddScoped<IOutfitRepository, OutfitRepository>();
        services.AddScoped<IValidationPollRepository, ValidationPollRepository>();
        services.AddScoped<IWearEventRepository, WearEventRepository>();
        services.AddScoped<IUserStyleProfileRepository, UserStyleProfileRepository>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IStyleRuleRepository, StyleRuleRepository>();
        services.AddScoped<IClothingTagRepository, ClothingTagRepository>();
        services.AddScoped<IOutfitItemRepository, OutfitItemRepository>();
        services.AddScoped<IPollOptionRepository, PollOptionRepository>();
        services.AddScoped<IVoteRepository, VoteRepository>();

        services.AddScoped<IUserPreferencesRepository, UserPreferencesRepository>();
        services.AddScoped<ICalendarEventRepository, CalendarEventRepository>();
        services.AddScoped<ITrendingOutfitRepository, TrendingOutfitRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IAppPreferencesRepository, AppPreferencesRepository>();
        services.AddScoped<INotificationSettingsRepository, NotificationSettingsRepository>();
   
        services.AddScoped<IFeedPostRepository, FeedPostRepository>();
        services.AddScoped<IPostReactionRepository, PostReactionRepository>();
        services.AddScoped<IPostCommentRepository, PostCommentRepository>();
        services.AddScoped<IFollowRepository, FollowRepository>();
        services.AddScoped<DataSeeder>();
        return services;
    }
}

