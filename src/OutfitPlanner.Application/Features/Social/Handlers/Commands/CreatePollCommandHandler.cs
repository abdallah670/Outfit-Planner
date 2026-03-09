using AutoMapper;
using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.DTOs.Social;
using OutfitPlanner.Application.Exceptions;
using OutfitPlanner.Application.Features.Social.Requests.Commands;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace OutfitPlanner.Application.Features.Social.Handlers.Commands;

/// <summary>
/// Handler for CreatePollCommand
/// </summary>
public class CreatePollCommandHandler : IRequestHandler<CreatePollCommand, BaseCommandResponse>
{
    private readonly IValidationPollRepository _validationPollRepository;
    private readonly IOutfitRepository _outfitRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CreatePollCommandHandler> _logger;

    public CreatePollCommandHandler(
        IValidationPollRepository validationPollRepository,
        IOutfitRepository outfitRepository,
        IMapper mapper,
        ILogger<CreatePollCommandHandler> logger)
    {
        _validationPollRepository = validationPollRepository;
        _outfitRepository = outfitRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<BaseCommandResponse> Handle(CreatePollCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseCommandResponse();

        try
        {
            // Validation: Question not empty
            if (string.IsNullOrWhiteSpace(request.Request.Question))
            {
                response.Success = false;
                response.Message = "Question is required";
                response.Errors.Add("Question is required");
                return response;
            }

            // Validation: ExpiresAt in future
            if (request.Request.ExpiresAt <= DateTimeOffset.UtcNow)
            {
                response.Success = false;
                response.Message = "Expiration date must be in the future";
                response.Errors.Add("Expiration date must be in the future");
                return response;
            }

            // Validation: At least 2 options
            if (request.Request.Options == null || request.Request.Options.Count < 2)
            {
                response.Success = false;
                response.Message = "At least 2 options are required";
                response.Errors.Add("At least 2 options are required");
                return response;
            }

            // Validate outfit IDs if provided
            foreach (var option in request.Request.Options)
            {
                if (option.OutfitId.HasValue)
                {
                    var outfit = await _outfitRepository.GetByIdAsync(option.OutfitId.Value);
                    if (outfit == null || outfit.UserId != request.UserId)
                    {
                        response.Success = false;
                        response.Message = $"Outfit with ID {option.OutfitId} not found or does not belong to user";
                        response.Errors.Add($"Outfit with ID {option.OutfitId} not found or does not belong to user");
                        return response;
                    }
                }
            }

            // Create poll entity
            var poll = new ValidationPoll
            {
                UserId = request.UserId,
                Question = request.Request.Question,
                Context = request.Request.Context ?? "{}",
                ExpiresAt = request.Request.ExpiresAt,
                Status = PollStatus.Active,
                Options = new List<PollOption>()
            };

            // Add options
            foreach (var optionDto in request.Request.Options)
            {
                poll.Options.Add(new PollOption
                {
                    OutfitId = optionDto.OutfitId,
                    Description = optionDto.Description,
                    DisplayOrder = optionDto.DisplayOrder
                });
            }

            // Save poll
            await _validationPollRepository.AddAsync(poll);

            response.Id = poll.Id;
            response.Success = true;
            response.Message = "Poll created successfully";
            
            _logger.LogInformation("Poll {Id} created by user {UserId}", poll.Id, request.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating poll for user {UserId}", request.UserId);
            response.Success = false;
            response.Message = "Error creating poll";
            response.Errors.Add(ex.Message);
        }

        return response;
    }
}
