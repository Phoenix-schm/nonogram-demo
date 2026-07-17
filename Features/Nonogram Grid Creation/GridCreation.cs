using Godot;
using System;

namespace Features.NonogramGridCreation;

[Tool]
public partial class GridCreation : Container
{
    [ExportCategory("Line Genration")]
    [Export] public int DividerCount { get; set; }
    [Export] public Color MainLineColor { get; set; }
    [Export] public Color DividerLineColor { get; set; }

    [Export] public Vector2I CellCount { get; set; }

    private Vector2 gridSize;
    private float cellSize;     // uniform square size


    public override void _Draw()
    {
        InitializeGrid();
        DrawGrid();
    }

    private void InitializeGrid()
    {
        gridSize = GetViewportRect().Size;
        cellSize = CalculateCellSize(gridSize);
    }
    private void DrawGrid()
    {
        // iterate through CellCount.X/Y
        // draw line up/dow, left/right
    }

    private float CalculateCellSize(Vector2 _viewportSize)
    {
        float shortestSize;

        // Get the shortest side of the viewport
        if (_viewportSize.X > _viewportSize.Y)
            shortestSize = _viewportSize.X;
        else
            shortestSize = _viewportSize.Y;

        // Cell size is set to be the smallest size it needs to be to fit the longest side of the grid
        // allows cell sizes to be square
        if (CellCount.X > CellCount.Y)
            return shortestSize / CellCount.X;
        else
            return shortestSize / CellCount.Y;
    }
}
