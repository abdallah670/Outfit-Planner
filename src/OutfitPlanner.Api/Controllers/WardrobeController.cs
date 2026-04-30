using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using OutfitPlanner.Application.Contracts.Infrastructure;

using OutfitPlanner.Application.DTOs.Wardrobe;
using OutfitPlanner.Application.Features.ClothingItems.Requests.Commands;
using OutfitPlanner.Application.Features.ClothingItems.Requests.Queries;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WardrobeController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<WardrobeController> _logger;

    private readonly IImageStorageService _imageStorageService;

    public WardrobeController(
        IMediator mediator,
        ILogger<WardrobeController> logger,
        IImageStorageService imageStorageService)
    {
        _mediator = mediator;
        _logger = logger;
        _imageStorageService = imageStorageService;
    }

    private string GetUserId() => User.FindFirstValue("uid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    /// <summary>
    /// Gets all clothing items for the authenticated user
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ClothingItemListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<ClothingItemListDto>>> GetAll()
    {
        var userId = GetUserId();
        var items = await _mediator.Send(new GetClothingItemListRequest { UserId = userId });
        return Ok(items);
    }

    /// <summary>
    /// Gets filtered and paginated clothing items
    /// </summary>
    [HttpGet("filtered")]
    [ProducesResponseType(typeof(PagedResult<ClothingItemListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResult<ClothingItemListDto>>> GetFiltered(
        [FromQuery] string? category,
        [FromQuery] string? color,
        [FromQuery] string? occasion,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();
        var result = await _mediator.Send(new GetFilteredClothingItemsRequest
        {
            UserId = userId,
            Category = category,
            Color = color,
            Occasion = occasion,
            SearchQuery = search,
            Page = page,
            PageSize = pageSize
        });
        return Ok(result);
    }

    /// <summary>
    /// Gets a specific clothing item by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ClothingItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ClothingItemDto>> GetById(Guid id)
    {
        var userId = GetUserId();
        var item = await _mediator.Send(new GetClothingItemByIdRequest { Id = id, UserId = userId });
        return Ok(item);
    }

    /// <summary>
    /// Gets clothing items filtered by category
    /// </summary>
    [HttpGet("category/{category}")]
    [ProducesResponseType(typeof(List<ClothingItemListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<ClothingItemListDto>>> GetByCategory(string category)
    {
        var userId = GetUserId();
        var items = await _mediator.Send(new GetClothingItemsByCategoryRequest
        {
            UserId = userId,
            Category = category
        });
        return Ok(items);
    }

    /// <summary>
    /// Creates a new clothing item
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ClothingItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(BaseCommandResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ClothingItemDto>> Create([FromForm] CreateClothingItemDto request, IFormFile? image)
    {
        var userId = GetUserId();

        if (image != null)
        {
            var uploadResult = await _imageStorageService.UploadImageAsync(
                image.OpenReadStream(),
                image.FileName,
                userId);

            if (uploadResult.Success)
            {
                request.ImageUrl = uploadResult.OriginalPath;
                request.ThumbnailUrl = uploadResult.ThumbnailPath;
            }
        }

        var command = new CreateClothingItemCommand
        {
            UserId = userId,
            Request = request
        };
        var response = await _mediator.Send(command);

        if (response == null)
            return BadRequest(new BaseCommandResponse { Success = false, Message = "Failed to create clothing item" });

        _logger.LogInformation("User {UserId} created clothing item {ItemId}", userId, response.Id);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    /// <summary>
    /// Updates an existing clothing item
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ClothingItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseCommandResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ClothingItemDto>> Update(Guid id, [FromForm] UpdateClothingItemDto request, IFormFile? image)
    {
        var userId = GetUserId();

        if (image != null)
        {
            var uploadResult = await _imageStorageService.UploadImageAsync(
                image.OpenReadStream(),
                image.FileName,
                userId);

            if (uploadResult.Success)
            {
                request.ImageUrl = uploadResult.OriginalPath;
                request.ThumbnailUrl = uploadResult.ThumbnailPath;
            }
        }

        var command = new UpdateClothingItemCommand
        {
            Id = id,
            UserId = userId,
            Request = request
        };
        var result = await _mediator.Send(command);
        _logger.LogInformation("User {UserId} updated clothing item {ItemId}", GetUserId(), id);
        return Ok(result);
    }

    /// <summary>
    /// Soft deletes a clothing item
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> Delete(Guid id)
    {
        var userId = GetUserId();
        var command = new DeleteClothingItemCommand
        {
            Id = id,
            UserId = userId
        };
        var response = await _mediator.Send(command);

        if (!response.Success)
            return NotFound(response);

        _logger.LogInformation("User {UserId} soft-deleted clothing item {ItemId}", GetUserId(), id);
        return NoContent();
    }

    /// <summary>
    /// Records a wear event for a clothing item
    /// </summary>
    [HttpPost("{id:guid}/wear")]
    [ProducesResponseType(typeof(ClothingItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ClothingItemDto>> RecordWear(Guid id, [FromBody] RecordWearDto dto)
    {
        var userId = GetUserId();

        // Ensure DTO matches route parameter
        if (dto.ClothingItemId != id)
        {
            return BadRequest(new BaseCommandResponse
            {
                Success = false,
                Message = "Clothing item ID in route does not match DTO"
            });
        }

        var command = new RecordWearCommand
        {
            UserId = userId,
            Request = dto
        };
        var response = await _mediator.Send(command);

        return Ok(response);
    }

    /// <summary>
    /// Quick wear - just records that the item was worn now
    /// </summary>
    [HttpPost("{id:guid}/wear/quick")]
    [ProducesResponseType(typeof(ClothingItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ClothingItemDto>> QuickWear(Guid id)
    {
        var userId = GetUserId();
        var dto = new RecordWearDto
        {
            ClothingItemId = id,
            WornAt = DateTimeOffset.UtcNow
        };

        var command = new RecordWearCommand { UserId = userId, Request = dto };
        var response = await _mediator.Send(command);

        return Ok(response);
    }

    /// <summary>
    /// Gets wardrobe health statistics
    /// </summary>
    [HttpGet("health")]
    [ProducesResponseType(typeof(WardrobeHealthDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<WardrobeHealthDto>> GetHealth()
    {
        var userId = GetUserId();
        var health = await _mediator.Send(new GetWardrobeHealthRequest { UserId = userId });
        return Ok(health);
    }
}
