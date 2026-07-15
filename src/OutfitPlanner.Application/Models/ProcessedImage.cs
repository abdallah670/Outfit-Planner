namespace OutfitPlanner.Application.Contracts.Infrastructure.Models{
public class ProcessedImage : IDisposable
{
    public Stream Original { get; set; } = Stream.Null;

    public string FileName { get; set; } = string.Empty;

    public string Extension { get; set; } = ".jpg";

    public ImageMetadata Metadata { get; set; } = new();

    public Guid ImageId { get; set; } = Guid.NewGuid();

    public void Dispose()
    {
        Original?.Dispose();
    }
}

}
