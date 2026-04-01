using AutoMapper;
using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.DTOs.Social;
using OutfitPlanner.Application.Exceptions;
using OutfitPlanner.Application.Features.Social.Requests.Queries;
using Microsoft.Extensions.Logging;

namespace OutfitPlanner.Application.Features.Social.Handlers.Queries;

/// <summary>
/// Handler for GetPollsRequest
/// </summary>
public class GetPollsRequestHandler : IRequestHandler<GetPollsRequest, List<ValidationPollDto>>
{
    private readonly IValidationPollRepository _validationPollRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetPollsRequestHandler> _logger;

    public GetPollsRequestHandler(
        IValidationPollRepository validationPollRepository,
        IMapper mapper,
        ILogger<GetPollsRequestHandler> logger)
    {
        _validationPollRepository = validationPollRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<List<ValidationPollDto>> Handle(GetPollsRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var polls = await _validationPollRepository.GetByUserIdAsync(request.UserId);
            
            if (polls == null)
            {
                _logger.LogInformation("No polls found for user {UserId}", request.UserId);
                return new List<ValidationPollDto>();
            }

            var pollList = polls.ToList();
            var result = new List<ValidationPollDto>();

            foreach (var poll in pollList)
            {
                // Load options and votes
                var pollWithDetails = await _validationPollRepository.GetByIdAsync(poll.Id);
                if (pollWithDetails != null)
                {
                    result.Add(_mapper.Map<ValidationPollDto>(pollWithDetails));
                }
            }

            _logger.LogInformation("Retrieved {Count} polls for user {UserId}", result.Count, request.UserId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting polls for user {UserId}", request.UserId);
            throw new BadRequestException("Error retrieving polls");
        }
    }
}
