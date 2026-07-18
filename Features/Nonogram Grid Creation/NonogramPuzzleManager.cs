using Common;
using Features.NonogramGridCreation.BarGeneration;
using Godot;
using Godot.Collections;
using System;

namespace Features.NonogramGridCreation;

[Tool]
public partial class NonogramPuzzleManager : PanelContainer
{
    public static NonogramPuzzleManager Instance { get; private set; }

    [ExportCategory("Grid Generation")]
    [Export] Image Level { get; set; }

    public Vector2I CellCount;

    // *** Hint Generation ***
    public Array<R_BarHint> h_hints;
    public Array<R_BarHint> v_hints;

    bool isColorful;

    public Array<Color> colorList;

    public override void _Ready()
    {
        if (Level == null)
        {
            GameLogger.Error("Level image not initialized");
            return;
        }

        CellCount = Level.GetSize();
        colorList.Clear();

        // create hints for h and v bars
        for (int x = 0; x < CellCount.X; x++)
            h_hints.Add(GetBarHint(x, CellCount.Y, false));
        for (int y = 0; y < CellCount.Y; y++)
            v_hints.Add(GetBarHint(y, CellCount.X, true));

        if (colorList.Count == 1)
            isColorful = false;
        else
            isColorful = true;

        GridCreation.Instance.cellCount = CellCount;
        GridCreation.Instance.QueueRedraw();
    }

    private R_BarHint GetBarHint(int notch, int altNotch, bool isVertical)
    {
        R_BarHint barHint = new();
        Vector2I index;

        int curAmount = 0;
        Color curColor = Colors.White;

        Color prevColor = Colors.White;

        // iterate thru cells in column/row
        for(int y = 0; y < altNotch; y++)
        {
            if (isVertical)
                index = new Vector2I(y, notch);
            else
                index = new Vector2I(notch, y);

            curColor = Level.GetPixelv(index);

            // if there's a swap to white, but we have a count going, add number
            if (curColor == Colors.White && curAmount > 0)
            {
                barHint.numberList.Add(curAmount);
                curAmount = 0;
                TryAddColorToList(prevColor);
                barHint.colorList.Add(prevColor);
                continue;
            }
            else if (curColor == Colors.White)
                continue;

            // if there's a color swap, and we have a count going
            if (prevColor != curColor && curAmount > 0)
            {
                barHint.numberList.Add(curAmount);
                curAmount = 0;
                TryAddColorToList(prevColor);
                barHint.colorList.Add(prevColor);
            }

            // Add to count when have the same color
            if (curAmount > 0 && curColor == prevColor)
                curAmount = 0;
            else
            {
                barHint.numberList.Add(curAmount);
                curAmount = 0;
                TryAddColorToList(curColor);
                barHint.colorList.Add(curColor);
            }

            prevColor = curColor;
        }

        // last check in case of notch being filled with color all the way to the end
        if (curAmount > 0)
        {
            barHint.numberList.Add(curAmount);
            TryAddColorToList(curColor);
            barHint.colorList.Add(curColor);
        }

        return barHint;
    }

    private void TryAddColorToList(Color newColor)
    {
        bool isNewColor = true;

        for (int i = 0; i < colorList.Count; i++)
        {
            if (colorList[i] == newColor)
            {
                isNewColor = false;
                break;
            }
        }

        if (isNewColor)
            colorList.Add(newColor);
    }

    public override void _EnterTree()
    {
        if (Instance != null && Instance != this)
        {
            GameLogger.Warning("Excess instance of singleton. Deleting...");
            QueueFree();
            return;
        }

        Instance = this;
    }
}
