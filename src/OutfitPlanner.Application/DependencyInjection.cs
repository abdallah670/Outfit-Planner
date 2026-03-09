
using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OutfitPlanner.Application.Contracts;
using OutfitPlanner.Application.Services;

namespace OutfitPlanner.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            
            // Add image processing services
            services.AddSingleton<IOutfitImageProcessingService, OutfitImageProcessingService>();
            services.AddSingleton<IImageCombinationService, ImageCombinationService>();
            
            return services;
        }
    }
}
