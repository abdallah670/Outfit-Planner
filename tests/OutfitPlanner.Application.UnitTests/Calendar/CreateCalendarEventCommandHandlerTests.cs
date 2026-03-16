using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.DTOs.Calendar;
using OutfitPlanner.Application.Exceptions;
using OutfitPlanner.Application.Features.Calendar.Handlers.Commands;
using OutfitPlanner.Application.Features.Calendar.Requests.Commands;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Domain.Entities;
using Xunit;

// Use DTO CalendarEventType for requests
using DtoCalendarEventType = OutfitPlanner.Application.DTOs.Calendar.CalendarEventType;

namespace OutfitPlanner.Application.UnitTests.Calendar;

public class CreateCalendarEventCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<CreateCalendarEventCommandHandler>> _loggerMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly CreateCalendarEventCommandHandler _handler;

    public CreateCalendarEventCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<CreateCalendarEventCommandHandler>>();
        _mapperMock = new Mock<IMapper>();
        _handler = new CreateCalendarEventCommandHandler(
            _loggerMock.Object,
            _unitOfWorkMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequestWithoutOutfit_ShouldCreateCalendarEvent()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var request = new CreateCalendarEventCommand
        {
            UserId = userId,
            Request = new CreateCalendarEventRequest
            {
                Title = "Team Meeting",
                Description = "Weekly team sync",
                Location = "Conference Room A",
                EventDate = DateTimeOffset.Now.AddDays(1),
                StartTime = TimeSpan.FromHours(10),
                EndTime = TimeSpan.FromHours(11),
                EventType = DtoCalendarEventType.Work,
                OutfitId = null,
                Notes = "Bring laptop"
            }
        };

        _unitOfWorkMock.Setup(u => u.CalendarEvents.AddAsync(It.IsAny<Domain.Entities.CalendarEvent>()))
            .Callback<Domain.Entities.CalendarEvent>(e => { })
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Calendar event created successfully");
    }

    [Fact]
    public async Task Handle_ValidRequestWithOutfit_ShouldCreateCalendarEventAndWearEvent()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var outfitId = Guid.NewGuid();
        var outfit = new Outfit 
        { 
            Id = outfitId, 
            UserId = userId, 
            Name = "Business Casual",
            User = null!, 
            Items = new List<OutfitItem>() 
        };

        var request = new CreateCalendarEventCommand
        {
            UserId = userId,
            Request = new CreateCalendarEventRequest
            {
                Title = "Client Meeting",
                Description = "Important client presentation",
                Location = "Main Office",
                EventDate = DateTimeOffset.Now.AddDays(2),
                StartTime = TimeSpan.FromHours(14),
                EndTime = TimeSpan.FromHours(15),
                EventType = DtoCalendarEventType.Meeting,
                OutfitId = outfitId,
                Notes = "Wear formal attire"
            }
        };

        _unitOfWorkMock.Setup(u => u.Outfits.GetByIdAsync(outfitId))
            .ReturnsAsync(outfit);
        _unitOfWorkMock.Setup(u => u.WearEvents.AddAsync(It.IsAny<WearEvent>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.CalendarEvents.AddAsync(It.IsAny<Domain.Entities.CalendarEvent>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Calendar event created successfully");
    }

    [Fact]
    public async Task Handle_InvalidOutfit_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var outfitId = Guid.NewGuid();
        var request = new CreateCalendarEventCommand
        {
            UserId = userId,
            Request = new CreateCalendarEventRequest
            {
                Title = "Meeting",
                EventDate = DateTimeOffset.Now.AddDays(1),
                EventType = DtoCalendarEventType.Meeting,
                OutfitId = outfitId
            }
        };

        _unitOfWorkMock.Setup(u => u.Outfits.GetByIdAsync(outfitId))
            .ReturnsAsync((Outfit?)null);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Outfit not found");
    }

    [Fact]
    public async Task Handle_OutfitBelongsToDifferentUser_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var differentUserId = Guid.NewGuid().ToString();
        var outfitId = Guid.NewGuid();
        var outfit = new Outfit 
        { 
            Id = outfitId, 
            UserId = differentUserId,
            User = null!,
            Items = new List<OutfitItem>()
        };

        var request = new CreateCalendarEventCommand
        {
            UserId = userId,
            Request = new CreateCalendarEventRequest
            {
                Title = "Meeting",
                EventDate = DateTimeOffset.Now.AddDays(1),
                EventType = DtoCalendarEventType.Meeting,
                OutfitId = outfitId
            }
        };

        _unitOfWorkMock.Setup(u => u.Outfits.GetByIdAsync(outfitId))
            .ReturnsAsync(outfit);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Outfit not found");
    }

    [Fact]
    public async Task Handle_InvalidRequest_ShouldThrowValidationException()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var request = new CreateCalendarEventCommand
        {
            UserId = userId,
            Request = new CreateCalendarEventRequest
            {
                Title = "", // Empty title - invalid
                EventDate = DateTimeOffset.MinValue, // Invalid date
                EventType = (DtoCalendarEventType)999 // Invalid enum value
            }
        };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(request, CancellationToken.None));
    }
}
