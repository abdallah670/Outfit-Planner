using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Features.Admin.DTOs;
using OutfitPlanner.Application.Features.Admin.Requests.Queries;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Features.Admin.Handlers.Queries;

public class GetAllSettingsQueryHandler : IRequestHandler<GetAllSettingsQuery, List<SystemSettingDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetAllSettingsQueryHandler> _logger;

    public GetAllSettingsQueryHandler(IUnitOfWork unitOfWork, ILogger<GetAllSettingsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<List<SystemSettingDto>> Handle(GetAllSettingsQuery request, CancellationToken cancellationToken)
    {
        var settings = await _unitOfWork.Repository<SystemSetting>()
            .GetQueryable()
            .OrderBy(s => s.Category)
            .ThenBy(s => s.Key)
            .Select(s => new SystemSettingDto(
                s.Key,
                s.Value,
                s.DataType,
                s.Description,
                s.IsEditable
            ))
            .ToListAsync(cancellationToken);
            
        return settings;
    }
}
