using AutoMapper;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.DTOs.Outfit;
using OutfitPlanner.Application.DTOs.Wardrobe;
using OutfitPlanner.Application.Features.Outfits.Handlers.Commands;
using OutfitPlanner.Application.Features.Outfits.Handlers.Queries;
using OutfitPlanner.Application.Features.Outfits.Requests.Commands;
using OutfitPlanner.Application.Features.Outfits.Requests.Queries;
using OutfitPlanner.Application.Features.ClothingItems.Handlers.Commands;
using OutfitPlanner.Application.Features.ClothingItems.Requests.Commands;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Application.Features.Outfits.Requests.Commands.Validators;
using FluentValidation;
using OutfitPlanner.Persistence;
using OutfitPlanner.Persistence.Repositories;
using OutfitPlanner.Application.Profiles;
using Xunit;

namespace OutfitPlanner.Application.IntegrationTests.Outfits;

public class OutfitTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly AppDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public OutfitTests()
    {
        var services = new ServiceCollection();

        // Use an in-memory database for testing
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase($"OutfitPlannerOutfitTestDb_{Guid.NewGuid()}"));

        // Add logging
        services.AddLogging(b => b.AddDebug());

        // Add AutoMapper with proper mappings
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
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
        services.AddValidatorsFromAssemblyContaining<CreateOutfitCommandValidator>();

        _serviceProvider = services.BuildServiceProvider();

        _dbContext = _serviceProvider.GetRequiredService<AppDbContext>();
        _unitOfWork = _serviceProvider.GetRequiredService<IUnitOfWork>();
        _mapper = _serviceProvider.GetRequiredService<IMapper>();
    }

    private async Task<Guid> CreateClothingItem(string userId, string name = "Test Item")
    {
        var logger = _serviceProvider.GetRequiredService<ILogger<CreateClothingItemCommandHandler>>();
        var handler = new CreateClothingItemCommandHandler(logger, _unitOfWork, _mapper);
        var command = new CreateClothingItemCommand
        {
            UserId = userId,
            Request = new CreateClothingItemDto
            {
                Name = name,
                Type = "Top",
                Category = "Casual",
                PrimaryColor = "Blue",
                Fabric = "Cotton"
            }
        };
        var result = await ((IRequestHandler<CreateClothingItemCommand, ClothingItemDto>)handler).Handle(command, CancellationToken.None);
        return result.Id;
    }

    [Fact]
    public async Task CreateOutfit_ShouldSucceed_WithValidData()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var itemId = await CreateClothingItem(userId);
        
        var logger = _serviceProvider.GetRequiredService<ILogger<CreateOutfitCommandHandler>>();
        var validator = _serviceProvider.GetRequiredService<IValidator<CreateOutfitCommand>>();
        var handler = new CreateOutfitCommandHandler(_unitOfWork, _mapper, validator, logger);

        var command = new CreateOutfitCommand
        {
            UserId = userId,
            Request = new CreateOutfitDto
            {
                Name = "Summer Casual",
                Occasion = "Casual",
                Season = "Summer",
                Items = new List<CreateOutfitItemDto>
                {
                    new CreateOutfitItemDto { ClothingItemId = itemId, Role = "Primary" }
                }
            }
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
      //  result..Should().BeTrue();
        result.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task GetOutfitById_ShouldReturnOutfit_WithItems()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var itemId = await CreateClothingItem(userId);
        
        var createLogger = _serviceProvider.GetRequiredService<ILogger<CreateOutfitCommandHandler>>();
        var createValidator = _serviceProvider.GetRequiredService<IValidator<CreateOutfitCommand>>();
        var createHandler = new CreateOutfitCommandHandler(_unitOfWork, _mapper, createValidator, createLogger);
        var createResult = await createHandler.Handle(new CreateOutfitCommand
        {
            UserId = userId,
            Request = new CreateOutfitDto
            {
                Name = "Test Outfit",
                Occasion = "Casual",
                Season = "Summer",
                Items = new List<CreateOutfitItemDto> { new CreateOutfitItemDto { ClothingItemId = itemId, Role = "Primary" } }
            }
        }, CancellationToken.None);

        var getHandler = new GetOutfitByIdRequestHandler(_unitOfWork.Outfits, _mapper, _serviceProvider.GetRequiredService<ILogger<GetOutfitByIdRequestHandler>>());

        // Act
        var result = await getHandler.Handle(new GetOutfitByIdRequest { Id = createResult.Id, UserId = userId }, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Test Outfit");
        result.Items.Should().HaveCount(1);
        result.Items[0].ClothingItemId.Should().Be(itemId);
    }

    [Fact]
    public async Task GetOutfitById_ShouldThrow_WhenNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var getHandler = new GetOutfitByIdRequestHandler(_unitOfWork.Outfits, _mapper, _serviceProvider.GetRequiredService<ILogger<GetOutfitByIdRequestHandler>>());

        // Act
        var act = async () => await getHandler.Handle(new GetOutfitByIdRequest { Id = Guid.NewGuid(), UserId = userId }, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<OutfitPlanner.Application.Exceptions.NotFoundException>();
    }

    [Fact]
    public async Task RecordOutfitWear_ShouldUpdateCounters()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var itemId = await CreateClothingItem(userId);
        
        var createLogger = _serviceProvider.GetRequiredService<ILogger<CreateOutfitCommandHandler>>();
        var createValidator = _serviceProvider.GetRequiredService<IValidator<CreateOutfitCommand>>();
        var createHandler = new CreateOutfitCommandHandler(_unitOfWork, _mapper, createValidator, createLogger);
        var createResult = await createHandler.Handle(new CreateOutfitCommand
        {
            UserId = userId,
            Request = new CreateOutfitDto
            {
                Name = "Wear Test",
                Occasion = "Casual",
                Season = "Summer",
                Items = new List<CreateOutfitItemDto> { new CreateOutfitItemDto { ClothingItemId = itemId, Role = "Primary" } }
            }
        }, CancellationToken.None);

        var wearHandler = new RecordOutfitWearCommandHandler(_unitOfWork, _mapper, _serviceProvider.GetRequiredService<ILogger<RecordOutfitWearCommandHandler>>());
        var wornAt = DateTimeOffset.UtcNow;

        // Act
        await wearHandler.Handle(new RecordOutfitWearCommand 
        { 
            OutfitId = createResult.Id, 
            UserId = userId, 
            WornAt = wornAt,
            WeatherCondition = "Sunny"
        }, CancellationToken.None);

        // Assert
        var outfit = await _unitOfWork.Outfits.GetByIdAsync(createResult.Id);
        outfit!.TimesWorn.Should().Be(1);
        outfit.LastWorn.Should().BeCloseTo(wornAt, TimeSpan.FromSeconds(1));

        var item = await _unitOfWork.ClothingItems.GetByIdAsync(itemId);
        item!.WearCount.Should().Be(1);
        item.LastWorn.Should().BeCloseTo(wornAt, TimeSpan.FromSeconds(1));
        
        // Verify events
        var events = await _unitOfWork.WearEvents.GetAllAsync();
        events.Count(e => e.OutfitId == createResult.Id).Should().BeGreaterThanOrEqualTo(1);
        events.Count(e => e.ClothingItemId == itemId).Should().Be(1);
    }

    [Fact]
    public async Task DeleteOutfit_ShouldSoftDelete()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var itemId = await CreateClothingItem(userId);
        
        var createLogger = _serviceProvider.GetRequiredService<ILogger<CreateOutfitCommandHandler>>();
        var createValidator = _serviceProvider.GetRequiredService<IValidator<CreateOutfitCommand>>();
        var createHandler = new CreateOutfitCommandHandler(_unitOfWork, _mapper, createValidator, createLogger);
        var createResult = await createHandler.Handle(new CreateOutfitCommand
        {
            UserId = userId,
            Request = new CreateOutfitDto
            {
                Name = "To Delete",
                Occasion = "Casual",
                Season = "Summer",
                Items = new List<CreateOutfitItemDto> { new CreateOutfitItemDto { ClothingItemId = itemId, Role = "Primary" } }
            }
        }, CancellationToken.None);

        var deleteHandler = new DeleteOutfitCommandHandler(_unitOfWork, _mapper, _serviceProvider.GetRequiredService<ILogger<DeleteOutfitCommandHandler>>());

        // Act
        await deleteHandler.Handle(new DeleteOutfitCommand { Id = createResult.Id, UserId = userId }, CancellationToken.None);

        // Assert
        var outfit = await _unitOfWork.Outfits.GetByIdAsync(createResult.Id);
        outfit.Should().BeNull();
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
        _serviceProvider.Dispose();
    }
}
