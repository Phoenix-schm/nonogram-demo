using Common;
using Features.NonogramGridCreation.BarGeneration;
using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

namespace Features.NonogramGridCreation;

[Tool]
public partial class NonogramPuzzleManager : PanelContainer
{
    public static NonogramPuzzleManager Instance { get; private set; }

    [ExportCategory("Grid Generation")]
    [Export] Image Level { get; set; }

    public static Vector2I CellCount { get; private set; }

    // *** Hint Generation ***
    public Array<RC_NotchHint> h_barHint = new();
    public Array<RC_NotchHint> v_barHint = new();

    public bool isColorful;

    public HashSet<Color> uniqueColorList = new();

    public override void _Ready()
    {
        if (Level == null)
        {
            GameLogger.Error("Level image not initialized");
            return;
        }

        uniqueColorList = new();
        h_barHint = new();
        v_barHint = new();

        // create hints for h and v bars
        for (int x = 0; x < CellCount.X; x++)
            h_barHint.Add(GetNotchHint(x, CellCount.Y, false));
        for (int y = 0; y < CellCount.Y; y++)
            v_barHint.Add(GetNotchHint(y, CellCount.X, true));

        isColorful = uniqueColorList.Count > 1;

        GridCreation.Instance.cellCount = CellCount;
        GridCreation.Instance.QueueRedraw();
    }

    private RC_NotchHint GetNotchHint(int notch, int altNotch, bool isVertical)
    {
        RC_NotchHint notchHint = new();
        Vector2I index;

        int curAmount = 0;
        Color curColor = Colors.White;
        Color prevColor = Colors.White;

        // iterate through cells in current column/row
        for(int cell = 0; cell < altNotch; cell++)
        {
            index = isVertical ? new(cell, notch) : new(notch, cell);
            curColor = Level.GetPixelv(index);

            // if there's a swap to white, but we have a count going, add number
            if (curColor == Colors.White && curAmount > 0)
            {
                notchHint.numberList.Add(curAmount);
                curAmount = 0;
                uniqueColorList.Add(prevColor);
                notchHint.colorList.Add(prevColor);
                continue;
            }
            else if (curColor == Colors.White) // move on
                continue;

            // if there's a color swap, and we have a count going
            if (prevColor != curColor && curAmount > 0)
            {
                notchHint.numberList.Add(curAmount);
                curAmount = 0;
                uniqueColorList.Add(prevColor);
                notchHint.colorList.Add(prevColor);
            }

            // Add to count when have the same color
            if ((curAmount > 0 && curColor == prevColor) || curAmount == 0)
                curAmount += 1;
            else
            {
                notchHint.numberList.Add(curAmount);
                curAmount = 0;
                uniqueColorList.Add(curColor);
                notchHint.colorList.Add(curColor);
            }
            prevColor = curColor;
        }

        // last check in case of notch being filled with color all the way to the end
        if (curAmount > 0)
        {
            notchHint.numberList.Add(curAmount);
            uniqueColorList.Add(curColor);
            notchHint.colorList.Add(curColor);
        }

        return notchHint;
    }

    public override void _EnterTree()
    {
        if (!Engine.IsEditorHint())
        {
            if (Instance != null && Instance != this)
            {
                GameLogger.Warning("Excess instance of singleton. Deleting...");
                QueueFree();
                return;
            }
        }

        if (Level == null)
        {
            GameLogger.Error("Level image not initialized");
            return;
        }

        CellCount = Level.GetSize();

        Instance = this;
    }
}
