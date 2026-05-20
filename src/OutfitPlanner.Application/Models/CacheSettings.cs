namespace OutfitPlanner.Application.Models
{
    public class CacheSettings
    {
        public const string SectionName = "CacheSettings";

        public int DefaultExpirationMinutes { get; set; } = 30;
        public int MaxCacheSizeMB { get; set; } = 100;
        public bool EnableSlidingExpiration { get; set; } = true;
        public int SlidingExpirationMinutes { get; set; } = 15;
    }
}