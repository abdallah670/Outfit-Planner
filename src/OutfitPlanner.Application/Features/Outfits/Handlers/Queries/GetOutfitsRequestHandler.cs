using AutoMapper;
using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.DTOs.Outfit;
using OutfitPlanner.Application.Features.Outfits.Requests.Queries;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Exceptions;

namespace OutfitPlanner.Application.Features.Outfits.Handlers.Queries;

public class GetOutfitsRequestHandler : IRequestHandler<GetOutfitsRequest, List<OutfitListDto>>
{
    private readonly IOutfitRepository _outfitRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetOutfitsRequestHandler> _logger;

    public GetOutfitsRequestHandler(IOutfitRepository outfitRepository, IMapper mapper, ILogger<GetOutfitsRequestHandler> logger)
    {
        _outfitRepository = outfitRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<List<OutfitListDto>> Handle(GetOutfitsRequest request, CancellationToken cancellationToken)
    {
        try{
            var outfits=await _outfitRepository.GetByUserIdAsync(request.UserId);
            if(outfits==null){
                _logger.LogInformation("No outfits found for user with ID {UserId}", request.UserId);
                return new List<OutfitListDto>();
            }
            return _mapper.Map<List<OutfitListDto>>(outfits);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting outfits");
            throw new BadRequestException("Error getting outfits");
        }

      
    }
}
