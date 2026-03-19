namespace OutfitPlanner.Application.DTOs.User;

public class ConnectAccountRequestDto
{
    public string Provider { get; set; } = string.Empty;
    public string ReturnUrl { get; set; } = string.Empty;
}
