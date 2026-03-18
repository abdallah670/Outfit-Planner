namespace OutfitPlanner.Infrastructure.Services.Models;

/// <summary>
/// Represents a grid layout for outfit image composition
/// </summary>
public class GridLayout
{
    public int CanvasWidth { get; set; }
    public int CanvasHeight { get; set; }
    public List<GridCell> Cells { get; set; } = new();
}

/// <summary>
/// Represents a single cell in the grid layout
/// </summary>
public class GridCell
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    public GridCell(int x, int y, int width, int height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }
}
