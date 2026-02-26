using FluentValidation;
using OutfitPlanner.Domain.Enums;

namespace OutfitPlanner.Application.DTOs.Wardrobe.Validators;

public class CreateClothingItemRequestValidator : AbstractValidator<CreateClothingItemDto>
{
    public CreateClothingItemRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Type is required")
            .Must(BeValidClothingType).WithMessage($"Type must be a valid value: {string.Join(", ", Enum.GetNames<ClothingType>())}");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required")
            .MaximumLength(50).WithMessage("Category must not exceed 50 characters");

        RuleFor(x => x.PrimaryColor)
            .NotEmpty().WithMessage("Primary color is required")
            .MaximumLength(30).WithMessage("Primary color must not exceed 30 characters");

        RuleFor(x => x.SecondaryColors)
            .Must(colors => colors == null || colors.Count <= 5)
            .WithMessage("Cannot have more than 5 secondary colors");

        RuleForEach(x => x.SecondaryColors)
            .MaximumLength(30).WithMessage("Secondary color must not exceed 30 characters");

        RuleFor(x => x.Fabric)
            .NotEmpty().WithMessage("Fabric is required")
            .Must(BeValidFabricType).WithMessage($"Fabric must be a valid value: {string.Join(", ", Enum.GetNames<FabricType>())}");

        RuleFor(x => x.Brand)
            .MaximumLength(50).WithMessage("Brand must not exceed 50 characters");

        RuleFor(x => x.PurchasePrice)
            .GreaterThanOrEqualTo(0).WithMessage("Purchase price must be a non-negative value");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .Length(3).WithMessage("Currency must be a 3-letter ISO code");

        RuleFor(x => x.PurchaseDate)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Purchase date cannot be in the future");

        RuleFor(x => x.Size)
            .MaximumLength(20).WithMessage("Size must not exceed 20 characters");

        RuleFor(x => x.Condition)
            .NotEmpty().WithMessage("Condition is required")
            .Must(BeValidCondition).WithMessage("Condition must be one of: new, excellent, good, fair, poor");

        RuleFor(x => x.ImageUrl)
            .MaximumLength(500).WithMessage("Image URL must not exceed 500 characters")
            .Must(BeValidUrl).When(x => !string.IsNullOrEmpty(x.ImageUrl))
            .WithMessage("Image URL must be a valid URL");

        RuleFor(x => x.ThumbnailUrl)
            .MaximumLength(500).WithMessage("Thumbnail URL must not exceed 500 characters")
            .Must(BeValidUrl).When(x => !string.IsNullOrEmpty(x.ThumbnailUrl))
            .WithMessage("Thumbnail URL must be a valid URL");

        RuleFor(x => x.MaintenanceNotes)
            .MaximumLength(500).WithMessage("Maintenance notes must not exceed 500 characters");

        RuleFor(x => x.Tags)
            .Must(tags => tags == null || tags.Count <= 10)
            .WithMessage("Cannot have more than 10 tags");

        RuleForEach(x => x.Tags)
            .MaximumLength(30).WithMessage("Tag must not exceed 30 characters");
    }

    private static bool BeValidClothingType(string type)
    {
        return Enum.TryParse<ClothingType>(type, ignoreCase: true, out _);
    }

    private static bool BeValidFabricType(string fabric)
    {
        return Enum.TryParse<FabricType>(fabric, ignoreCase: true, out _);
    }

    private static bool BeValidCondition(string condition)
    {
        var validConditions = new[] { "new", "excellent", "good", "fair", "poor" };
        return validConditions.Contains(condition.ToLowerInvariant());
    }

    private static bool BeValidUrl(string url)
    {
        if (string.IsNullOrEmpty(url)) return true;
        if (url.StartsWith("uploads/") || url.StartsWith("/uploads/")) return true;
        
        return Uri.TryCreate(url, UriKind.Absolute, out var result) 
               && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
}
