namespace OutfitPlanner.Application.Models
{
    public class MaintenanceSettings
    {
        public const string SectionName = "MaintenanceSettings";

        public string DefaultMessage { get; set; } = "System is currently under maintenance. Please try again later.";
        public List<string> AllowedIpAddresses { get; set; } = new() { "127.0.0.1", "::1" };
        public bool BypassMaintenanceForAdmins { get; set; } = true;
    }
}