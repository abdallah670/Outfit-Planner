using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OutfitPlanner.Application.Contracts;
using OutfitPlanner.Application.Contracts.Infrastructure;
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

        // Background Removal Settings
        services.Configure<BackgroundRemovalSettings>(
            configuration.GetSection(BackgroundRemovalSettings.SectionName));
        
        // Outfit Image Cache Settings
        services.Configure<OutfitImageCacheSettings>(
            configuration.GetSection(OutfitImageCacheSettings.SectionName));

        // Register image processing services
        services.AddScoped<IImageProcessingService, ImageProcessingService>();
        services.AddScoped<IBackgroundRemovalService, RemoveBgBackgroundRemovalService>();
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

        return services;
    }
}
