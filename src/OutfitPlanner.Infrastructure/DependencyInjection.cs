using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OutfitPlanner.Application.Contracts;
using OutfitPlanner.Application.Contracts.Infrastructure;
using OutfitPlanner.Application.Models;
using OutfitPlanner.Application.Models.Authentication;
using OutfitPlanner.Infrastructure.Configuration;
using OutfitPlanner.Infrastructure.Services;
using OutfitPlanner.Persistence;

namespace OutfitPlanner.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPersistence(configuration);
        
        // Image Storage Settings
        var storageSettings = configuration.GetSection(ImageStorageSettings.SectionName)
            .Get<ImageStorageSettings>() ?? new ImageStorageSettings();
        services.AddSingleton(storageSettings);

        // Outfit Image Cache Settings
        services.Configure<OutfitImageCacheSettings>(
            configuration.GetSection(OutfitImageCacheSettings.SectionName));

        // Register image processing services
        services.AddScoped<IImageProcessingService, ImageProcessingService>();
     
        services.AddScoped<IOutfitImageCacheService, OutfitImageCacheService>();

        // Register storage service
        services.AddScoped<IImageStorageService, LocalFileStorageService>();

        // Register Weather API services
        services.Configure<WeatherApiSettings>(
            configuration.GetSection(WeatherApiSettings.SectionName));
        
        services.AddHttpClient<IWeatherService, OpenWeatherMapWeatherService>(client =>
        {
            var settings = configuration.GetSection(WeatherApiSettings.SectionName)
                .Get<WeatherApiSettings>() ?? new WeatherApiSettings();
            
            client.BaseAddress = new Uri(settings.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);
        });

        // Register image combination and processing services (moved from Application layer)
        services.AddScoped<IImageCombinationService, ImageCombinationService>();
        services.AddScoped<IOutfitImageProcessingService, OutfitImageProcessingService>();

        // Register outfit image generator service
        services.AddScoped<IOutfitImageGeneratorService, OutfitImageGeneratorService>();

        // Register Search service
        services.AddScoped<ISearchService, SearchService>();

        // Register Trending Calculation Service

        // Register Email Settings and Service
        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
        services.AddScoped<IEmailService, EmailService>();

        // Register Background Removal Settings
        services.Configure<BackgroundRemovalSettings>(
            configuration.GetSection(BackgroundRemovalSettings.SectionName));

        // Register AI Settings
        services.Configure<AISettings>(
            configuration.GetSection(AISettings.SectionName));

        // Register Authentication Settings
        services.Configure<GoogleAuthSettings>(
            configuration.GetSection(GoogleAuthSettings.SectionName));
        services.Configure<FacebookAuthSettings>(
            configuration.GetSection(FacebookAuthSettings.SectionName));

        // Register Backup Settings
        services.Configure<BackupSettings>(
            configuration.GetSection(BackupSettings.SectionName));

        // Register Cache Settings
        services.Configure<CacheSettings>(
            configuration.GetSection(CacheSettings.SectionName));

        // Register Maintenance Settings
        services.Configure<MaintenanceSettings>(
            configuration.GetSection(MaintenanceSettings.SectionName));

        // Register Service Management Settings
        services.Configure<ServiceManagementSettings>(
            configuration.GetSection(ServiceManagementSettings.SectionName));

        // Register User Activity Settings
        services.Configure<UserActivitySettings>(
            configuration.GetSection(UserActivitySettings.SectionName));

        // Register Account Unlock Background Job
        services.AddScoped<AccountUnlockBackgroundJob>();

        // Register Infrastructure Services
        services.AddHealthChecks();
        services.AddScoped<IBackupService, BackupService>();
        services.AddScoped<ICacheManagementService, CacheManagementService>();
        services.AddScoped<IMaintenanceService, MaintenanceService>();
        services.AddScoped<IServiceManagementService, ServiceManagementService>();
        services.AddScoped<IUserActivityService, UserActivityService>();

        return services;
    }
}
