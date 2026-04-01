using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutfitPlanner.Application.DTOs.Social;
using OutfitPlanner.Application.Features.Social.Requests.Commands;
using OutfitPlanner.Application.Features.Social.Requests.Queries;

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
    /// Get trending outfits
    /// </summary>
    [HttpGet("outfits")]
    public async Task<ActionResult<List<TrendingOutfitDto>>> GetTrendingOutfits()
    {
        // For now, we reuse the existing request but the handler can be extended 
        // to use the DailyTrendingOutfits table we just created.
        var result = await _mediator.Send(new GetTrendingOutfitsRequest());
        return Ok(result);
    }
}
