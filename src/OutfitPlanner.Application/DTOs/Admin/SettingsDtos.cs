namespace OutfitPlanner.Application.Features.Admin.DTOs;

public record SystemSettingDto(
    string Key, 
    string Value, 
    string DataType, 
    string Description, 
    bool IsEditable
);

public record UpdateSettingRequest(string Value);
