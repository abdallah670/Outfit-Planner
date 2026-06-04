namespace OutfitPlanner.Application.Models
{
    public class AISettings
    {
        public const string SectionName = "AI";
        
        public string ApiKey { get; set; } = string.Empty;
        public string Endpoint { get; set; } = "https://api.openai.com/v1/chat/completions";
        public string ModelName { get; set; } = "gpt-3.5-turbo";
        public int MaxTokens { get; set; } = 1024;
        public double Temperature { get; set; } = 0.7;
        public int MaxHistoryMessages { get; set; } = 10;
        public int CacheMinutes { get; set; } = 30;
    }
}