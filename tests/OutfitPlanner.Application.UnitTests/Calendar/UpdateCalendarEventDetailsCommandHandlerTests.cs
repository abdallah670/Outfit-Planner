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

public class UpdateCalendarEventDetailsCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<UpdateCalendarEventDetailsCommandHandler>> _loggerMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly UpdateCalendarEventDetailsCommandHandler _handler;

    public UpdateCalendarEventDetailsCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<UpdateCalendarEventDetailsCommandHandler>>();
        _mapperMock = new Mock<IMapper>();
        _handler = new UpdateCalendarEventDetailsCommandHandler(
            _unitOfWorkMock.Object,
            _loggerMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldUpdateCalendarEvent()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var eventId = Guid.NewGuid();
        var existingEvent = new Domain.Entities.CalendarEvent
        {
            Id = eventId,
            UserId = userId,
            Title = "Old Title",
            Description = "Old Description",
            EventDate = DateTimeOffset.Now.AddDays(1),
            EventType = Domain.Enums.CalendarEventType.General
        };

        var request = new UpdateCalendarEventDetailsCommand
        {
            Id = eventId,
            UserId = userId,
            Request = new UpdateCalendarEventItemRequest
            {
                Title = "Updated Title",
                Description = "Updated Description",
                Location = "New Location",
                EventType = DtoCalendarEventType.Work
            }
        };

        _unitOfWorkMock.Setup(u => u.CalendarEvents.GetByIdAsync(eventId))
            .ReturnsAsync(existingEvent);
        _unitOfWorkMock.Setup(u => u.CalendarEvents.UpdateAsync(It.IsAny<Domain.Entities.CalendarEvent>()))
            .Returns(Task.CompletedTask);
        //_unitOfWorkMock.Setup(u => u.SaveChangesAsync())
          //  .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Calendar event updated successfully");
        existingEvent.Title.Should().Be("Updated Title");
        existingEvent.Description.Should().Be("Updated Description");
        existingEvent.Location.Should().Be("New Location");
    }

    [Fact]
    public async Task Handle_EventNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var eventId = Guid.NewGuid();

        var request = new UpdateCalendarEventDetailsCommand
        {
            Id = eventId,
            UserId = userId,
            Request = new UpdateCalendarEventItemRequest { Title = "New Title" }
        };

        _unitOfWorkMock.Setup(u => u.CalendarEvents.GetByIdAsync(eventId))
            .ReturnsAsync((Domain.Entities.CalendarEvent?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_DifferentUser_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var differentUserId = Guid.NewGuid().ToString();
        var eventId = Guid.NewGuid();
        var existingEvent = new Domain.Entities.CalendarEvent
        {
            Id = eventId,
            UserId = differentUserId,
            Title = "Original Title",
            EventDate = DateTimeOffset.Now.AddDays(1),
            EventType = Domain.Enums.CalendarEventType.General
        };

        var request = new UpdateCalendarEventDetailsCommand
        {
            Id = eventId,
            UserId = userId,
            Request = new UpdateCalendarEventItemRequest { Title = "New Title" }
        };

        _unitOfWorkMock.Setup(u => u.CalendarEvents.GetByIdAsync(eventId))
            .ReturnsAsync(existingEvent);

        // Act & Assert
        await Assert.ThrowsAsync<Exceptions.UnauthorizedAccessException>(() => _handler.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_AddOutfitToExistingEvent_ShouldCreateWearEvent()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var eventId = Guid.NewGuid();
        var outfitId = Guid.NewGuid();
        var outfit = new Outfit
        {
            Id = outfitId,
            UserId = userId,
            User = null!,
            Items = new List<OutfitItem>()
        };

        var existingEvent = new Domain.Entities.CalendarEvent
        {
            Id = eventId,
            UserId = userId,
            Title = "Meeting",
            EventDate = DateTimeOffset.Now.AddDays(1),
            EventType = Domain.Enums.CalendarEventType.General,
            WearEventId = null
        };

        var request = new UpdateCalendarEventDetailsCommand
        {
            Id = eventId,
            UserId = userId,
            Request = new UpdateCalendarEventItemRequest
            {
                OutfitId = outfitId
            }
        };

        _unitOfWorkMock.Setup(u => u.CalendarEvents.GetByIdAsync(eventId))
            .ReturnsAsync(existingEvent);
        _unitOfWorkMock.Setup(u => u.Outfits.GetByIdAsync(outfitId))
            .ReturnsAsync(outfit);
        _unitOfWorkMock.Setup(u => u.WearEvents.AddAsync(It.IsAny<WearEvent>()))
            .Returns(Task.CompletedTask);
      //  _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
        //    .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_UpdateOutfitInExistingWearEvent_ShouldUpdateWearEvent()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var eventId = Guid.NewGuid();
        var wearEventId = Guid.NewGuid();
        var outfitId = Guid.NewGuid();
        var outfit = new Outfit
        {
            Id = outfitId,
            UserId = userId,
            User = null!,
            Items = new List<OutfitItem>()
        };
        var wearEvent = new WearEvent
        {
            Id = wearEventId,
            UserId = userId,
            OutfitId = Guid.NewGuid(), // Old outfit
            WornAt = DateTimeOffset.Now
        };

        var existingEvent = new Domain.Entities.CalendarEvent
        {
            Id = eventId,
            UserId = userId,
            Title = "Meeting",
            EventDate = DateTimeOffset.Now.AddDays(1),
            EventType = Domain.Enums.CalendarEventType.General,
            WearEventId = wearEventId
        };

        var request = new UpdateCalendarEventDetailsCommand
        {
            Id = eventId,
            UserId = userId,
            Request = new UpdateCalendarEventItemRequest
            {
                OutfitId = outfitId
            }
        };

        _unitOfWorkMock.Setup(u => u.CalendarEvents.GetByIdAsync(eventId))
            .ReturnsAsync(existingEvent);
        _unitOfWorkMock.Setup(u => u.Outfits.GetByIdAsync(outfitId))
            .ReturnsAsync(outfit);
        _unitOfWorkMock.Setup(u => u.WearEvents.GetByIdAsync(wearEventId))
            .ReturnsAsync(wearEvent);
        _unitOfWorkMock.Setup(u => u.WearEvents.UpdateAsync(It.IsAny<WearEvent>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        wearEvent.OutfitId.Should().Be(outfitId);
    }
}
