using MediatR;
using OutfitPlanner.Application.Contracts.Persistence;
using OutfitPlanner.Application.DTOs.User;
using OutfitPlanner.Application.Features.User.Requests.Queries;

namespace OutfitPlanner.Application.Features.User.Handlers.Queries;

public class GetAppPreferencesQueryHandler : IRequestHandler<GetAppPreferencesQuery, AppPreferencesDto>
{
    private readonly IAppPreferencesRepository _appPreferencesRepository;

    public GetAppPreferencesQueryHandler(IAppPreferencesRepository appPreferencesRepository)
    {
        _appPreferencesRepository = appPreferencesRepository;
    }

    public async Task<AppPreferencesDto> Handle(GetAppPreferencesQuery request, CancellationToken cancellationToken)
    {
        var preferences = await _appPreferencesRepository.GetByUserIdAsync(request.UserId);
        
        if (preferences == null)
        {
            // Return default preferences
            return new AppPreferencesDto();
        }

        return new AppPreferencesDto
        {
            TemperatureUnit = preferences.TemperatureUnit.ToString(),
            Language = preferences.Language,
            Theme = preferences.Theme.ToString(),
            MeasurementUnit = preferences.MeasurementUnit.ToString()
        };
    }
}
