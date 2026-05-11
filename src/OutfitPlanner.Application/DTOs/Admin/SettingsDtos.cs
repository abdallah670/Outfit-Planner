namespace OutfitPlanner.Application.DTOs.Admin;

public record SystemSettingDto(
    string Key, 
    string Value, 
    string DataType, 
    string Description, 
    bool IsEditable
);

public record UpdateSettingRequest(string Value);
