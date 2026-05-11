namespace OutfitPlanner.Application.DTOs.Admin;

public class CreateBackupRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IncludeFiles { get; set; } = true;
    public bool Compress { get; set; } = true;
    public string[]? ExcludedTables { get; set; }
}
