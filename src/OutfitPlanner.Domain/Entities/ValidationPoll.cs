using OutfitPlanner.Domain.Enums;

namespace OutfitPlanner.Domain.Entities;

public class ValidationPoll : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public User User { get; set; } = null!;

    public string Question { get; set; } = string.Empty;
    public string Context { get; set; } = "{}"; // JSON stored as string
    public DateTimeOffset ExpiresAt { get; set; }
    public PollStatus Status { get; set; } = PollStatus.Active;
    
    public ICollection<PollOption> Options { get; set; } = new List<PollOption>();
    public ICollection<Vote> Votes { get; set; } = new List<Vote>(); // Relation? Poll->Votes direct? Diagram says ValidationPolls ||--o{ Votes
}

public class PollOption : BaseEntity
{
    public Guid PollId { get; set; }
    public ValidationPoll Poll { get; set; } = null!;
    
    public Guid? OutfitId { get; set; }
    public Outfit? Outfit { get; set; }
    
    public string Description { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    
    public ICollection<Vote> Votes { get; set; } = new List<Vote>();
}

public class Vote : BaseEntity
{
    public Guid PollId { get; set; }
    
    public Guid OptionId { get; set; }
    public PollOption Option { get; set; } = null!;
    
    public string VoterId { get; set; } = string.Empty;
    public User Voter { get; set; } = null!;
    
    public int Rating { get; set; } // 1-5
    public bool IsAnonymous { get; set; }
    
}
