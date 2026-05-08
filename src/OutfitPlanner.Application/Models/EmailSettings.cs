namespace OutfitPlanner.Application.Models
{
    public class EmailSettings
    {
        public string From { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; }
        public string SmtpUsername { get; set; } = string.Empty;
        public string SmtpPassword { get; set; } = string.Empty;
        public bool EnableSsl { get; set; }
    }
}
