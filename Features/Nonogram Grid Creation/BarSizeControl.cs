using Common;
using Features.NonogramGridCreation.BarGeneration;
using Godot;
using Godot.Collections;
using System;

namespace Features.NonogramGridCreation;

public partial class BarSizeControl : PanelContainer
{
    public Vector2 Consequence;

    int h_longestHint;
    int v_longestHint;

    Vector2 viewportSize;
    Vector2 gridViewportSize;

    float cellSize;
    Vector2I cellCount;
    Vector2 defaultBarRatio;           // how far the bars are allowed to take up the screen until. 1/4th
    Vector2 secondaryBarRatio;         // if default doesn't work, use soncdary. 1/2
    bool isValidConsequence;

    float defaultFontSizeModifier = .8f;
    float minimumFontSize = 20;


    public override void _Ready()
    {
        cellCount = NonogramPuzzleManager.CellCount;
        h_longestHint = GetLargetBarHintNotch(NonogramPuzzleManager.Instance.h_hints);
        v_longestHint = GetLargetBarHintNotch(NonogramPuzzleManager.Instance.v_hints);

        viewportSize = GetViewportRect().Size;
        gridViewportSize = GridCreation.Instance.GetViewportRect().Size;

        defaultBarRatio = viewportSize / 4;
        secondaryBarRatio = viewportSize / 2;

        // How much room is left for bars to fill up
        // will be used to set minimum bar size control
        Consequence = viewportSize - gridViewportSize;

        cellSize = CalculateCellSize(viewportSize);

        // starting font size for calculating bar length
        float pseudoFontSize = cellSize * defaultFontSizeModifier;

        //TODO: Calculate notch heights for v/h bars

        //TODO: Calculate room for improvement by consequence - longest sides of bars
        Vector2 roomForImprovement;

        bool canSetMinimum = false;

        while (!canSetMinimum)
        {
            // if room for improvement has negative numbers,
            //      then numbers will be going off screen

            // if already hitting defaultBarRatio,
            //      adjust fontsizemodifer to make font smaller.
            //      Recalculate consequence

            // if hitting minimum allowed font size,
            //      use secondaryBarRatio and adjust for that. recalculate

            // if haven't hit bar ratio, make consquence bigger and recalculate

            // if roomForImprovement  > (font_size - font_size * 1.1) <- calc for slight offset from top/left
            //      there's still room for improvement
            //      shift consequence with roomForImprovement and recalculate



            canSetMinimum = true;
            break;
        }

        // set minimum consequence
    }

    public override void _Notification(int what)
    {
        if (what == NotificationReady)
        {
        }
        if (what == NotificationResized)
        {

        }
    }

    private bool IsConsequenceLargerThanBarRatio(Vector2 barRatio)
    {
        bool isLarger = false;
        if (Consequence.X > barRatio.X)
        {
            Consequence.X = barRatio.X;
            isLarger = true;
        }

        if (Consequence.Y > barRatio.Y)
        {
            Consequence.Y = barRatio.Y;
            isLarger = true;
        }

        return isLarger;

    }

    public static int GetLargetBarHintNotch(Array<R_BarHint> barHint)
    {
        int largetInt = -1;
        int index = -1;

        for (int i = 0;  i < barHint.Count; i++)
        {
            if (barHint[i].numberList.Count > largetInt)
            {
                largetInt = barHint[i].numberList.Count;
                index = i;
            }
        }

        return barHint[index].numberList.Count;
    }

    private float CalculateCellSize(Vector2 _viewportSize)
    {
        if (cellCount.X <= 0 || cellCount.Y <= 0)
        {
            GameLogger.Error($"Cell Count contains less than 0 integer: {cellCount}");
            return 0;
        }

        float shortestSize;

        // Get the shortest side of the viewport
        if (_viewportSize.X < _viewportSize.Y)
            shortestSize = _viewportSize.X;
        else
            shortestSize = _viewportSize.Y;

        // Cell size is set to be the smallest size it needs to be to fit the longest side of the grid
        // allows cell sizes to be square
        if (cellCount.X > cellCount.Y)
            return shortestSize / cellCount.X;
        else
            return shortestSize / cellCount.Y;
    }
}
