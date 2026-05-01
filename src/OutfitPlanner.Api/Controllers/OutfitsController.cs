using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using OutfitPlanner.Application.Contracts;
using OutfitPlanner.Application.Features.Outfits.Requests.Commands;
using OutfitPlanner.Application.Features.Outfits.Requests.Queries;

using OutfitPlanner.Application.DTOs.Outfit;
using OutfitPlanner.Application.Contracts.Infrastructure;
using OutfitPlanner.Application.Exceptions;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OutfitsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<OutfitsController> _logger;
    private readonly IImageCombinationService _imageService;
    private readonly IOutfitImageCacheService _imageCacheService;
    private readonly IWebHostEnvironment _environment;

    public OutfitsController(
        IMediator mediator, 
        ILogger<OutfitsController> logger,
        IImageCombinationService imageService,
        IOutfitImageCacheService imageCacheService,
        IWebHostEnvironment environment)
    {
        _mediator = mediator;
        _logger = logger;
        _imageService = imageService;
        _imageCacheService = imageCacheService;
        _environment = environment;
    }

    private string GetUserId() => User.FindFirstValue("uid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    /// <summary>
    /// Gets all outfits for the authenticated user
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<OutfitDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<OutfitDto>>> GetAll()
    {
        var userId = GetUserId();
        var outfits = await _mediator.Send(new GetOutfitsRequest { UserId = userId });
        return Ok(outfits);
    }

    /// <summary>
    /// Gets filtered and paginated outfits
    /// </summary>
    [HttpGet("filtered")]
    [ProducesResponseType(typeof(PagedResult<OutfitDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<OutfitDto>>> GetFiltered(
        [FromQuery] string? occasion,
        [FromQuery] string? season,
        [FromQuery] string? search,
        [FromQuery] string? sortBy,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();
        var result = await _mediator.Send(new GetFilteredOutfitsRequest
        {
            UserId = userId,
            Occasion = occasion,
            Season = season,
            SearchQuery = search,
            SortBy = sortBy,
            Page = page,
            PageSize = pageSize
        });
        return Ok(result);
    }

    /// <summary>
    /// Gets a specific outfit by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OutfitDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OutfitDto>> GetById(Guid id)
    {
        var userId = GetUserId();
        var outfit = await _mediator.Send(new GetOutfitByIdRequest { Id = id, UserId = userId });
        
        if (outfit == null)
            return NotFound();

        return Ok(outfit);
    }

    /// <summary>
    /// Creates a new outfit
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(OutfitDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(BaseCommandResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OutfitDto>> Create([FromBody] CreateOutfitDto request)
    {
        var userId = GetUserId();
        var command = new CreateOutfitCommand { UserId = userId, Request = request };
        var response = await _mediator.Send(command);

        if (response == null)
            return BadRequest("Failed to create outfit");

        _logger.LogInformation("User {UserId} created outfit {OutfitId}", userId, response.Id);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    /// <summary>
    /// Creates a new outfit with a photo upload (no clothing items required)
    /// </summary>
    [HttpPost("with-photo")]
    [ProducesResponseType(typeof(CreateOutfitWithPhotoResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(BaseCommandResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreateOutfitWithPhotoResponseDto>> CreateWithPhoto(
        [FromForm] string name,
        [FromForm] string? occasion,
        [FromForm] string? season,
        [FromForm] string? weatherCondition,
        [FromForm] IFormFile photo)
    {
        var userId = GetUserId();
        
        if (photo == null || photo.Length == 0)
        {
            return BadRequest(new BaseCommandResponse 
            { 
                Success = false, 
                Message = "Photo is required",
                Errors = new List<string> { "Photo file is required" }
            });
        }

        var command = new CreateOutfitWithPhotoCommand 
        { 
            UserId = userId, 
            Name = name,
            Occasion = occasion,
            Season = season,
            WeatherCondition = weatherCondition,
            Photo = photo
        };
        
        var response = await _mediator.Send(command);

        if (response == null)
            return BadRequest(new BaseCommandResponse 
            { 
                Success = false, 
                Message = "Failed to create outfit with photo" 
            });

        _logger.LogInformation("User {UserId} created outfit {OutfitId} with photo", userId, response.Id);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    /// <summary>
    /// Updates an existing outfit
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(OutfitDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseCommandResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OutfitDto>> Update(Guid id, [FromBody] UpdateOutfitDto request)
    {
        var userId = GetUserId();
        var command = new UpdateOutfitCommand { Id = id, UserId = userId, Request = request };
        var result = await _mediator.Send(command);
        
        _logger.LogInformation("User {UserId} updated outfit {OutfitId}", userId, id);
        return Ok(result);
    }

    /// <summary>
    /// Deletes an outfit
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid id)
    {
        var userId = GetUserId();
        var command = new DeleteOutfitCommand { Id = id, UserId = userId };
        var response = await _mediator.Send(command);

        if (!response.Success)
            return NotFound(response);

        // Also delete cached image
        await _imageCacheService.DeleteCachedImageAsync(id);

        _logger.LogInformation("User {UserId} deleted outfit {OutfitId}", userId, id);
        return NoContent();
    }

    /// <summary>
    /// Records a wear event for an outfit
    /// </summary>
    [HttpPost("{id:guid}/wear")]
    [ProducesResponseType(typeof(OutfitDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OutfitDto>> RecordWear(Guid id, [FromBody] RecordOutfitWearDto dto)
    {
        var userId = GetUserId();
        var command = new RecordOutfitWearCommand 
        { 
            UserId = userId, 
            OutfitId = id,
            WornAt = dto.WornAt,
            WeatherCondition = dto.WeatherCondition,
            EventId = dto.EventId
        };
        var response = await _mediator.Send(command);

        if (response == null)
            return BadRequest("Failed to record wear");

        return Ok(response);
    }

    /// <summary>
    /// Gets a smart outfit recommendation for today based on:
    /// - User's current location/weather
    /// - Today's calendar events
    /// - User's style preferences (colors, style profile)
    /// - Outfit wear history
    /// </summary>
    /// <param name="lat">Latitude (optional, uses browser geolocation)</param>
    /// <param name="lon">Longitude (optional, uses browser geolocation)</param>
    [HttpGet("today")]
    [ProducesResponseType(typeof(TodaysPickResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TodaysPickResult>> GetToday(
        [FromQuery] double? lat = null,
        [FromQuery] double? lon = null,
        [FromQuery] DateTime? date = null)
    {
        var userId = GetUserId();
        
        _logger.LogInformation("Getting today's pick for user {UserId} with location: {Lat}, {Lon}, date: {Date}", 
            userId, lat, lon, date);

        var query = new GetTodaysPickQuery
        {
            UserId = userId,
            Latitude = lat,
            Longitude = lon,
            Date = date.HasValue ? new DateTimeOffset(date.Value.Date) : null
        };
        
        var result = await _mediator.Send(query);
        
        if (result.Outfit == null)
            return NotFound("No suitable outfit found for today. Add some outfits to your wardrobe!");

        return Ok(result);
    }

    /// <summary>
    /// Generates outfit suggestions based on criteria
    /// </summary>
    [HttpPost("generate")]
    [ProducesResponseType(typeof(List<OutfitDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<OutfitDto>>> GenerateSuggestions([FromBody] OutfitSuggestionsDto dto)
    {
        var userId = GetUserId();
        var query = new GenerateOutfitSuggestionsQuery 
        { 
            UserId = userId, 
            OutfitSuggestionsDto = dto 
        };
        var suggestions = await _mediator.Send(query);
        return Ok(suggestions);
    }

    /// <summary>
    /// Generates a combined image for an outfit using AI guide principles.
    /// Returns cached image if available, otherwise generates and caches a new one.
    /// </summary>
    [HttpGet("{id:guid}/combined-image")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetCombinedImage(Guid id)
    {
        try
        {
            // First, check if we have a cached image
            var cachedImage = await _imageCacheService.GetCachedImageAsync(id);
            if (cachedImage != null)
            {
                _logger.LogInformation("Returning cached image for outfit {OutfitId}", id);
                return File(cachedImage, "image/jpeg", $"outfit-{id}.jpg");
            }
           

            var userId = GetUserId();
            var outfit = await _mediator.Send(new GetOutfitByIdRequest { Id = id, UserId = userId });
           
            if (outfit == null || outfit.Items == null || !outfit.Items.Any())
                return NotFound("Outfit not found or has no items");
            var existingImageUrl = outfit.ImageUrl;
            if (!string.IsNullOrEmpty(existingImageUrl))
            {
                // Read the existing cached image and return it
                var imagePath = Path.Combine(_environment.WebRootPath, existingImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    var imageBytes = await System.IO.File.ReadAllBytesAsync(imagePath);
                    return File(imageBytes, "image/jpeg", $"outfit-{id}.jpg");
                }
            }
            // Get outfit items with their metadata
            var itemsWithImages = outfit.Items
                .Where(i => !string.IsNullOrEmpty(i.ClothingItemImageUrl))
                .ToList();

            if (!itemsWithImages.Any())
                return NotFound("No images found for outfit items");

            // Extract URLs, types, and names for smart combination
            var imageUrls = itemsWithImages.Select(i => i.ClothingItemImageUrl!).ToList();
            var clothingTypes = itemsWithImages.Select(i => i.ClothingItemType).ToList();
            var clothingNames = itemsWithImages.Select(i => i.ClothingItemName).ToList();

            // Combine images with smart layout using clothing types
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var combinedImage = await _imageService.CombineImagesAsync(
                imageUrls, 
                clothingTypes, 
                clothingNames,
                _environment.WebRootPath, 
                baseUrl);
            
            if (combinedImage == null || combinedImage.Length == 0)
            {
                _logger.LogWarning("Could not generate combined image for outfit {OutfitId} - no valid images found", id);
                return NotFound("Could not generate outfit preview - clothing item images are missing or unavailable.");
            }

            // Cache the generated image for future requests
            await _imageCacheService.CacheImageAsync(id, combinedImage);
            _logger.LogInformation("Generated and cached image for outfit {OutfitId}", id);
            
            // Update the outfit with the ImageUrl
            var updateDto = new UpdateOutfitDto { ImageUrl = $"/uploads/outfit-images/outfit-{id}.jpg" };
            await _mediator.Send(new UpdateOutfitCommand { Id = id, UserId = userId, Request = updateDto });
            return File(combinedImage, "image/jpeg", $"outfit-{id}.jpg");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating combined image for outfit {OutfitId}", id);
            return StatusCode(500, $"Error generating combined image: {ex.Message}");
        }
    }
}
