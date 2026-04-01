using AutoMapper;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.DTOs.Feed;
using OutfitPlanner.Application.Features.Feed.Handlers.Commands;
using OutfitPlanner.Application.Features.Feed.Handlers.Queries;
using OutfitPlanner.Application.Features.Feed.Requests.Commands;
using OutfitPlanner.Application.Features.Feed.Requests.Commands.Validators;
using OutfitPlanner.Application.Features.Feed.Requests.Queries;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Domain.Enums;
using OutfitPlanner.Persistence;
using OutfitPlanner.Persistence.Repositories;
using Xunit;

namespace OutfitPlanner.Application.IntegrationTests.Social;

public class SocialTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly AppDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SocialTests()
    {
        var services = new ServiceCollection();

        // Use an in-memory database for testing
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase($"OutfitPlannerSocialTestDb_{Guid.NewGuid()}"));

        // Add logging
        services.AddLogging(b => b.AddDebug());

        // Add AutoMapper with proper mappings
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<Profiles.MappingProfile>();
        });
        services.AddSingleton<IMapper>(mapperConfig.CreateMapper());

        // Register repositories
        services.AddScoped<IClothingItemRepository, ClothingItemRepository>();
        services.AddScoped<IOutfitRepository, OutfitRepository>();
        services.AddScoped<IValidationPollRepository, ValidationPollRepository>();
        services.AddScoped<IWearEventRepository, WearEventRepository>();
        services.AddScoped<IUserStyleProfileRepository, UserStyleProfileRepository>();
        services.AddScoped<IOutfitFeedbackRepository, OutfitFeedbackRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IStyleRuleRepository, StyleRuleRepository>();
        services.AddScoped<IClothingTagRepository, ClothingTagRepository>();
        services.AddScoped<IOutfitItemRepository, OutfitItemRepository>();
        services.AddScoped<IPollOptionRepository, PollOptionRepository>();
        services.AddScoped<IVoteRepository, VoteRepository>();
        services.AddScoped<IUserPreferencesRepository, UserPreferencesRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register Validators
        services.AddValidatorsFromAssemblyContaining<CreatePollCommandValidator>();

        // Register MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreatePollCommand).Assembly));

        _serviceProvider = services.BuildServiceProvider();

        _dbContext = _serviceProvider.GetRequiredService<AppDbContext>();
        _unitOfWork = _serviceProvider.GetRequiredService<IUnitOfWork>();
        _mapper = _serviceProvider.GetRequiredService<IMapper>();
    }

    [Fact]
    public async Task CreatePoll_ShouldSucceed_WithValidData()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        
        var logger = _serviceProvider.GetRequiredService<ILogger<CreatePollCommandHandler>>();
        var handler = new CreatePollCommandHandler(
            _unitOfWork.ValidationPolls,
            _unitOfWork.Outfits,
            _mapper,
            logger);

        var command = new CreatePollCommand
        {
            UserId = userId,
            Request = new CreatePollDto
            {
                Question = "Which outfit looks better for a date?",
                Context = "Looking for suggestions for Friday night",
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
                Options = new List<CreatePollOptionDto>
                {
                    new CreatePollOptionDto { Description = "Blue dress with heels", DisplayOrder = 1 },
                    new CreatePollOptionDto { Description = "Casual jeans with blouse", DisplayOrder = 2 }
                }
            }
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Id.Should().NotBe(Guid.Empty);
        result.Message.Should().Be("Poll created successfully");
    }

    [Fact]
    public async Task CreatePoll_ShouldFail_WhenLessThanTwoOptions()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        
        var logger = _serviceProvider.GetRequiredService<ILogger<CreatePollCommandHandler>>();
        var handler = new CreatePollCommandHandler(
            _unitOfWork.ValidationPolls,
            _unitOfWork.Outfits,
            _mapper,
            logger);

        var command = new CreatePollCommand
        {
            UserId = userId,
            Request = new CreatePollDto
            {
                Question = "Which outfit looks better?",
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
                Options = new List<CreatePollOptionDto>
                {
                    new CreatePollOptionDto { Description = "Only one option", DisplayOrder = 1 }
                }
            }
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain("At least 2 options are required");
    }

    [Fact]
    public async Task CreatePoll_ShouldFail_WhenExpiredInPast()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        
        var logger = _serviceProvider.GetRequiredService<ILogger<CreatePollCommandHandler>>();
        var handler = new CreatePollCommandHandler(
            _unitOfWork.ValidationPolls,
            _unitOfWork.Outfits,
            _mapper,
            logger);

        var command = new CreatePollCommand
        {
            UserId = userId,
            Request = new CreatePollDto
            {
                Question = "Which outfit looks better?",
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(-1), // Already expired
                Options = new List<CreatePollOptionDto>
                {
                    new CreatePollOptionDto { Description = "Option 1", DisplayOrder = 1 },
                    new CreatePollOptionDto { Description = "Option 2", DisplayOrder = 2 }
                }
            }
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain("Expiration date must be in the future");
    }

    [Fact]
    public async Task VoteOnPoll_ShouldSucceed_WhenPollActive()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        
        // First create a poll
        var createLogger = _serviceProvider.GetRequiredService<ILogger<CreatePollCommandHandler>>();
        var createHandler = new CreatePollCommandHandler(
            _unitOfWork.ValidationPolls,
            _unitOfWork.Outfits,
            _mapper,
            createLogger);
        
        var pollResult = await createHandler.Handle(new CreatePollCommand
        {
            UserId = userId,
            Request = new CreatePollDto
            {
                Question = "Vote test poll",
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
                Options = new List<CreatePollOptionDto>
                {
                    new CreatePollOptionDto { Description = "Option A", DisplayOrder = 1 },
                    new CreatePollOptionDto { Description = "Option B", DisplayOrder = 2 }
                }
            }
        }, CancellationToken.None);

        // Get the option ID from the poll
        var poll = await _unitOfWork.ValidationPolls.GetByIdAsync(pollResult.Id);
        var optionId = poll!.Options.First().Id;

        // Now vote
        var voteLogger = _serviceProvider.GetRequiredService<ILogger<VoteOnPollCommandHandler>>();
        var voteHandler = new VoteOnPollCommandHandler(
            _unitOfWork.ValidationPolls,
            _unitOfWork.PollOptions,
            _unitOfWork.Votes,
            _mapper,
            voteLogger);

        var voteCommand = new VoteOnPollCommand
        {
            UserId = userId,
            PollId = pollResult.Id,
            Request = new CastVoteDto
            {
                OptionId = optionId,
                Rating = 4,
                Comment = "Looks great!",
                IsAnonymous = false
            }
        };

        // Act
        var voteResult = await voteHandler.Handle(voteCommand, CancellationToken.None);

        // Assert
        voteResult.Should().NotBeNull();
        voteResult.Success.Should().BeTrue();
        voteResult.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task VoteOnPoll_ShouldFail_WhenPollExpired()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        
        // First create an expired poll
        var createLogger = _serviceProvider.GetRequiredService<ILogger<CreatePollCommandHandler>>();
        var createHandler = new CreatePollCommandHandler(
            _unitOfWork.ValidationPolls,
            _unitOfWork.Outfits,
            _mapper,
            createLogger);
        
        var pollResult = await createHandler.Handle(new CreatePollCommand
        {
            UserId = userId,
            Request = new CreatePollDto
            {
                Question = "Expired poll test",
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(-1), // Already expired
                Options = new List<CreatePollOptionDto>
                {
                    new CreatePollOptionDto { Description = "Option A", DisplayOrder = 1 },
                    new CreatePollOptionDto { Description = "Option B", DisplayOrder = 2 }
                }
            }
        }, CancellationToken.None);

        // Get the option ID from the poll
        var poll = await _unitOfWork.ValidationPolls.GetByIdAsync(pollResult.Id);
        var optionId = poll!.Options.First().Id;

        // Try to vote
        var voteLogger = _serviceProvider.GetRequiredService<ILogger<VoteOnPollCommandHandler>>();
        var voteHandler = new VoteOnPollCommandHandler(
            _unitOfWork.ValidationPolls,
            _unitOfWork.PollOptions,
            _unitOfWork.Votes,
            _mapper,
            voteLogger);

        var voteCommand = new VoteOnPollCommand
        {
            UserId = userId,
            PollId = pollResult.Id,
            Request = new CastVoteDto
            {
                OptionId = optionId,
                Rating = 3
            }
        };

        // Act
        var voteResult = await voteHandler.Handle(voteCommand, CancellationToken.None);

        // Assert
        voteResult.Should().NotBeNull();
        voteResult.Success.Should().BeFalse();
        voteResult.Errors.Should().Contain("Poll has expired");
    }

    [Fact]
    public async Task VoteOnPoll_ShouldFail_WhenAlreadyVoted()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        
        // First create a poll
        var createLogger = _serviceProvider.GetRequiredService<ILogger<CreatePollCommandHandler>>();
        var createHandler = new CreatePollCommandHandler(
            _unitOfWork.ValidationPolls,
            _unitOfWork.Outfits,
            _mapper,
            createLogger);
        
        var pollResult = await createHandler.Handle(new CreatePollCommand
        {
            UserId = userId,
            Request = new CreatePollDto
            {
                Question = "Double vote test",
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
                Options = new List<CreatePollOptionDto>
                {
                    new CreatePollOptionDto { Description = "Option A", DisplayOrder = 1 },
                    new CreatePollOptionDto { Description = "Option B", DisplayOrder = 2 }
                }
            }
        }, CancellationToken.None);

        // Get the option IDs from the poll
        var poll = await _unitOfWork.ValidationPolls.GetByIdAsync(pollResult.Id);
        var option1Id = poll!.Options.First().Id;
        var option2Id = poll.Options.Skip(1).First().Id;

        // First vote
        var voteLogger = _serviceProvider.GetRequiredService<ILogger<VoteOnPollCommandHandler>>();
        var voteHandler = new VoteOnPollCommandHandler(
            _unitOfWork.ValidationPolls,
            _unitOfWork.PollOptions,
            _unitOfWork.Votes,
            _mapper,
            voteLogger);

        await voteHandler.Handle(new VoteOnPollCommand
        {
            UserId = userId,
            PollId = pollResult.Id,
            Request = new CastVoteDto
            {
                OptionId = option1Id,
                Rating = 5
            }
        }, CancellationToken.None);

        // Try to vote again with a different option
        var secondVoteResult = await voteHandler.Handle(new VoteOnPollCommand
        {
            UserId = userId,
            PollId = pollResult.Id,
            Request = new CastVoteDto
            {
                OptionId = option2Id,
                Rating = 3
            }
        }, CancellationToken.None);

        // Assert
        secondVoteResult.Should().NotBeNull();
        secondVoteResult.Success.Should().BeFalse();
        secondVoteResult.Errors.Should().Contain("You have already voted on this poll");
    }

    [Fact]
    public async Task GetPolls_ShouldReturnUserPolls()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var otherUserId = Guid.NewGuid().ToString();
        
        // Create polls for user
        var createLogger = _serviceProvider.GetRequiredService<ILogger<CreatePollCommandHandler>>();
        var createHandler = new CreatePollCommandHandler(
            _unitOfWork.ValidationPolls,
            _unitOfWork.Outfits,
            _mapper,
            createLogger);

        await createHandler.Handle(new CreatePollCommand
        {
            UserId = userId,
            Request = new CreatePollDto
            {
                Question = "User poll 1",
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
                Options = new List<CreatePollOptionDto>
                {
                    new CreatePollOptionDto { Description = "Option 1", DisplayOrder = 1 },
                    new CreatePollOptionDto { Description = "Option 2", DisplayOrder = 2 }
                }
            }
        }, CancellationToken.None);

        await createHandler.Handle(new CreatePollCommand
        {
            UserId = userId,
            Request = new CreatePollDto
            {
                Question = "User poll 2",
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
                Options = new List<CreatePollOptionDto>
                {
                    new CreatePollOptionDto { Description = "Option A", DisplayOrder = 1 },
                    new CreatePollOptionDto { Description = "Option B", DisplayOrder = 2 }
                }
            }
        }, CancellationToken.None);

        // Create poll for other user
        await createHandler.Handle(new CreatePollCommand
        {
            UserId = otherUserId,
            Request = new CreatePollDto
            {
                Question = "Other user poll",
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
                Options = new List<CreatePollOptionDto>
                {
                    new CreatePollOptionDto { Description = "Option X", DisplayOrder = 1 },
                    new CreatePollOptionDto { Description = "Option Y", DisplayOrder = 2 }
                }
            }
        }, CancellationToken.None);

        // Act
        var getPollsHandler = new GetPollsRequestHandler(
            _unitOfWork.ValidationPolls,
            _mapper,
            _serviceProvider.GetRequiredService<ILogger<GetPollsRequestHandler>>());

        var result = await getPollsHandler.Handle(new GetPollsRequest { UserId = userId }, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.All(p => p.UserId == userId).Should().BeTrue();
    }

    [Fact]
    public async Task GetPollById_ShouldReturnWithVoteCounts()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        
        // Create poll with options
        var createLogger = _serviceProvider.GetRequiredService<ILogger<CreatePollCommandHandler>>();
        var createHandler = new CreatePollCommandHandler(
            _unitOfWork.ValidationPolls,
            _unitOfWork.Outfits,
            _mapper,
            createLogger);

        var pollResult = await createHandler.Handle(new CreatePollCommand
        {
            UserId = userId,
            Request = new CreatePollDto
            {
                Question = "Vote count test",
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
                Options = new List<CreatePollOptionDto>
                {
                    new CreatePollOptionDto { Description = "Option A", DisplayOrder = 1 },
                    new CreatePollOptionDto { Description = "Option B", DisplayOrder = 2 }
                }
            }
        }, CancellationToken.None);

        // Get poll and options
        var poll = await _unitOfWork.ValidationPolls.GetByIdAsync(pollResult.Id);
        var option1Id = poll!.Options.First().Id;
        var option2Id = poll.Options.Skip(1).First().Id;

        // Add votes
        var voter1Id = Guid.NewGuid().ToString();
        var voter2Id = Guid.NewGuid().ToString();
        
        var voteRepo = _unitOfWork.Votes;
        await voteRepo.AddAsync(new Vote
        {
            PollId = pollResult.Id,
            OptionId = option1Id,
            VoterId = voter1Id,
            Rating = 5,
            IsAnonymous = false
        });
        await voteRepo.AddAsync(new Vote
        {
            PollId = pollResult.Id,
            OptionId = option1Id,
            VoterId = voter2Id,
            Rating = 4,
            IsAnonymous = true
        });

        // Act
        var getPollHandler = new GetPollByIdRequestHandler(
            _unitOfWork.ValidationPolls,
            _mapper,
            _serviceProvider.GetRequiredService<ILogger<GetPollByIdRequestHandler>>());

        var result = await getPollHandler.Handle(new GetPollByIdRequest { Id = pollResult.Id, UserId = userId }, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Question.Should().Be("Vote count test");
        result.TotalVotes.Should().Be(2);
        result.Options.Should().HaveCount(2);
        
        var option1 = result.Options.First(o => o.Id == option1Id);
        option1.VoteCount.Should().Be(2);
        
        var option2 = result.Options.First(o => o.Id == option2Id);
        option2.VoteCount.Should().Be(0);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
        _serviceProvider.Dispose();
    }
}
