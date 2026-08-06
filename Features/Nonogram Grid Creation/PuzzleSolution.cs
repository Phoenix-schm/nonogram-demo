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
        if (NonogramPuzzleManager.Instance == null && GridCreation.Instance == null)
            return;

        if (!ShowSolution)
            return;

        Vector2I cellCount = NonogramPuzzleManager.CellCount;
        float cellSize = GridCreation.Instance.cellSize;
        bool isColorful = NonogramPuzzleManager.Instance.IsColorful;

        for (int x = 0; x < cellCount.X; x++)
        {
            for (int y = 0; y < cellCount.Y; y++)
            {
                int fauxCurIndex = x + (y * cellCount.X); // translate Vector2I into flattened index
                int colorIndex = NonogramPuzzleManager.Instance.FullColorGrid[fauxCurIndex];

                if (colorIndex == 0)    // if it's white
                    continue;
                Color cellColor;

                if (isColorful)
                    cellColor = NonogramPuzzleManager.Instance.GetColorFromList(colorIndex);
                else
                    cellColor = altCellColor;

                DrawRect(
                    new Rect2(new Vector2(x * cellSize, y * cellSize),
                    new Vector2(cellSize, cellSize)),
                    cellColor
                    );
            }
        }
    }


    public override void _EnterTree()
    {
        GridCreation.OnGridFinishedInitializing += QueueRedraw;
        NonogramPuzzleManager.OnManualColorDictChanged += QueueRedraw;
    }

    public override void _ExitTree()
    {
        GridCreation.OnGridFinishedInitializing -= QueueRedraw;
        NonogramPuzzleManager.OnManualColorDictChanged -= QueueRedraw;
    }
}
