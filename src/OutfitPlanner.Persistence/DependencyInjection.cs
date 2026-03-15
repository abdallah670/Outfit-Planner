using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Persistence.Repositories;
using OutfitPlanner.Persistence.Data;
using OutfitPlanner.Application.Models.Identity;
using OutfitPlanner.Application.Contracts.Identity;
using OutfitPlanner.Persistence.Security;

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

        services.AddIdentity<User, IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

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
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
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
        services.AddScoped<IOutfitFeedbackRepository, OutfitFeedbackRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IStyleRuleRepository, StyleRuleRepository>();
        services.AddScoped<IClothingTagRepository, ClothingTagRepository>();
        services.AddScoped<IOutfitItemRepository, OutfitItemRepository>();
        services.AddScoped<IPollOptionRepository, PollOptionRepository>();
        services.AddScoped<IVoteRepository, VoteRepository>();
        services.AddScoped<IUserPreferencesRepository, UserPreferencesRepository>();
        services.AddScoped<ICalendarEventRepository, CalendarEventRepository>();
        services.AddScoped<ITrendingOutfitRepository, TrendingOutfitRepository>();
        services.AddScoped<DataSeeder>();
        return services;
    }
}
