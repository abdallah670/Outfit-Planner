using AutoMapper;
using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Features.Outfits.Requests.Commands;
using OutfitPlanner.Application.Responses;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Exceptions;

namespace OutfitPlanner.Application.Features.Outfits.Handlers.Commands;

public class DeleteOutfitCommandHandler : IRequestHandler<DeleteOutfitCommand, BaseCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<DeleteOutfitCommandHandler> _logger;

    public DeleteOutfitCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<DeleteOutfitCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<BaseCommandResponse> Handle(DeleteOutfitCommand request, CancellationToken cancellationToken)
    {
       try{
          var outfit=await _unitOfWork.Outfits.GetByIdAsync(request.Id);
          if(outfit==null){
            return new BaseCommandResponse{
                Success=false,
                Message="Outfit not found",
                Errors=new List<string>{ "Outfit not found" }
            };
          }
          
          await _unitOfWork.Outfits.RemoveAsync(outfit);
          await _unitOfWork.SaveChangesAsync(cancellationToken);
          return new BaseCommandResponse{
            Success=true,
            Message="Outfit deleted successfully"
          };

        
       }
       catch(Exception ex){
        _logger.LogError(ex, "Error deleting outfit");
        throw new BadRequestException("Error deleting outfit");
       }
    }
}
