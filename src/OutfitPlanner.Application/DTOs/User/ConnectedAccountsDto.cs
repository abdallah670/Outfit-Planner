namespace OutfitPlanner.Application.DTOs.User;

public class ConnectedAccountsDto
{
    public string? Provider { get; set; }
    public string? ProviderId { get; set; }
    public bool IsConnected => !string.IsNullOrEmpty(Provider);
    public string ProviderName => Provider switch
    {
        "Google" => "Google",
        "Facebook" => "Facebook",
        "Instagram" => "Instagram",
        _ => "None"
    };
}
