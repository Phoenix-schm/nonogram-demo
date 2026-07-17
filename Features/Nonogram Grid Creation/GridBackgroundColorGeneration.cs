using Godot;
using System;

namespace Features.NonogramGridCreation;

[Tool]
public partial class GridBackgroundColorGeneration : Control
{
    [Export] Color GridColor { 
        get { return gridColor; }
        set { gridColor = value;
            QueueRedraw();
            } }
    Color gridColor;

    GridCreation gridLogic;

    public override void _Draw()
    {
        if (GridCreation.Instance == null)
            return;

        float cellSize = GridCreation.Instance.cellSize;
        DrawRect(
            new Rect2(
                new Vector2(0, 0),
                new Vector2(GridCreation.Instance.CellCount.X * cellSize, GridCreation.Instance.CellCount.Y * cellSize)
                ),
                GridColor
            );
    }

    public override void _EnterTree()
    {
        GridCreation.OnGridFinishedInitializing += QueueRedraw;
    }
    public override void _ExitTree()
    {
        GridCreation.OnGridFinishedInitializing -= QueueRedraw;
    }
}
