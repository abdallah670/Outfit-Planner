using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.DTOs.Wardrobe;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Domain.Enums;
using OutfitPlanner.Domain.ValueObjects;

namespace OutfitPlanner.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WardrobeController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<WardrobeController> _logger;

    public WardrobeController(
        IMediator mediator,
        ILogger<WardrobeController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    // [HttpGet]
    // public async Task<ActionResult<IEnumerable<ClothingItemDto>>> GetAll()
    // {
    //     var userId = GetUserId();
    //     var items = await _mediator.Send(new GetAllClothingItemsRequest(userId));
    //     return Ok(items.Select(MapToDto));
    // }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ClothingItemDto>> GetById(Guid id)
    {
        var item = await _mediator.Send(new GetClothingItemByIdRequest(id));
        if (item == null || item.UserId != GetUserId())
            return NotFound();
        return Ok(MapToDto(item));
    }

    // [HttpGet("category/{category}")]
    // public async Task<ActionResult<IEnumerable<ClothingItemDto>>> GetByCategory(string category)
    // {
    //     var userId = GetUserId();
    //     var items = await _clothingItemRepository.GetByCategoryAsync(userId, category);
    //     return Ok(items.Select(MapToDto));
    // }

    // [HttpPost]
    // public async Task<ActionResult<ClothingItemDto>> Create([FromBody] CreateClothingItemRequest request)
    // {
    //     var userId = GetUserId();

    //     var item = new ClothingItem
    //     {
    //         UserId = userId,
    //         Name = request.Name,
    //         Type = Enum.Parse<ClothingType>(request.Type, true),
    //         Category = request.Category,
    //         PrimaryColor = request.PrimaryColor,
    //         SecondaryColors = request.SecondaryColors,
    //         Fabric = Enum.Parse<FabricType>(request.Fabric, true),
    //         Brand = request.Brand,
    //         PurchasePrice = Money.From(request.PurchasePrice, request.Currency),
    //         PurchaseDate = request.PurchaseDate,
    //         Size = request.Size,
    //         Condition = request.Condition,
    //         ImageUrl = request.ImageUrl,
    //         ThumbnailUrl = request.ThumbnailUrl,
    //         MaintenanceNotes = request.MaintenanceNotes,
    //         Tags = request.Tags.Select(t => new ClothingTag
    //         {
    //             Name = t,
    //             Source = "manual",
    //             Confidence = 1.0m
    //         }).ToList()
    //     };

    //     await _clothingItemRepository.AddAsync(item);
    //     await _unitOfWork.SaveChangesAsync();

    //     _logger.LogInformation("User {UserId} created clothing item {ItemId}", userId, item.Id);
    //     return CreatedAtAction(nameof(GetById), new { id = item.Id }, MapToDto(item));
    // }

    // [HttpPut("{id:guid}")]
    // public async Task<ActionResult<ClothingItemDto>> Update(Guid id, [FromBody] UpdateClothingItemRequest request)
    // {
    //     var item = await _clothingItemRepository.GetByIdAsync(id);
    //     if (item == null || item.UserId != GetUserId())
    //         return NotFound();

    //     item.Name = request.Name;
    //     item.Type = Enum.Parse<ClothingType>(request.Type, true);
    //     item.Category = request.Category;
    //     item.PrimaryColor = request.PrimaryColor;
    //     item.SecondaryColors = request.SecondaryColors;
    //     item.Fabric = Enum.Parse<FabricType>(request.Fabric, true);
    //     item.Brand = request.Brand;
    //     item.PurchasePrice = Money.From(request.PurchasePrice, request.Currency);
    //     item.PurchaseDate = request.PurchaseDate;
    //     item.Size = request.Size;
    //     item.Condition = request.Condition;
    //     item.ImageUrl = request.ImageUrl;
    //     item.ThumbnailUrl = request.ThumbnailUrl;
    //     item.MaintenanceNotes = request.MaintenanceNotes;

    //     await _clothingItemRepository.UpdateAsync(item);
    //     await _unitOfWork.SaveChangesAsync();

    //     _logger.LogInformation("User {UserId} updated clothing item {ItemId}", GetUserId(), id);
    //     return Ok(MapToDto(item));
    // }

    // [HttpDelete("{id:guid}")]
    // public async Task<ActionResult> Delete(Guid id)
    // {
    //     var item = await _clothingItemRepository.GetByIdAsync(id);
    //     if (item == null || item.UserId != GetUserId())
    //         return NotFound();

    //     item.IsActive = false; // Soft delete
    //     await _clothingItemRepository.UpdateAsync(item);
    //     await _unitOfWork.SaveChangesAsync();

    //     _logger.LogInformation("User {UserId} soft-deleted clothing item {ItemId}", GetUserId(), id);
    //     return NoContent();
    // }

    // [HttpPost("{id:guid}/wear")]
    // public async Task<ActionResult<ClothingItemDto>> RecordWear(Guid id)
    // {
    //     var item = await _clothingItemRepository.GetByIdAsync(id);
    //     if (item == null || item.UserId != GetUserId())
    //         return NotFound();

    //     item.WearCount++;
    //     item.LastWorn = DateTimeOffset.UtcNow;
    //     await _clothingItemRepository.UpdateAsync(item);
    //     await _unitOfWork.SaveChangesAsync();

    //     _logger.LogInformation("User {UserId} recorded wear for item {ItemId} (count: {Count})", GetUserId(), id, item.WearCount);
    //     return Ok(MapToDto(item));
    // }

    // // --- Mapping ---

    // private static ClothingItemDto MapToDto(ClothingItem item) => new()
    // {
    //     Id = item.Id,
    //     UserId = item.UserId,
    //     Name = item.Name,
    //     Type = item.Type.ToString(),
    //     Category = item.Category,
    //     PrimaryColor = item.PrimaryColor,
    //     SecondaryColors = item.SecondaryColors,
    //     Fabric = item.Fabric.ToString(),
    //     Brand = item.Brand,
    //     PurchasePrice = item.PurchasePrice.Amount,
    //     Currency = item.PurchasePrice.Currency,
    //     PurchaseDate = item.PurchaseDate,
    //     Size = item.Size,
    //     Condition = item.Condition,
    //     ImageUrl = item.ImageUrl,
    //     ThumbnailUrl = item.ThumbnailUrl,
    //     IsActive = item.IsActive,
    //     LastWorn = item.LastWorn,
    //     WearCount = item.WearCount,
    //     LastWashed = item.LastWashed,
    //     MaintenanceNotes = item.MaintenanceNotes,
    //     Tags = item.Tags.Select(t => new ClothingTagDto
    //     {
    //         Id = t.Id,
    //         Name = t.Name,
    //         Source = t.Source,
    //         Confidence = t.Confidence
    //     }).ToList(),
    //     CreatedAt = item.CreatedAt
    // };
}
