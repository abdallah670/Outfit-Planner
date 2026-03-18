using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Contracts.Persistence;
using OutfitPlanner.Application.DTOs.User;
using OutfitPlanner.Application.Features.User.Requests.Commands;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Features.User.Handlers.Commands;

public class UpdateAppPreferencesCommandHandler : IRequestHandler<UpdateAppPreferencesCommand, BaseCommandResponse>
{
    private readonly IAppPreferencesRepository _appPreferencesRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAppPreferencesCommandHandler(
        IAppPreferencesRepository appPreferencesRepository,
        IUnitOfWork unitOfWork)
    {
        _appPreferencesRepository = appPreferencesRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BaseCommandResponse> Handle(UpdateAppPreferencesCommand request, CancellationToken cancellationToken)
    {
        var preferences = await _appPreferencesRepository.GetByUserIdAsync(request.UserId);
        
        if (preferences == null)
        {
            // Create new preferences
            preferences = new AppPreferences
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                TemperatureUnit = Enum.Parse<TemperatureUnit>(request.Preferences.TemperatureUnit),
                Language = request.Preferences.Language,
                Theme = Enum.Parse<AppTheme>(request.Preferences.Theme),
                MeasurementUnit = Enum.Parse<MeasurementUnit>(request.Preferences.MeasurementUnit),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _appPreferencesRepository.AddAsync(preferences);
        }
        else
        {
            // Update existing preferences
            preferences.TemperatureUnit = Enum.Parse<TemperatureUnit>(request.Preferences.TemperatureUnit);
            preferences.Language = request.Preferences.Language;
            preferences.Theme = Enum.Parse<AppTheme>(request.Preferences.Theme);
            preferences.MeasurementUnit = Enum.Parse<MeasurementUnit>(request.Preferences.MeasurementUnit);
            preferences.UpdatedAt = DateTime.UtcNow;
            await _appPreferencesRepository.UpdateAsync(preferences);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new BaseCommandResponse
        {
            Success = true,
            Message = "App preferences updated successfully"
        };
    }
}
