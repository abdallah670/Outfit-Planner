using AutoMapper;
using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Contracts.Persistence;
using OutfitPlanner.Application.DTOs.Feed;
using OutfitPlanner.Application.Exceptions;
using OutfitPlanner.Application.Features.Feed.Requests.Queries;
using Microsoft.Extensions.Logging;

namespace OutfitPlanner.Application.Features.Feed.Handlers.Queries;

/// <summary>
/// Handler for GetPollByIdRequest
/// </summary>
public class GetPollByIdRequestHandler : IRequestHandler<GetPollByIdRequest, ValidationPollDto>
{
    private readonly IValidationPollRepository _validationPollRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetPollByIdRequestHandler> _logger;

    public GetPollByIdRequestHandler(
        IValidationPollRepository validationPollRepository,
        IMapper mapper,
        ILogger<GetPollByIdRequestHandler> logger)
    {
        _validationPollRepository = validationPollRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ValidationPollDto> Handle(GetPollByIdRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var poll = await _validationPollRepository.GetByIdAsync(request.Id);
            
            if (poll == null)
            {
                _logger.LogInformation("Poll with ID {Id} not found", request.Id);
                throw new NotFoundException("Poll", request.Id);
            }

            _logger.LogInformation("Retrieved poll {Id} for user {UserId}", request.Id, request.UserId);
            return _mapper.Map<ValidationPollDto>(poll);
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting poll {Id}", request.Id);
            throw new BadRequestException("Error retrieving poll");
        }
    }
}
