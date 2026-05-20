namespace OutfitPlanner.Application.Contracts.Infrastructure.Models{
public class ProcessedImage : IDisposable
{
   
    public Stream Original { get; set; } = Stream.Null;

    
    public Stream Thumbnail { get; set; } = Stream.Null;

   
    public Stream Medium { get; set; } = Stream.Null;

    
    public Stream Large { get; set; } = Stream.Null;

    public string FileName { get; set; } = string.Empty;

  
    public string Extension { get; set; } = ".jpg";

    public ImageMetadata Metadata { get; set; } = new();

    
    public Guid ImageId { get; set; } = Guid.NewGuid();

    public void Dispose()
    {
        Original?.Dispose();
        Thumbnail?.Dispose();
        Medium?.Dispose();
        Large?.Dispose();
    }
}

}
