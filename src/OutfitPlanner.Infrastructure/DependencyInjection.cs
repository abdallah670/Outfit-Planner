using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
           var storageSettings = configuration.GetSection(ImageStorageSettings.SectionName)
            .Get<ImageStorageSettings>() ?? new ImageStorageSettings();

        services.AddSingleton(storageSettings);

        // Register image processing service
        services.AddScoped<IImageProcessingService, ImageProcessingService>();

      
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

        return services;
    }
}
