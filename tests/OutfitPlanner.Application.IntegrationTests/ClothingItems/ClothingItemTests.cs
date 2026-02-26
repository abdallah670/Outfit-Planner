using AutoMapper;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.DTOs.Wardrobe;
using OutfitPlanner.Application.Features.ClothingItems.Handlers.Commands;
using OutfitPlanner.Application.Features.ClothingItems.Handlers.Queries;
using OutfitPlanner.Application.Features.ClothingItems.Requests.Commands;
using OutfitPlanner.Application.Features.ClothingItems.Requests.Queries;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Domain.Enums;
using OutfitPlanner.Persistence;
using OutfitPlanner.Persistence.Repositories;
using OutfitPlanner.Application.Profiles;

namespace OutfitPlanner.Application.IntegrationTests.ClothingItems;

public class ClothingItemTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly AppDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ClothingItemTests()
    {
        var services = new ServiceCollection();

        // Use an in-memory database for testing
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase($"OutfitPlannerClothingTestDb_{Guid.NewGuid()}"));

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

        _serviceProvider = services.BuildServiceProvider();

        _dbContext = _serviceProvider.GetRequiredService<AppDbContext>();
        _unitOfWork = _serviceProvider.GetRequiredService<IUnitOfWork>();
        _mapper = _serviceProvider.GetRequiredService<IMapper>();
    }

    /// <summary>
    /// Creates a valid CreateClothingItemRequest with all required fields
    /// </summary>
    private static CreateClothingItemDto CreateValidRequest(string? name = null, string? type = null)
    {
        return new CreateClothingItemDto
        {
            Name = name ?? "Test Item",
            Type = type ?? "Top",
            Category = "Casual",
            PrimaryColor = "Blue",
            Fabric = "Cotton",
            Condition = "good",
            ImageUrl = "https://example.com/images/test-item.jpg",
            ThumbnailUrl = "https://example.com/thumbnails/test-item.jpg"
        };
    }

    [Fact]
    public async Task CreateClothingItem_ShouldCreateItem_WithValidData()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var logger = _serviceProvider.GetRequiredService<ILogger<CreateClothingItemCommandHandler>>();
        var handler = new CreateClothingItemCommandHandler(logger, _unitOfWork, _mapper);

        var command = new CreateClothingItemCommand
        {
            UserId = userId,
            Request = CreateValidRequest("Blue T-Shirt", "Top")
        };

        // Act - Cast to interface to use explicit implementation
        IRequestHandler<CreateClothingItemCommand, ClothingItemDto> handlerInterface = handler;
        var result = await handlerInterface.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBe(Guid.Empty, "a valid ID should be generated");
    }

    [Fact]
    public async Task CreateClothingItem_ShouldPersistItem_InDatabase()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var logger = _serviceProvider.GetRequiredService<ILogger<CreateClothingItemCommandHandler>>();
        var handler = new CreateClothingItemCommandHandler(logger, _unitOfWork, _mapper);

        var command = new CreateClothingItemCommand
        {
            UserId = userId,
            Request = CreateValidRequest("Red Dress", "Dress")
        };

        // Act
        IRequestHandler<CreateClothingItemCommand, ClothingItemDto> handlerInterface = handler;
        var result = await handlerInterface.Handle(command, CancellationToken.None);

        // Assert - Verify item was persisted
        var savedItem = await _unitOfWork.ClothingItems.GetByIdAsync(result.Id);
        savedItem.Should().NotBeNull();
        savedItem!.Name.Should().Be("Red Dress");
        savedItem.Type.Should().Be(ClothingType.Dress);
        savedItem.Category.Should().Be("Casual");
        savedItem.PrimaryColor.Should().Be("Blue");
        savedItem.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task GetClothingItemById_ShouldReturnItem_WhenItemExists()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        
        // First create an item
        var createLogger = _serviceProvider.GetRequiredService<ILogger<CreateClothingItemCommandHandler>>();
        var createHandler = new CreateClothingItemCommandHandler(createLogger, _unitOfWork, _mapper);
        
        var createCommand = new CreateClothingItemCommand
        {
            UserId = userId,
            Request = CreateValidRequest("Green Jacket", "Outerwear")
        };
        
        IRequestHandler<CreateClothingItemCommand, ClothingItemDto> createHandlerInterface = createHandler;
        var createResult = await createHandlerInterface.Handle(createCommand, CancellationToken.None);
        
        // Now get the item - constructor order: repository, logger, mapper
        var getLogger = _serviceProvider.GetRequiredService<ILogger<GetClothingItemByIdRequestHanlder>>();
        var getHandler = new GetClothingItemByIdRequestHanlder(_unitOfWork.ClothingItems, getLogger, _mapper);
        
        var getRequest = new GetClothingItemByIdRequest { Id = createResult.Id, UserId = userId };

        // Act
        IRequestHandler<GetClothingItemByIdRequest, ClothingItemDto> getHandlerInterface = getHandler;
        var result = await getHandlerInterface.Handle(getRequest, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Green Jacket");
        result.Type.Should().Be("Outerwear");
        result.Category.Should().Be("Casual");
        result.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task GetClothingItemById_ShouldThrowException_WhenItemDoesNotExist()
    {
        // Arrange - constructor order: repository, logger, mapper
        var getLogger = _serviceProvider.GetRequiredService<ILogger<GetClothingItemByIdRequestHanlder>>();
        var getHandler = new GetClothingItemByIdRequestHanlder(_unitOfWork.ClothingItems, getLogger, _mapper);
        
        var getRequest = new GetClothingItemByIdRequest { Id = Guid.NewGuid() };

        // Act & Assert
        // Note: The handler wraps NotFoundException in BadRequestException due to the catch-all exception handler
        IRequestHandler<GetClothingItemByIdRequest, ClothingItemDto> getHandlerInterface = getHandler;
        var act = async () => await getHandlerInterface.Handle(getRequest, CancellationToken.None);
        await act.Should().ThrowAsync<OutfitPlanner.Application.Exceptions.NotFoundException>();
    }

    [Fact]
    public async Task CreateAndGetClothingItem_FullWorkflow_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var createLogger = _serviceProvider.GetRequiredService<ILogger<CreateClothingItemCommandHandler>>();
        var createHandler = new CreateClothingItemCommandHandler(createLogger, _unitOfWork, _mapper);

        var command = new CreateClothingItemCommand
        {
            UserId = userId,
            Request = CreateValidRequest("Black Jeans", "Bottom")
        };

        // Act - Create
        IRequestHandler<CreateClothingItemCommand, ClothingItemDto> createHandlerInterface = createHandler;
        var createResult = await createHandlerInterface.Handle(command, CancellationToken.None);

        // Assert - Create
        createResult.Should().NotBeNull();
        createResult.Id.Should().NotBe(Guid.Empty);

        // Act - Get
        var getLogger = _serviceProvider.GetRequiredService<ILogger<GetClothingItemByIdRequestHanlder>>();
        var getHandler = new GetClothingItemByIdRequestHanlder(_unitOfWork.ClothingItems, getLogger, _mapper);
        IRequestHandler<GetClothingItemByIdRequest, ClothingItemDto> getHandlerInterface = getHandler;
         var getResult = await getHandlerInterface.Handle(new GetClothingItemByIdRequest { Id = createResult.Id, UserId = userId }, CancellationToken.None);

        // Assert - Get
        getResult.Should().NotBeNull();
        getResult.Name.Should().Be("Black Jeans");
        getResult.Type.Should().Be("Bottom");
        getResult.Category.Should().Be("Casual");
        getResult.UserId.Should().Be(userId);
        getResult.ImageUrl.Should().Be("https://example.com/images/test-item.jpg");
        getResult.ThumbnailUrl.Should().Be("https://example.com/thumbnails/test-item.jpg");
    }

    [Fact]
    public async Task CreateClothingItem_WithDifferentTypes_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var logger = _serviceProvider.GetRequiredService<ILogger<CreateClothingItemCommandHandler>>();
        var handler = new CreateClothingItemCommandHandler(logger, _unitOfWork, _mapper);

        var types = new[] { "Top", "Bottom", "Dress", "Outerwear", "Footwear", "Accessory" };

        IRequestHandler<CreateClothingItemCommand, ClothingItemDto> handlerInterface = handler;
        
        foreach (var type in types)
        {
            var command = new CreateClothingItemCommand
            {
                UserId = userId,
                Request = CreateValidRequest($"{type} Item", type)
            };

            // Act
            var result = await handlerInterface.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();

        }

        // Verify all items were created
        var items = await _unitOfWork.ClothingItems.GetByUserIdAsync(userId);
        items.Should().HaveCount(types.Length);
    }

    #region Delete Clothing Item Tests

    [Fact]
    public async Task DeleteClothingItem_ShouldSucceed_WhenItemExistsAndBelongsToUser()
    {
        // Arrange - Create a user and clothing item
        var userId = Guid.NewGuid().ToString();
        var user = new User { Id = userId, UserName = "testuser", Email = "test@example.com", Name = "Test User" };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Create a clothing item first
        var createLogger = _serviceProvider.GetRequiredService<ILogger<CreateClothingItemCommandHandler>>();
        var createHandler = new CreateClothingItemCommandHandler(createLogger, _unitOfWork, _mapper);
        var createCommand = new CreateClothingItemCommand
        {
            UserId = userId,
            Request = CreateValidRequest("Item to Delete", "Top")
        };
        IRequestHandler<CreateClothingItemCommand, ClothingItemDto> createHandlerInterface = createHandler;
        var createResult = await createHandlerInterface.Handle(createCommand, CancellationToken.None);
        createResult.Should().NotBeNull();


        // Act - Delete the item
        var deleteLogger = _serviceProvider.GetRequiredService<ILogger<DeleteClothingItemCommandHandler>>();
        var deleteHandler = new DeleteClothingItemCommandHandler(deleteLogger, _unitOfWork);
        var deleteCommand = new DeleteClothingItemCommand { Id = createResult.Id, UserId = userId };
        
        IRequestHandler<DeleteClothingItemCommand, BaseCommandResponse> deleteHandlerInterface = deleteHandler;
        var deleteResult = await deleteHandlerInterface.Handle(deleteCommand, CancellationToken.None);

        // Assert
        deleteResult.Success.Should().BeTrue();
        deleteResult.Message.Should().Be("Clothing item deleted successfully");
        deleteResult.Id.Should().Be(createResult.Id);

        // Verify item is actually deleted
        var deletedItem = await _unitOfWork.ClothingItems.GetByIdAsync(createResult.Id);
        deletedItem.Should().BeNull("item should be deleted from database");
    }

    [Fact]
    public async Task DeleteClothingItem_ShouldThrowNotFoundException_WhenItemDoesNotExist()
    {
        // Arrange - Create a user but no clothing item
        var userId = Guid.NewGuid().ToString();
        var user = new User { Id = userId, UserName = "testuser", Email = "test@example.com", Name = "Test User" };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var deleteLogger = _serviceProvider.GetRequiredService<ILogger<DeleteClothingItemCommandHandler>>();
        var deleteHandler = new DeleteClothingItemCommandHandler(deleteLogger, _unitOfWork);
        var deleteCommand = new DeleteClothingItemCommand { Id = Guid.NewGuid(), UserId = userId };

        // Act & Assert
        IRequestHandler<DeleteClothingItemCommand, BaseCommandResponse> deleteHandlerInterface = deleteHandler;
        var act = async () => await deleteHandlerInterface.Handle(deleteCommand, CancellationToken.None);
        await act.Should().ThrowAsync<OutfitPlanner.Application.Exceptions.NotFoundException>()
            .WithMessage("*Clothing item*");
    }

    [Fact]
    public async Task DeleteClothingItem_ShouldThrowNotFoundException_WhenUserDoesNotExist()
    {
        // Arrange - No user in database
        var deleteLogger = _serviceProvider.GetRequiredService<ILogger<DeleteClothingItemCommandHandler>>();
        var deleteHandler = new DeleteClothingItemCommandHandler(deleteLogger, _unitOfWork);
        var deleteCommand = new DeleteClothingItemCommand 
        { 
            Id = Guid.NewGuid(), 
            UserId = Guid.NewGuid().ToString() 
        };

        // Act & Assert
        IRequestHandler<DeleteClothingItemCommand, BaseCommandResponse> deleteHandlerInterface = deleteHandler;
        var act = async () => await deleteHandlerInterface.Handle(deleteCommand, CancellationToken.None);
        await act.Should().ThrowAsync<OutfitPlanner.Application.Exceptions.NotFoundException>()
            .WithMessage("*User*");
    }

    [Fact]
    public async Task DeleteClothingItem_ShouldThrowUnauthorizedAccessException_WhenItemBelongsToAnotherUser()
    {
        // Arrange - Create two users
        var userId1 = Guid.NewGuid().ToString();
        var userId2 = Guid.NewGuid().ToString();
        var user1 = new User { Id = userId1, UserName = "user1", Email = "user1@example.com", Name = "User One" };
        var user2 = new User { Id = userId2, UserName = "user2", Email = "user2@example.com", Name = "User Two" };
        _dbContext.Users.AddRange(user1, user2);
        await _dbContext.SaveChangesAsync();

        // Create item for user1
        var createLogger = _serviceProvider.GetRequiredService<ILogger<CreateClothingItemCommandHandler>>();
        var createHandler = new CreateClothingItemCommandHandler(createLogger, _unitOfWork, _mapper);
        var createCommand = new CreateClothingItemCommand
        {
            UserId = userId1,
            Request = CreateValidRequest("User1's Item", "Top")
        };
        IRequestHandler<CreateClothingItemCommand, ClothingItemDto> createHandlerInterface = createHandler;
        var createResult = await createHandlerInterface.Handle(createCommand, CancellationToken.None);

        // Act - Try to delete user1's item as user2
        var deleteLogger = _serviceProvider.GetRequiredService<ILogger<DeleteClothingItemCommandHandler>>();
        var deleteHandler = new DeleteClothingItemCommandHandler(deleteLogger, _unitOfWork);
        var deleteCommand = new DeleteClothingItemCommand { Id = createResult.Id, UserId = userId2 };

        IRequestHandler<DeleteClothingItemCommand, BaseCommandResponse> deleteHandlerInterface = deleteHandler;
        var act = async () => await deleteHandlerInterface.Handle(deleteCommand, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<OutfitPlanner.Application.Exceptions.UnauthorizedAccessException>()
            .WithMessage("*not authorized*");

        // Verify item still exists
        var item = await _unitOfWork.ClothingItems.GetByIdAsync(createResult.Id);
        item.Should().NotBeNull("item should not be deleted when unauthorized");
    }

    #endregion

    #region Update Clothing Item Tests

    [Fact]
    public async Task UpdateClothingItem_ShouldSucceed_WhenItemExistsAndBelongsToUser()
    {
        // Arrange - Create a user and clothing item
        var userId = Guid.NewGuid().ToString();
        var user = new User { Id = userId, UserName = "testuser", Email = "test@example.com", Name = "Test User" };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Create a clothing item first
        var createLogger = _serviceProvider.GetRequiredService<ILogger<CreateClothingItemCommandHandler>>();
        var createHandler = new CreateClothingItemCommandHandler(createLogger, _unitOfWork, _mapper);
        var createCommand = new CreateClothingItemCommand
        {
            UserId = userId,
            Request = CreateValidRequest("Original Item", "Top")
        };
        IRequestHandler<CreateClothingItemCommand, ClothingItemDto> createHandlerInterface = createHandler;
        var createResult = await createHandlerInterface.Handle(createCommand, CancellationToken.None);
        createResult.Should().NotBeNull();


        // Act - Update the item
        var updateLogger = _serviceProvider.GetRequiredService<ILogger<UpdateClothingItemCommandHandler>>();
        var updateHandler = new UpdateClothingItemCommandHandler(_unitOfWork, updateLogger, _mapper);
        var updateCommand = new UpdateClothingItemCommand
        {
            Id = createResult.Id,
            UserId = userId,
            Request = new UpdateClothingItemDto
            {
                Name = "Updated Item",
                Type = "Bottom",
                Category = "Formal",
                PrimaryColor = "Red",
                Fabric = "Silk",
                Condition = "excellent",
                ImageUrl = "https://example.com/updated.jpg"
            }
        };

        var result = await updateHandler.Handle(updateCommand, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Updated Item");
        result.Type.Should().Be("Bottom");
        result.Category.Should().Be("Formal");
        result.PrimaryColor.Should().Be("Red");

        // Verify item was updated in database
        var updatedItem = await _unitOfWork.ClothingItems.GetByIdAsync(createResult.Id);
        updatedItem.Should().NotBeNull();
        updatedItem!.Name.Should().Be("Updated Item");
        updatedItem.Type.Should().Be(Domain.Enums.ClothingType.Bottom);
        updatedItem.Category.Should().Be("Formal");
        updatedItem.PrimaryColor.Should().Be("Red");
    }

    [Fact]
    public async Task UpdateClothingItem_ShouldThrowNotFoundException_WhenItemDoesNotExist()
    {
        // Arrange - Create a user but no clothing item
        var userId = Guid.NewGuid().ToString();
        var user = new User { Id = userId, UserName = "testuser", Email = "test@example.com", Name = "Test User" };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var updateLogger = _serviceProvider.GetRequiredService<ILogger<UpdateClothingItemCommandHandler>>();
        var updateHandler = new UpdateClothingItemCommandHandler(_unitOfWork, updateLogger, _mapper);
        var updateCommand = new UpdateClothingItemCommand
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Request = new UpdateClothingItemDto
            {
                Name = "Updated Item",
                Type = "Top",
                Category = "Casual",
                PrimaryColor = "Blue",
                Fabric = "Cotton",
                Condition = "good"
            }
        };

        // Act & Assert
        var act = async () => await updateHandler.Handle(updateCommand, CancellationToken.None);
        await act.Should().ThrowAsync<OutfitPlanner.Application.Exceptions.NotFoundException>()
            .WithMessage("*Clothing item*");
    }

    [Fact]
    public async Task UpdateClothingItem_ShouldThrowUnauthorizedAccessException_WhenItemBelongsToAnotherUser()
    {
        // Arrange - Create two users
        var userId1 = Guid.NewGuid().ToString();
        var userId2 = Guid.NewGuid().ToString();
        var user1 = new User { Id = userId1, UserName = "user1", Email = "user1@example.com", Name = "User One" };
        var user2 = new User { Id = userId2, UserName = "user2", Email = "user2@example.com", Name = "User Two" };
        _dbContext.Users.AddRange(user1, user2);
        await _dbContext.SaveChangesAsync();

        // Create item for user1
        var createLogger = _serviceProvider.GetRequiredService<ILogger<CreateClothingItemCommandHandler>>();
        var createHandler = new CreateClothingItemCommandHandler(createLogger, _unitOfWork, _mapper);
        var createCommand = new CreateClothingItemCommand
        {
            UserId = userId1,
            Request = CreateValidRequest("User1's Item", "Top")
        };
        IRequestHandler<CreateClothingItemCommand, ClothingItemDto> createHandlerInterface = createHandler;
        var createResult = await createHandlerInterface.Handle(createCommand, CancellationToken.None);

        // Act - Try to update user1's item as user2
        var updateLogger = _serviceProvider.GetRequiredService<ILogger<UpdateClothingItemCommandHandler>>();
        var updateHandler = new UpdateClothingItemCommandHandler(_unitOfWork, updateLogger, _mapper);
        var updateCommand = new UpdateClothingItemCommand
        {
            Id = createResult.Id,
            UserId = userId2,
            Request = new UpdateClothingItemDto
            {
                Name = "Hacked Item",
                Type = "Top",
                Category = "Casual",
                PrimaryColor = "Black",
                Fabric = "Cotton",
                Condition = "good"
            }
        };

        var act = async () => await updateHandler.Handle(updateCommand, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<OutfitPlanner.Application.Exceptions.UnauthorizedAccessException>()
            .WithMessage("*not authorized*");

        // Verify item was not updated
        var item = await _unitOfWork.ClothingItems.GetByIdAsync(createResult.Id);
        item.Should().NotBeNull();
        item!.Name.Should().Be("User1's Item", "item name should not be changed by unauthorized user");
    }

    [Fact]
    public async Task UpdateClothingItem_ShouldThrowValidationException_WhenRequestIsInvalid()
    {
        // Arrange - Create a user and clothing item
        var userId = Guid.NewGuid().ToString();
        var user = new User { Id = userId, UserName = "testuser", Email = "test@example.com", Name = "Test User" };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Create a clothing item first
        var createLogger = _serviceProvider.GetRequiredService<ILogger<CreateClothingItemCommandHandler>>();
        var createHandler = new CreateClothingItemCommandHandler(createLogger, _unitOfWork, _mapper);
        var createCommand = new CreateClothingItemCommand
        {
            UserId = userId,
            Request = CreateValidRequest("Original Item", "Top")
        };
        IRequestHandler<CreateClothingItemCommand, ClothingItemDto> createHandlerInterface = createHandler;
        var createResult = await createHandlerInterface.Handle(createCommand, CancellationToken.None);

        // Act - Try to update with invalid data (empty name)
        var updateLogger = _serviceProvider.GetRequiredService<ILogger<UpdateClothingItemCommandHandler>>();
        var updateHandler = new UpdateClothingItemCommandHandler(_unitOfWork, updateLogger, _mapper);
        var updateCommand = new UpdateClothingItemCommand
        {
            Id = createResult.Id,
            UserId = userId,
            Request = new UpdateClothingItemDto
            {
                Name = "", // Invalid - empty name
                Type = "Top",
                Category = "Casual",
                PrimaryColor = "Blue",
                Fabric = "Cotton",
                Condition = "good"
            }
        };

        var act = async () => await updateHandler.Handle(updateCommand, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<OutfitPlanner.Application.Exceptions.ValidationException>();
    }

    #endregion

    #region Get Clothing Item List Tests

    [Fact]
    public async Task GetClothingItemList_ShouldReturnItems_WhenUserHasItems()
    {
        // Arrange - Create a user and multiple clothing items
        var userId = Guid.NewGuid().ToString();
        var user = new User { Id = userId, UserName = "testuser", Email = "test@example.com", Name = "Test User" };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Create multiple clothing items
        var createLogger = _serviceProvider.GetRequiredService<ILogger<CreateClothingItemCommandHandler>>();
        var createHandler = new CreateClothingItemCommandHandler(createLogger, _unitOfWork, _mapper);
        IRequestHandler<CreateClothingItemCommand, ClothingItemDto> createHandlerInterface = createHandler;

        var items = new[]
        {
            ("Blue T-Shirt", "Top"),
            ("Black Jeans", "Bottom"),
            ("Summer Dress", "Dress"),
            ("Winter Jacket", "Outerwear")
        };

        foreach (var (name, type) in items)
        {
            var command = new CreateClothingItemCommand
            {
                UserId = userId,
                Request = CreateValidRequest(name, type)
            };
            await createHandlerInterface.Handle(command, CancellationToken.None);
        }

        // Act - Get the list of items
        var getListLogger = _serviceProvider.GetRequiredService<ILogger<GetClothingItemListRequestHandler>>();
        var getListHandler = new GetClothingItemListRequestHandler(getListLogger, _unitOfWork);
        var getRequest = new GetClothingItemListRequest { UserId = userId };

        IRequestHandler<GetClothingItemListRequest, List<ClothingItemListDto>> getListHandlerInterface = getListHandler;
        var result = await getListHandlerInterface.Handle(getRequest, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(4, "user has 4 clothing items");
        result.Select(i => i.Name).Should().Contain("Blue T-Shirt", "Black Jeans", "Summer Dress", "Winter Jacket");
        result.Select(i => i.Type).Should().Contain("Top", "Bottom", "Dress", "Outerwear");
    }

    [Fact]
    public async Task GetClothingItemList_ShouldReturnEmptyList_WhenUserHasNoItems()
    {
        // Arrange - Create a user without clothing items
        var userId = Guid.NewGuid().ToString();
        var user = new User { Id = userId, UserName = "testuser", Email = "test@example.com", Name = "Test User" };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var getListLogger = _serviceProvider.GetRequiredService<ILogger<GetClothingItemListRequestHandler>>();
        var getListHandler = new GetClothingItemListRequestHandler(getListLogger, _unitOfWork);
        var getRequest = new GetClothingItemListRequest { UserId = userId };

        // Act
        IRequestHandler<GetClothingItemListRequest, List<ClothingItemListDto>> getListHandlerInterface = getListHandler;
        var result = await getListHandlerInterface.Handle(getRequest, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty("user has no clothing items");
    }

    [Fact]
    public async Task GetClothingItemList_ShouldOnlyReturnItemsForSpecificUser()
    {
        // Arrange - Create two users with items
        var userId1 = Guid.NewGuid().ToString();
        var userId2 = Guid.NewGuid().ToString();
        var user1 = new User { Id = userId1, UserName = "user1", Email = "user1@example.com", Name = "User One" };
        var user2 = new User { Id = userId2, UserName = "user2", Email = "user2@example.com", Name = "User Two" };
        _dbContext.Users.AddRange(user1, user2);
        await _dbContext.SaveChangesAsync();

        var createLogger = _serviceProvider.GetRequiredService<ILogger<CreateClothingItemCommandHandler>>();
        var createHandler = new CreateClothingItemCommandHandler(createLogger, _unitOfWork, _mapper);
        IRequestHandler<CreateClothingItemCommand, ClothingItemDto> createHandlerInterface = createHandler;

        // Create items for user1
        var command1 = new CreateClothingItemCommand
        {
            UserId = userId1,
            Request = CreateValidRequest("User1 Item", "Top")
        };
        await createHandlerInterface.Handle(command1, CancellationToken.None);

        // Create items for user2
        var command2 = new CreateClothingItemCommand
        {
            UserId = userId2,
            Request = CreateValidRequest("User2 Item", "Bottom")
        };
        await createHandlerInterface.Handle(command2, CancellationToken.None);

        // Act - Get items for user1
        var getListLogger = _serviceProvider.GetRequiredService<ILogger<GetClothingItemListRequestHandler>>();
        var getListHandler = new GetClothingItemListRequestHandler(getListLogger, _unitOfWork);
        var getRequest = new GetClothingItemListRequest { UserId = userId1 };

        IRequestHandler<GetClothingItemListRequest, List<ClothingItemListDto>> getListHandlerInterface = getListHandler;
        var result = await getListHandlerInterface.Handle(getRequest, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1, "only user1's items should be returned");
        result[0].Name.Should().Be("User1 Item");
    }

    #endregion

    #region Get Clothing Items By Category Tests

    [Fact]
    public async Task GetClothingItemsByCategory_ShouldReturnItems_WhenCategoryMatches()
    {
        // Arrange - Create a user with items in different categories
        var userId = Guid.NewGuid().ToString();
        var user = new User { Id = userId, UserName = "testuser", Email = "test@example.com", Name = "Test User" };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var createLogger = _serviceProvider.GetRequiredService<ILogger<CreateClothingItemCommandHandler>>();
        var createHandler = new CreateClothingItemCommandHandler(createLogger, _unitOfWork, _mapper);
        IRequestHandler<CreateClothingItemCommand, ClothingItemDto> createHandlerInterface = createHandler;

        // Create items with different categories
        var casualCommand = new CreateClothingItemCommand
        {
            UserId = userId,
            Request = new CreateClothingItemDto
            {
                Name = "Casual T-Shirt",
                Type = "Top",
                Category = "Casual",
                PrimaryColor = "Blue",
                Fabric = "Cotton",
                Condition = "good",
                ImageUrl = "https://example.com/casual.jpg"
            }
        };
        await createHandlerInterface.Handle(casualCommand, CancellationToken.None);

        var formalCommand = new CreateClothingItemCommand
        {
            UserId = userId,
            Request = new CreateClothingItemDto
            {
                Name = "Formal Shirt",
                Type = "Top",
                Category = "Formal",
                PrimaryColor = "White",
                Fabric = "Cotton",
                Condition = "excellent",
                ImageUrl = "https://example.com/formal.jpg"
            }
        };
        await createHandlerInterface.Handle(formalCommand, CancellationToken.None);

        var sportCommand = new CreateClothingItemCommand
        {
            UserId = userId,
            Request = new CreateClothingItemDto
            {
                Name = "Sport Jersey",
                Type = "Top",
                Category = "Sport",
                PrimaryColor = "Red",
                Fabric = "Polyester",
                Condition = "good",
                ImageUrl = "https://example.com/sport.jpg"
            }
        };
        await createHandlerInterface.Handle(sportCommand, CancellationToken.None);

        // Act - Get items by category "Formal"
        var getByCategoryLogger = _serviceProvider.GetRequiredService<ILogger<GetClothingItemsByCategoryRequestHandler>>();
        var getByCategoryHandler = new GetClothingItemsByCategoryRequestHandler(getByCategoryLogger, _unitOfWork);
        var getRequest = new GetClothingItemsByCategoryRequest { UserId = userId, Category = "Formal" };

        IRequestHandler<GetClothingItemsByCategoryRequest, List<ClothingItemListDto>> getByCategoryHandlerInterface = getByCategoryHandler;
        var result = await getByCategoryHandlerInterface.Handle(getRequest, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1, "only one item in Formal category");
        result[0].Name.Should().Be("Formal Shirt");
        result[0].Category.Should().Be("Formal");
    }

    [Fact]
    public async Task GetClothingItemsByCategory_ShouldReturnEmptyList_WhenNoItemsInCategory()
    {
        // Arrange - Create a user with items but not in the requested category
        var userId = Guid.NewGuid().ToString();
        var user = new User { Id = userId, UserName = "testuser", Email = "test@example.com", Name = "Test User" };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var createLogger = _serviceProvider.GetRequiredService<ILogger<CreateClothingItemCommandHandler>>();
        var createHandler = new CreateClothingItemCommandHandler(createLogger, _unitOfWork, _mapper);
        IRequestHandler<CreateClothingItemCommand, ClothingItemDto> createHandlerInterface = createHandler;

        var command = new CreateClothingItemCommand
        {
            UserId = userId,
            Request = CreateValidRequest("Casual Item", "Top") // Uses "Casual" category
        };
        await createHandlerInterface.Handle(command, CancellationToken.None);

        // Act - Get items by category "Formal" (which doesn't exist)
        var getByCategoryLogger = _serviceProvider.GetRequiredService<ILogger<GetClothingItemsByCategoryRequestHandler>>();
        var getByCategoryHandler = new GetClothingItemsByCategoryRequestHandler(getByCategoryLogger, _unitOfWork);
        var getRequest = new GetClothingItemsByCategoryRequest { UserId = userId, Category = "Formal" };

        IRequestHandler<GetClothingItemsByCategoryRequest, List<ClothingItemListDto>> getByCategoryHandlerInterface = getByCategoryHandler;
        var result = await getByCategoryHandlerInterface.Handle(getRequest, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty("no items in Formal category");
    }

    [Fact]
    public async Task GetClothingItemsByCategory_ShouldReturnMultipleItems_WhenMultipleItemsInCategory()
    {
        // Arrange - Create a user with multiple items in the same category
        var userId = Guid.NewGuid().ToString();
        var user = new User { Id = userId, UserName = "testuser", Email = "test@example.com", Name = "Test User" };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var createLogger = _serviceProvider.GetRequiredService<ILogger<CreateClothingItemCommandHandler>>();
        var createHandler = new CreateClothingItemCommandHandler(createLogger, _unitOfWork, _mapper);
        IRequestHandler<CreateClothingItemCommand, ClothingItemDto> createHandlerInterface = createHandler;

        // Create multiple casual items
        var casualItems = new[] { "Casual T-Shirt", "Casual Jeans", "Casual Sneakers" };
        foreach (var itemName in casualItems)
        {
            var command = new CreateClothingItemCommand
            {
                UserId = userId,
                Request = new CreateClothingItemDto
                {
                    Name = itemName,
                    Type = "Top",
                    Category = "Casual",
                    PrimaryColor = "Blue",
                    Fabric = "Cotton",
                    Condition = "good",
                    ImageUrl = $"https://example.com/{itemName}.jpg"
                }
            };
            await createHandlerInterface.Handle(command, CancellationToken.None);
        }

        // Act
        var getByCategoryLogger = _serviceProvider.GetRequiredService<ILogger<GetClothingItemsByCategoryRequestHandler>>();
        var getByCategoryHandler = new GetClothingItemsByCategoryRequestHandler(getByCategoryLogger, _unitOfWork);
        var getRequest = new GetClothingItemsByCategoryRequest { UserId = userId, Category = "Casual" };

        IRequestHandler<GetClothingItemsByCategoryRequest, List<ClothingItemListDto>> getByCategoryHandlerInterface = getByCategoryHandler;
        var result = await getByCategoryHandlerInterface.Handle(getRequest, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3, "three items in Casual category");
        result.Select(i => i.Name).Should().Contain("Casual T-Shirt", "Casual Jeans", "Casual Sneakers");
        result.All(i => i.Category == "Casual").Should().BeTrue("all items should be in Casual category");
    }

    [Fact]
    public async Task GetClothingItemsByCategory_ShouldOnlyReturnItemsForSpecificUser()
    {
        // Arrange - Create two users with items in the same category
        var userId1 = Guid.NewGuid().ToString();
        var userId2 = Guid.NewGuid().ToString();
        var user1 = new User { Id = userId1, UserName = "user1", Email = "user1@example.com", Name = "User One" };
        var user2 = new User { Id = userId2, UserName = "user2", Email = "user2@example.com", Name = "User Two" };
        _dbContext.Users.AddRange(user1, user2);
        await _dbContext.SaveChangesAsync();

        var createLogger = _serviceProvider.GetRequiredService<ILogger<CreateClothingItemCommandHandler>>();
        var createHandler = new CreateClothingItemCommandHandler(createLogger, _unitOfWork, _mapper);
        IRequestHandler<CreateClothingItemCommand, ClothingItemDto> createHandlerInterface = createHandler;

        // Create casual item for user1
        var command1 = new CreateClothingItemCommand
        {
            UserId = userId1,
            Request = new CreateClothingItemDto
            {
                Name = "User1 Casual Item",
                Type = "Top",
                Category = "Casual",
                PrimaryColor = "Blue",
                Fabric = "Cotton",
                Condition = "good",
                ImageUrl = "https://example.com/user1.jpg"
            }
        };
        await createHandlerInterface.Handle(command1, CancellationToken.None);

        // Create casual item for user2
        var command2 = new CreateClothingItemCommand
        {
            UserId = userId2,
            Request = new CreateClothingItemDto
            {
                Name = "User2 Casual Item",
                Type = "Top",
                Category = "Casual",
                PrimaryColor = "Red",
                Fabric = "Cotton",
                Condition = "good",
                ImageUrl = "https://example.com/user2.jpg"
            }
        };
        await createHandlerInterface.Handle(command2, CancellationToken.None);

        // Act - Get casual items for user1
        var getByCategoryLogger = _serviceProvider.GetRequiredService<ILogger<GetClothingItemsByCategoryRequestHandler>>();
        var getByCategoryHandler = new GetClothingItemsByCategoryRequestHandler(getByCategoryLogger, _unitOfWork);
        var getRequest = new GetClothingItemsByCategoryRequest { UserId = userId1, Category = "Casual" };

        IRequestHandler<GetClothingItemsByCategoryRequest, List<ClothingItemListDto>> getByCategoryHandlerInterface = getByCategoryHandler;
        var result = await getByCategoryHandlerInterface.Handle(getRequest, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1, "only user1's items should be returned");
        result[0].Name.Should().Be("User1 Casual Item");
    }

    #endregion

    #region Record Wear Tests

    [Fact]
    public async Task RecordWear_ShouldSucceed_WhenItemExistsAndBelongsToUser()
    {
        // Arrange - Create a user and clothing item
        var userId = Guid.NewGuid().ToString();
        var user = new User { Id = userId, UserName = "testuser", Email = "test@example.com", Name = "Test User" };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Create a clothing item first
        var createLogger = _serviceProvider.GetRequiredService<ILogger<CreateClothingItemCommandHandler>>();
        var createHandler = new CreateClothingItemCommandHandler(createLogger, _unitOfWork, _mapper);
        var createCommand = new CreateClothingItemCommand
        {
            UserId = userId,
            Request = new CreateClothingItemDto
            {
                Name = "Item to Wear",
                Type = "Top",
                Category = "Casual",
                PrimaryColor = "Blue",
                Fabric = "Cotton",
                Condition = "good",
                ImageUrl = "https://example.com/test.jpg"
            }
        };
        IRequestHandler<CreateClothingItemCommand, ClothingItemDto> createHandlerInterface = createHandler;
        var createResult = await createHandlerInterface.Handle(createCommand, CancellationToken.None);
        createResult.Should().NotBeNull();


        // Act - Record wear
        var recordWearLogger = _serviceProvider.GetRequiredService<ILogger<RecordWearCommandHandler>>();
        var recordWearHandler = new RecordWearCommandHandler(_unitOfWork, recordWearLogger, _mapper);
        var recordWearCommand = new RecordWearCommand
        {
            UserId = userId,
            Request = new RecordWearDto
            {
                ClothingItemId = createResult.Id,
                WornAt = DateTimeOffset.UtcNow.AddDays(-1), // Yesterday
                DurationMinutes = 120,
                WeatherCondition = "Sunny",
                Rating = 5,
                Notes = "Great outfit for the day"
            }
        };

        var result = await recordWearHandler.Handle(recordWearCommand, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(createResult.Id);

        // Verify wear event was created
        var wearEvents = await _unitOfWork.WearEvents.GetAllAsync();
        wearEvents.Should().HaveCount(1);
        var wearEvent = wearEvents.First();
        wearEvent.UserId.Should().Be(userId);
        wearEvent.ClothingItemId.Should().Be(createResult.Id);
        wearEvent.DurationMinutes.Should().Be(120);
        wearEvent.WeatherCondition.Should().Be("Sunny");
        wearEvent.Rating.Should().Be(5);
        wearEvent.Notes.Should().Be("Great outfit for the day");

        // Verify clothing item was updated
        var updatedItem = await _unitOfWork.ClothingItems.GetByIdAsync(createResult.Id);
        updatedItem.Should().NotBeNull();
        updatedItem!.WearCount.Should().Be(1);
        updatedItem!.LastWorn.Should().BeCloseTo(DateTimeOffset.UtcNow.AddDays(-1), TimeSpan.FromHours(2));
    }

    [Fact]
    public async Task RecordWear_ShouldThrowNotFoundException_WhenItemDoesNotExist()
    {
        // Arrange - Create a user but no clothing item
        var userId = Guid.NewGuid().ToString();
        var user = new User { Id = userId, UserName = "testuser", Email = "test@example.com", Name = "Test User" };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var recordWearLogger = _serviceProvider.GetRequiredService<ILogger<RecordWearCommandHandler>>();
        var recordWearHandler = new RecordWearCommandHandler(_unitOfWork, recordWearLogger, _mapper);
        var recordWearCommand = new RecordWearCommand
        {
            UserId = userId,
            Request = new RecordWearDto
            {
                ClothingItemId = Guid.NewGuid(),
                WornAt = DateTimeOffset.UtcNow
            }
        };

        // Act & Assert
        var act = async () => await recordWearHandler.Handle(recordWearCommand, CancellationToken.None);
        await act.Should().ThrowAsync<OutfitPlanner.Application.Exceptions.NotFoundException>()
            .WithMessage("*Clothing item*");
    }

    [Fact]
    public async Task RecordWear_ShouldThrowUnauthorizedAccessException_WhenItemBelongsToAnotherUser()
    {
        // Arrange - Create two users
        var userId1 = Guid.NewGuid().ToString();
        var userId2 = Guid.NewGuid().ToString();
        var user1 = new User { Id = userId1, UserName = "user1", Email = "user1@example.com", Name = "User One" };
        var user2 = new User { Id = userId2, UserName = "user2", Email = "user2@example.com", Name = "User Two" };
        _dbContext.Users.AddRange(user1, user2);
        await _dbContext.SaveChangesAsync();

        // Create item for user1
        var createLogger = _serviceProvider.GetRequiredService<ILogger<CreateClothingItemCommandHandler>>();
        var createHandler = new CreateClothingItemCommandHandler(createLogger, _unitOfWork, _mapper);
        var createCommand = new CreateClothingItemCommand
        {
            UserId = userId1,
            Request = new CreateClothingItemDto
            {
                Name = "User1's Item",
                Type = "Top",
                Category = "Casual",
                PrimaryColor = "Blue",
                Fabric = "Cotton",
                Condition = "good",
                ImageUrl = "https://example.com/test.jpg"
            }
        };
        IRequestHandler<CreateClothingItemCommand, ClothingItemDto> createHandlerInterface = createHandler;
        var createResult = await createHandlerInterface.Handle(createCommand, CancellationToken.None);

        // Act - Try to record wear for user1's item as user2
        var recordWearLogger = _serviceProvider.GetRequiredService<ILogger<RecordWearCommandHandler>>();
        var recordWearHandler = new RecordWearCommandHandler(_unitOfWork, recordWearLogger, _mapper);
        var recordWearCommand = new RecordWearCommand
        {
            UserId = userId2,
            Request = new RecordWearDto
            {
                ClothingItemId = createResult.Id,
                WornAt = DateTimeOffset.UtcNow
            }
        };

        var act = async () => await recordWearHandler.Handle(recordWearCommand, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<OutfitPlanner.Application.Exceptions.UnauthorizedAccessException>()
            .WithMessage("*not authorized*");

        // Verify no wear event was created
        var wearEvents = await _unitOfWork.WearEvents.GetAllAsync();
        wearEvents.Should().BeEmpty();

        // Verify item wear count was not updated
        var item = await _unitOfWork.ClothingItems.GetByIdAsync(createResult.Id);
        item.Should().NotBeNull();
        item!.WearCount.Should().Be(0);
    }

    [Fact]
    public async Task RecordWear_ShouldThrowValidationException_WhenRequestIsInvalid()
    {
        // Arrange - Create a user and clothing item
        var userId = Guid.NewGuid().ToString();
        var user = new User { Id = userId, UserName = "testuser", Email = "test@example.com", Name = "Test User" };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Create a clothing item
        var createLogger = _serviceProvider.GetRequiredService<ILogger<CreateClothingItemCommandHandler>>();
        var createHandler = new CreateClothingItemCommandHandler(createLogger, _unitOfWork, _mapper);
        var createCommand = new CreateClothingItemCommand
        {
            UserId = userId,
            Request = new CreateClothingItemDto
            {
                Name = "Test Item",
                Type = "Top",
                Category = "Casual",
                PrimaryColor = "Blue",
                Fabric = "Cotton",
                Condition = "good",
                ImageUrl = "https://example.com/test.jpg"
            }
        };
        IRequestHandler<CreateClothingItemCommand, ClothingItemDto> createHandlerInterface = createHandler;
        var createResult = await createHandlerInterface.Handle(createCommand, CancellationToken.None);

        // Act - Try to record wear with future date (invalid)
        var recordWearLogger = _serviceProvider.GetRequiredService<ILogger<RecordWearCommandHandler>>();
        var recordWearHandler = new RecordWearCommandHandler(_unitOfWork, recordWearLogger, _mapper);
        var recordWearCommand = new RecordWearCommand
        {
            UserId = userId,
            Request = new RecordWearDto
            {
                ClothingItemId = createResult.Id,
                WornAt = DateTimeOffset.UtcNow.AddDays(1) // Future date - invalid
            }
        };

        var act = async () => await recordWearHandler.Handle(recordWearCommand, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<OutfitPlanner.Application.Exceptions.ValidationException>();
    }

    #endregion

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
        _serviceProvider.Dispose();
    }
}
