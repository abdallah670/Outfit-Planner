using AutoMapper;
using FluentValidation;
using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.DTOs.Outfit;
using OutfitPlanner.Application.Features.Outfits.Requests.Commands;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Exceptions;
using OutfitPlanner.Domain.Entities;
using ValidationException = OutfitPlanner.Application.Exceptions.ValidationException;

namespace OutfitPlanner.Application.Features.Outfits.Handlers.Commands;

public class UpdateOutfitCommandHandler : IRequestHandler<UpdateOutfitCommand, OutfitDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<UpdateOutfitCommand> _validator;
    private readonly ILogger<UpdateOutfitCommandHandler> _logger;

    public UpdateOutfitCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IValidator<UpdateOutfitCommand> validator,
        ILogger<UpdateOutfitCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public async Task<OutfitDto> Handle(UpdateOutfitCommand request, CancellationToken cancellationToken)
    {
        try{
            var validationResult=await _validator.ValidateAsync(request, cancellationToken);
            if(!validationResult.IsValid){
                throw new ValidationException(validationResult);
            }
            var outfit =await _unitOfWork.Outfits.GetByIdAsync(request.Id);
            if(outfit==null){
                throw new NotFoundException(nameof(Outfit), request.Id);
            }
            _mapper.Map(request.Request, outfit);
            await _unitOfWork.Outfits.UpdateAsync(outfit);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return _mapper.Map<OutfitDto>(outfit);
        }
        catch(Exception ex){
            _logger.LogError(ex, "Error updating outfit {Id}", request.Id);
            throw;
        }
      
    }
}
