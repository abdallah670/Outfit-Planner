using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Domain.Enums;

namespace OutfitPlanner.Domain.Entities;



public class FeedPost : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public User? User { get; set; }
    
    public PostType PostType { get; set; }
    
    public Guid? OutfitId { get; set; }
    public Outfit? Outfit { get; set; }
    
    public Guid? PollId { get; set; }
    public ValidationPoll? Poll { get; set; }
    
    public string? Caption { get; set; }
    public List<string> Tags { get; set; } = new();
    
    public Visibility Visibility { get; set; } = Visibility.Public;
    
    public int LikesCount { get; set; }
    public int CommentsCount { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
    
    public ICollection<PostReaction> Reactions { get; set; } = new List<PostReaction>();
    public ICollection<PostComment> Comments { get; set; } = new List<PostComment>();
}
