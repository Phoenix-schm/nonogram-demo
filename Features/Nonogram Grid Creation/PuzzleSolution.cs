using Godot;
using System;

namespace Features.NonogramGridCreation;

[Tool]
public partial class PuzzleSolution : PanelContainer
{
    private bool showSolution;
    [Export] bool ShowSolution {
        get { return showSolution; }
        set 
        {
            showSolution = value;
            QueueRedraw(); 
        } }

    Color altCellColor;
    [Export] Color AltCellColor { 
        get { return altCellColor; } 
        set 
        { 
            altCellColor = value;
            QueueRedraw(); 
        } }

    public override void _Draw()
    {
        if (NonogramPuzzleManager.Instance == null)
            return;

        if (!ShowSolution)
            return;

        Vector2I cellCount = NonogramPuzzleManager.CellCount;
        float cellSize = GridCreation.Instance.cellSize;

        for (int x = 0; x < cellCount.X; x++)
        {
            for (int y = 0; y < cellCount.Y; y++)
            {
                Color cellColor = NonogramPuzzleManager.Instance.Level.GetPixel(x, y);

                if (cellColor == Colors.Black && !NonogramPuzzleManager.Instance.isColorful)
                    cellColor = altCellColor;
                if (cellColor == Colors.White || cellColor.A < 1)
                    continue;

                DrawRect(
                    new Rect2(new Vector2(x * cellSize, y * cellSize), new Vector2(cellSize, cellSize)),
                    cellColor
                    );
            }
        }
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
