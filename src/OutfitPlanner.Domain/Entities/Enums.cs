namespace OutfitPlanner.Domain.Enums;

public enum ClothingType
{
    Top,
    Bottom,
    Dress,
    Outerwear,
    Footwear,
    Accessory,
    Undergarment,
    Swimwear,
    Activewear
}

public enum OccasionType
{
    Casual,
    BusinessCasual,
    Formal,
    Athletic,
    Social,
    Work,
    Date,
    Travel
}

public enum Season
{
    Spring,
    Summer,
    Autumn,
    Winter,
    AllSeason
}

public enum StylePreference
{
    Minimalist,
    Classic,
    Bohemian,
    Streetwear,
    Professional,
    Athleisure,
    Eclectic,
    Vintage,
    
}

public enum PrivacyLevel
{
    Private,
    Follwers,
    Public
}

public enum ItemRole
{
    Primary,
    Secondary,
    Accent
}

public enum PollStatus
{
    Active,
    Closed,
    Expired
}

public enum OutfitStatus
{
    Active,
    Archived,
    Deleted
}

public enum FabricType
{
    Cotton,
    Polyester,
    Wool,
    Silk,
    Linen,
    Leather,
    Denim,
    Nylon,
    Spandex,
    Rayon,
    Other
}
public enum CalendarEventType
{
    General,
    Work,
    Meeting,
    Social,
    Date,
    Party,
    Sport,
    Travel,
    Appointment
}
public enum RecurrenceType
{
    None,
    Daily,
    Weekly,
    Monthly,
    Yearly
}
public enum PostType{
    Poll,
    Outfit
}
public enum Visibility{
    Private,
    Follwers,
    Public  
}
public enum ReactionType{
    Heart,
    Like,
    Love,
    Haha,
    Wow,
    Sad,
    Angry
}