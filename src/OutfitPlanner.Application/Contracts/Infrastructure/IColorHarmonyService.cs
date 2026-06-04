namespace OutfitPlanner.Application.Contracts.Infrastructure;

public class ColorHarmonyResult
{
    public double Score { get; set; }
    public string Scheme { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public List<string> HexColors { get; set; } = new();
}

public interface IColorHarmonyService
{
    Task<ColorHarmonyResult> CalculateHarmonyAsync(
        IEnumerable<string> hexColors,
        CancellationToken cancellationToken = default);
}