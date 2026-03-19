using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Exceptions;
using OutfitPlanner.Application.Features.User.Requests.Queries;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Features.User.Handlers.Queries;

public class ExportUserDataQueryHandler : IRequestHandler<ExportUserDataQuery, ExportUserDataResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IClothingItemRepository _clothingItemRepository;
    private readonly IOutfitRepository _outfitRepository;
    private readonly IWearEventRepository _wearEventRepository;
    private readonly IUserStyleProfileRepository _styleProfileRepository;
    private readonly IUserPreferencesRepository _userPreferencesRepository;

    public ExportUserDataQueryHandler(
        IUserRepository userRepository,
        IClothingItemRepository clothingItemRepository,
        IOutfitRepository outfitRepository,
        IWearEventRepository wearEventRepository,
        IUserStyleProfileRepository styleProfileRepository,
        IUserPreferencesRepository userPreferencesRepository)
    {
        _userRepository = userRepository;
        _clothingItemRepository = clothingItemRepository;
        _outfitRepository = outfitRepository;
        _wearEventRepository = wearEventRepository;
        _styleProfileRepository = styleProfileRepository;
        _userPreferencesRepository = userPreferencesRepository;
    }

    public async Task<ExportUserDataResult> Handle(ExportUserDataQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        
        if (user == null)
        {
            throw new NotFoundException("User", request.UserId);
        }

        var clothingItems = await _clothingItemRepository.FindAsync(c => c.UserId == request.UserId);
        var outfits = await _outfitRepository.FindAsync(o => o.UserId == request.UserId);
        var wearEvents = await _wearEventRepository.FindAsync(w => w.UserId == request.UserId);
        var styleProfile = await _styleProfileRepository.GetByUserIdAsync(request.UserId);
        var preferences = await _userPreferencesRepository.GetByUserIdAsync(request.UserId);

        using var memoryStream = new MemoryStream();
        using var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8);
        var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture);
        using var csvWriter = new CsvWriter(streamWriter, csvConfig);

        // Write User Info Section
        csvWriter.WriteField("OUTFIT PLANNER - USER DATA EXPORT");
        csvWriter.NextRecord();
        csvWriter.WriteField($"Exported At: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        csvWriter.NextRecord();
        csvWriter.WriteField($"User: {user.Name}");
        csvWriter.NextRecord();
        csvWriter.WriteField($"Email: {user.Email}");
        csvWriter.NextRecord();
        csvWriter.NextRecord();

        // Write Wardrobe Section
        csvWriter.WriteField("WARDROBE ITEMS");
        csvWriter.NextRecord();
        csvWriter.WriteField("Name");
        csvWriter.WriteField("Category");
        csvWriter.WriteField("Primary Color");
        csvWriter.WriteField("Brand");
        csvWriter.WriteField("Purchase Price");
        csvWriter.WriteField("Purchase Date");
        csvWriter.WriteField("Wear Count");
        csvWriter.NextRecord();

        foreach (var item in clothingItems)
        {
            csvWriter.WriteField(item.Name);
            csvWriter.WriteField(item.Category);
            csvWriter.WriteField(item.PrimaryColor);
            csvWriter.WriteField(item.Brand);
            csvWriter.WriteField(item.PurchasePrice?.ToString() ?? "");
            csvWriter.WriteField(item.PurchaseDate?.ToString("yyyy-MM-dd") ?? "");
            csvWriter.WriteField(item.WearCount);
            csvWriter.NextRecord();
        }

        csvWriter.NextRecord();
        csvWriter.NextRecord();

        // Write Outfits Section
        csvWriter.WriteField("OUTFITS");
        csvWriter.NextRecord();
        csvWriter.WriteField("Name");
        csvWriter.WriteField("Occasion");
        csvWriter.WriteField("Season");
        csvWriter.WriteField("Times Worn");
        csvWriter.NextRecord();

        foreach (var outfit in outfits)
        {
            csvWriter.WriteField(outfit.Name);
            csvWriter.WriteField(outfit.Occasion);
            csvWriter.WriteField(outfit.Season);
            csvWriter.WriteField(outfit.TimesWorn);
            csvWriter.NextRecord();
        }

        csvWriter.NextRecord();
        csvWriter.NextRecord();

        // Write Wear History Section
        csvWriter.WriteField("WEAR HISTORY");
        csvWriter.NextRecord();
        csvWriter.WriteField("Worn At");
        csvWriter.WriteField("Outfit ID");
        csvWriter.WriteField("Notes");
        csvWriter.WriteField("Weather");
        csvWriter.WriteField("Rating");
        csvWriter.NextRecord();

        foreach (var wearEvent in wearEvents)
        {
            csvWriter.WriteField(wearEvent.WornAt.ToString("yyyy-MM-dd"));
            csvWriter.WriteField(wearEvent.OutfitId?.ToString() ?? "");
            csvWriter.WriteField(wearEvent.Notes);
            csvWriter.WriteField(wearEvent.WeatherCondition);
            csvWriter.WriteField(wearEvent.Rating.ToString());
            csvWriter.NextRecord();
        }

        csvWriter.NextRecord();
        csvWriter.NextRecord();

        // Write Style Profile Section
        csvWriter.WriteField("STYLE PROFILE");
        csvWriter.NextRecord();
        if (styleProfile != null)
        {
            csvWriter.WriteField("Style");
            csvWriter.WriteField(styleProfile.Style.ToString());
            csvWriter.NextRecord();
            csvWriter.WriteField("Preferred Colors");
            csvWriter.WriteField(string.Join(", ", styleProfile.PreferredColors ?? new List<string>()));
            csvWriter.NextRecord();
            csvWriter.WriteField("Fit Preferences");
            csvWriter.WriteField(styleProfile.FitPreferences ?? "");
            csvWriter.NextRecord();
            csvWriter.WriteField("Comfort Priority");
            csvWriter.WriteField(styleProfile.ComfortPriority.ToString());
            csvWriter.NextRecord();
            csvWriter.WriteField("Accepts Trends");
            csvWriter.WriteField(styleProfile.AcceptsTrends ? "Yes" : "No");
            csvWriter.NextRecord();
        }
        else
        {
            csvWriter.WriteField("No style profile set");
            csvWriter.NextRecord();
        }

        csvWriter.NextRecord();

        // Write User Preferences Section
        csvWriter.WriteField("USER PREFERENCES");
        csvWriter.NextRecord();
        if (preferences != null)
        {
            csvWriter.WriteField("Share Outfits Anonymously");
            csvWriter.WriteField(preferences.ShareOutfitsAnonymously ? "Yes" : "No");
            csvWriter.NextRecord();
            csvWriter.WriteField("Include In Trend Analysis");
            csvWriter.WriteField(preferences.IncludeInTrendAnalysis ? "Yes" : "No");
            csvWriter.NextRecord();
            csvWriter.WriteField("Allow Friend Requests");
            csvWriter.WriteField(preferences.AllowFriendRequests ? "Yes" : "No");
            csvWriter.NextRecord();
            csvWriter.WriteField("Default Outfit Privacy");
            csvWriter.WriteField(preferences.DefaultOutfitPrivacy.ToString());
            csvWriter.NextRecord();
            csvWriter.WriteField("Show Body Metrics");
            csvWriter.WriteField(preferences.ShowBodyMetrics ? "Yes" : "No");
            csvWriter.NextRecord();
            csvWriter.WriteField("Allow Location Tracking");
            csvWriter.WriteField(preferences.AllowLocationTracking ? "Yes" : "No");
            csvWriter.NextRecord();
        }
        else
        {
            csvWriter.WriteField("No preferences set");
            csvWriter.NextRecord();
        }

        csvWriter.NextRecord();

        // Write Statistics
        csvWriter.WriteField("STATISTICS");
        csvWriter.NextRecord();
        csvWriter.WriteField("Total Clothing Items");
        csvWriter.WriteField(clothingItems.Count());
        csvWriter.NextRecord();
        csvWriter.WriteField("Total Outfits");
        csvWriter.WriteField(outfits.Count());
        csvWriter.NextRecord();
        csvWriter.WriteField("Total Wear Events");
        csvWriter.WriteField(wearEvents.Count());
        csvWriter.NextRecord();
        csvWriter.WriteField("Average Times Worn");
        csvWriter.WriteField(clothingItems.Any() ? clothingItems.Average(c => c.WearCount).ToString("F2") : "0");
        csvWriter.NextRecord();

        csvWriter.Flush();
        streamWriter.Flush();

        return new ExportUserDataResult
        {
            Data = memoryStream.ToArray(),
            ContentType = "text/csv",
            FileName = $"outfit-planner-data-{DateTime.UtcNow:yyyyMMdd}.csv"
        };
    }
}
