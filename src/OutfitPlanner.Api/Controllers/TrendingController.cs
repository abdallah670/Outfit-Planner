using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutfitPlanner.Application.DTOs.Social;
using OutfitPlanner.Application.Features.Social.Requests.Commands;
using OutfitPlanner.Application.Features.Social.Requests.Queries;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TrendingController : ControllerBase
{
    private readonly IMediator _mediator;

    public TrendingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Trigger calculation of trending outfits (Admin or internal use usually, but here for demo)
    /// </summary>
    [HttpPost("calculate")]
    public async Task<ActionResult> CalculateTrending()
    {
        var response = await _mediator.Send(new CalculateTrendingOutfitsCommand());
        if (!response.Success)
            return BadRequest(response);
        return Ok(response);
    }

    /// <summary>
    /// Get trending outfits with pagination
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Items per page (default: 20)</param>
    [HttpGet("outfits")]
    public async Task<ActionResult<PagedResult<TrendingOutfitDto>>> GetTrendingOutfits(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var request = new GetTrendingOutfitsRequest
        {
            Page = page,
            PageSize = pageSize
        };
        var result = await _mediator.Send(request);
        return Ok(result);
    }
}
