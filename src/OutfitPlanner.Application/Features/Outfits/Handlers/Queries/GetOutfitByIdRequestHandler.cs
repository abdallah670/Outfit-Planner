using AutoMapper;
using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.DTOs.Outfit;
using OutfitPlanner.Application.Features.Outfits.Requests.Queries;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Exceptions;

namespace OutfitPlanner.Application.Features.Outfits.Handlers.Queries;

public class GetOutfitByIdRequestHandler : IRequestHandler<GetOutfitByIdRequest, OutfitDto>
{
    private readonly IOutfitRepository _outfitRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetOutfitByIdRequestHandler> _logger;

    public GetOutfitByIdRequestHandler(IOutfitRepository outfitRepository, IMapper mapper, ILogger<GetOutfitByIdRequestHandler> logger)
    {
        _outfitRepository = outfitRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<OutfitDto> Handle(GetOutfitByIdRequest request, CancellationToken cancellationToken)
    {
        try{
            var outfit=await _outfitRepository.GetByIdAsync(request.Id);
            if(outfit==null){
                _logger.LogInformation("Outfit with ID {Id} not found", request.Id);
                throw new NotFoundException("Outfit", request.Id);
            }
            return _mapper.Map<OutfitDto>(outfit);

        }
        catch(NotFoundException){
            throw;
        }
        catch(Exception ex){
            _logger.LogError(ex, "Error getting outfit by ID");
            throw new BadRequestException("Error getting outfit by ID");
        }
    }
}
