using MediatR;

namespace OutfitPlanner.Application.Features.User.Requests.Queries;

public class ExportUserDataQuery : IRequest<ExportUserDataResult>
{
    public string UserId { get; set; } = string.Empty;
}

public class ExportUserDataResult
{
    public byte[] Data { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = "text/csv";
    public string FileName { get; set; } = "export.csv";
}
