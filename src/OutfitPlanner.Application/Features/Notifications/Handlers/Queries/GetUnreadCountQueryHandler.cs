using MediatR;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Exceptions;
using OutfitPlanner.Application.Features.Notifications.Requests.Queries;

namespace OutfitPlanner.Application.Features.Notifications.Handlers.Queries;

public class GetUnreadCountQueryHandler : IRequestHandler<GetUnreadCountQuery, int>
{
    private readonly ILogger<GetUnreadCountQueryHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public GetUnreadCountQueryHandler(ILogger<GetUnreadCountQueryHandler> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(GetUnreadCountQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate request
            if (string.IsNullOrEmpty(request.UserId))
            {
                _logger.LogWarning("User ID is required to get unread notification count");
                throw new BadRequestException("User ID is required");
            }

            // Check if user exists
            var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found while attempting to get unread notification count", request.UserId);
                throw new NotFoundException("User", request.UserId);
            }

            var count = await _unitOfWork.Notifications.GetUnreadCountAsync(request.UserId);

            _logger.LogInformation("User {UserId} has {Count} unread notifications", request.UserId, count);

            return count;
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (BadRequestException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while getting unread notification count for user {UserId}", request.UserId);
            throw new BadRequestException("An error occurred while getting unread notification count. Please try again later.");
        }
    }
}
