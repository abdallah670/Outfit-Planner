namespace OutfitPlanner.Application.Models
{
    public class BackgroundRemovalSettings
    {
        public const string SectionName = "BackgroundRemoval";
        
        public string Provider { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public bool Enabled { get; set; } = true;
    }
}