namespace OutfitPlanner.Application.DTOs.User;

public class StyleRuleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string CriteriaJson { get; set; } = "{}";
}

public class CreateStyleRuleDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CriteriaJson { get; set; } = "{}";
}

public class UpdateStyleRuleDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string CriteriaJson { get; set; } = "{}";
}
