namespace OutfitPlanner.Application.DTOs.Social;

/// <summary>
/// Request to update a poll
/// </summary>
public class UpdatePollRequest
{
    public string? Question { get; set; }
    public string? Context { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public List<UpdatePollOptionRequest>? Options { get; set; }
}

/// <summary>
/// Request to update a poll option
/// </summary>
public class UpdatePollOptionRequest
{
    public Guid? Id { get; set; } // Null for new options
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public Guid? OutfitId { get; set; }
}
