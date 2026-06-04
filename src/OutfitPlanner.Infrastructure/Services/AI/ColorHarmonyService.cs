using OutfitPlanner.Application.Contracts.Infrastructure;

namespace OutfitPlanner.Infrastructure.Services.AI;

public class ColorHarmonyService : IColorHarmonyService
{
    private static readonly Dictionary<string, (double H, double S, double V)> KnownColors = new()
    {
        // Reds
        ["#FF0000"] = (0, 1, 1), ["#DC143C"] = (348, 0.91, 0.86), ["#B22222"] = (0, 0.81, 0.70),
        // Pinks
        ["#FFC0CB"] = (350, 0.24, 1), ["#FFB6C1"] = (351, 0.29, 1), ["#F8B4C4"] = (350, 0.28, 0.97),
        // Blues
        ["#0000FF"] = (240, 1, 1), ["#4169E1"] = (225, 0.71, 0.88), ["#87CEEB"] = (197, 0.43, 0.92),
        ["#4D96FF"] = (217, 0.70, 1), ["#667EEA"] = (235, 0.56, 0.92), ["#A2C2E6"] = (210, 0.30, 0.90),
        // Greens
        ["#008000"] = (120, 1, 0.50), ["#9CAF88"] = (95, 0.22, 0.69), ["#6BCB77"] = (130, 0.48, 0.80),
        // Yellows
        ["#FFFF00"] = (60, 1, 1), ["#FDCB6E"] = (45, 0.56, 0.99), ["#FFD93D"] = (48, 0.76, 1),
        // Purples
        ["#800080"] = (300, 1, 0.50), ["#A78BFA"] = (255, 0.44, 0.98), ["#764BA2"] = (275, 0.54, 0.64),
        ["#F093FB"] = (295, 0.41, 0.98),
        // Neutrals
        ["#000000"] = (0, 0, 0), ["#FFFFFF"] = (0, 0, 1), ["#2D3436"] = (210, 0.16, 0.21),
        ["#636E72"] = (200, 0.12, 0.45), ["#DFE6E9"] = (190, 0.07, 0.91),
        ["#6B7280"] = (220, 0.12, 0.50), ["#1F2937"] = (215, 0.30, 0.22),
        // Oranges
        ["#FFA500"] = (39, 1, 1), ["#E17055"] = (15, 0.63, 0.88), ["#F5576C"] = (350, 0.65, 0.96),
        ["#DB2777"] = (330, 0.82, 0.86), ["#BE185D"] = (335, 0.87, 0.75), ["#831843"] = (335, 0.81, 0.51),
        ["#FCE7F3"] = (330, 0.08, 0.99), ["#FBCFE8"] = (320, 0.18, 0.99),
        ["#FDF2F8"] = (330, 0.04, 0.99), ["#EF4444"] = (0, 0.74, 0.94),
    };

    public Task<ColorHarmonyResult> CalculateHarmonyAsync(
        IEnumerable<string> hexColors,
        CancellationToken cancellationToken = default)
    {
        var colors = hexColors.ToList();
        if (colors.Count < 2)
        {
            return Task.FromResult(new ColorHarmonyResult
            {
                Score = 70,
                Scheme = "Monochromatic",
                Explanation = "A single color is always harmonious.",
                HexColors = colors
            });
        }

        var hsvColors = colors.Select(HexToHsv).ToList();
        var scheme = DetectScheme(hsvColors);
        var score = CalculateSchemeScore(hsvColors, scheme);
        var explanation = GenerateExplanation(scheme, score, colors);

        return Task.FromResult(new ColorHarmonyResult
        {
            Score = score,
            Scheme = scheme,
            Explanation = explanation,
            HexColors = colors
        });
    }

    private static (double H, double S, double V) HexToHsv(string hex)
    {
        var normalized = hex.TrimStart('#').ToUpperInvariant();
        if (KnownColors.TryGetValue($"#{normalized}", out var known))
            return known;

        if (normalized.Length != 6)
            return (0, 0, 0);

        var r = Convert.ToInt32(normalized[..2], 16) / 255.0;
        var g = Convert.ToInt32(normalized[2..4], 16) / 255.0;
        var b = Convert.ToInt32(normalized[4..6], 16) / 255.0;

        var max = Math.Max(r, Math.Max(g, b));
        var min = Math.Min(r, Math.Min(g, b));
        var delta = max - min;

        double h = 0;
        if (delta > 0)
        {
            if (max == r) h = 60 * (((g - b) / delta) % 6);
            else if (max == g) h = 60 * (((b - r) / delta) + 2);
            else h = 60 * (((r - g) / delta) + 4);
        }
        if (h < 0) h += 360;

        double s = max == 0 ? 0 : delta / max;
        double v = max;

        return (h, s, v);
    }

    private static string DetectScheme(List<(double H, double S, double V)> colors)
    {
        var hues = colors.Select(c => c.H).OrderBy(h => h).ToList();
        var hueDiffs = new List<double>();
        for (int i = 0; i < hues.Count; i++)
        {
            for (int j = i + 1; j < hues.Count; j++)
            {
                var diff = Math.Abs(hues[i] - hues[j]);
                hueDiffs.Add(Math.Min(diff, 360 - diff));
            }
        }

        if (!hueDiffs.Any()) return "Monochromatic";
        var avgDiff = hueDiffs.Average();

        if (avgDiff < 15) return "Monochromatic";
        if (avgDiff >= 140 && avgDiff <= 180) return "Complementary";
        if (avgDiff >= 50 && avgDiff <= 70) return "Analogous";
        if (avgDiff >= 110 && avgDiff <= 130) return "Triadic";
        if (avgDiff >= 80 && avgDiff <= 100) return "Split-Complementary";
        
        var isAllNeutral = colors.All(c => c.S < 0.15);
        if (isAllNeutral) return "Neutral";

        return "Custom";
    }

    private static double CalculateSchemeScore(List<(double H, double S, double V)> colors, string scheme)
    {
        var baseScore = scheme switch
        {
            "Monochromatic" => 90,
            "Complementary" => 85,
            "Analogous" => 80,
            "Triadic" => 75,
            "Split-Complementary" => 75,
            "Neutral" => 85,
            _ => 65
        };

        // Penalize if too many high-saturation colors clash
        var saturatedCount = colors.Count(c => c.S > 0.7);
        var saturationPenalty = Math.Max(0, saturatedCount - 2) * 5;

        // Bonus for mixing saturated with neutrals
        var hasNeutral = colors.Any(c => c.S < 0.15);
        var hasColorful = colors.Any(c => c.S > 0.3);
        var mixBonus = hasNeutral && hasColorful ? 5 : 0;

        return Math.Clamp(baseScore - saturationPenalty + mixBonus, 0, 100);
    }

    private static string GenerateExplanation(string scheme, double score, List<string> colors)
    {
        var schemeDesc = scheme switch
        {
            "Monochromatic" => "uses variations of the same color hue",
            "Complementary" => "pairs colors from opposite sides of the color wheel",
            "Analogous" => "uses colors that sit next to each other on the color wheel",
            "Triadic" => "uses three evenly spaced colors on the color wheel",
            "Split-Complementary" => "uses a base color and the two colors adjacent to its complement",
            "Neutral" => "uses neutral tones for a subtle, elegant look",
            _ => "combines colors in a unique arrangement"
        };

        var quality = score switch
        {
            >= 85 => "excellent",
            >= 75 => "good",
            >= 60 => "decent",
            _ => "fair"
        };

        return $"This palette {schemeDesc}. The color harmony score of {score}/100 is {quality}. " +
               $"Colors: {string.Join(", ", colors)}.";
    }
}