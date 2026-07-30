using Common;
using Features.NonogramGridCreation.BarGeneration;
using Godot;
using Godot.Collections;
using System;

namespace Features.NonogramGridCreation;

public partial class BarSizeControl : PanelContainer
{
    public static event Action OnBarControlInitialized;
    public static Vector2 Consequence;

    int h_longestHint;
    int v_longestHint;

    Vector2 viewportSize;
    Vector2 gridViewportSize;

    float cellSize;
    Vector2I cellCount;
    Vector2 defaultBarRatio;           // how far the bars are allowed to take up the screen until. 1/4th
    Vector2 secondaryBarRatio;         // if default doesn't work, use soncdary. 1/2
    bool isValidConsequence;

    public static float defaultFontSizeModifier = .8f;
    int minimumFontSize = 12;


    public async override void _Ready()
    {
        await ToSignal(GetTree(), StaticStringRef.s_processFrame);

        cellCount = NonogramPuzzleManager.CellCount;
        h_longestHint = GetLargetBarHintNotch(NonogramPuzzleManager.Instance.h_hints);
        v_longestHint = GetLargetBarHintNotch(NonogramPuzzleManager.Instance.v_hints);

        UpdateMinSizeWithConsequence();
    }

    private async void UpdateMinSizeWithConsequence()
    {
        await ToSignal(GetTree(), StaticStringRef.s_processFrame);

        viewportSize = GetParentAreaSize();
        gridViewportSize = GridCreation.Instance.GetViewportRect().Size;

        defaultBarRatio = viewportSize / 4;
        secondaryBarRatio = viewportSize / 3;

        // How much room is left for bars to fill up
        // will be used to set minimum bar size control
        Consequence = viewportSize - gridViewportSize;

        bool canSetMinimum = false;

        while (!canSetMinimum)
        {
            gridViewportSize = viewportSize - Consequence;
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



            canSetMinimum = UpdateConsequence();
        }

        OnBarControlInitialized?.Invoke();
        // set minimum consequence
        CustomMinimumSize = Consequence;
    }

    bool useSecondaryBarRatio;

    private bool UpdateConsequence()
    {
        bool returnValid = true;

        cellSize = CalculateCellSize(gridViewportSize);

        // starting font size for calculating bar length
        float pseudoFontSize = cellSize * defaultFontSizeModifier;

        // Calculate length/height of bars based on font size
        float calcHLength = GetNotchHBarLength(h_longestHint, pseudoFontSize, Mathf.RoundToInt(pseudoFontSize / 4));
        float calcVLength = GetNotchVBarLength(v_longestHint, pseudoFontSize, Mathf.RoundToInt(pseudoFontSize / 4));

        Vector2 calcLongestNotches = new Vector2(calcVLength, calcHLength);

        // Calculate how much room is left between consequence and longest notches
        Vector2 roomForImprovement = Consequence - calcLongestNotches;
        roomForImprovement = new Vector2(Mathf.CeilToInt(roomForImprovement.X), Mathf.CeilToInt(roomForImprovement.Y));

        // longest notches are off screen and must adjust font or consequence
        if (roomForImprovement.X < 0 || roomForImprovement.Y < 0)
        {
            // if Consequence is taking up 1/4th screen already
            if (Consequence.IsEqualApprox(defaultBarRatio) && !useSecondaryBarRatio || (Consequence > defaultBarRatio) && (Consequence < secondaryBarRatio) && !useSecondaryBarRatio)
            {
                // Try modifying font size
                defaultFontSizeModifier -= .05f;
                returnValid = false;

                // if font size too small, try to use secondary bar ratio
                if (cellSize *  defaultFontSizeModifier < minimumFontSize)
                {
                    defaultFontSizeModifier = .8f;
                    useSecondaryBarRatio = true;
                }
            }
            else if (useSecondaryBarRatio && (Consequence >= secondaryBarRatio))
            {
                defaultFontSizeModifier -= .05f;
                returnValid = false;

                if (cellSize * defaultFontSizeModifier < minimumFontSize)
                {
                    defaultFontSizeModifier = .8f;
                    GameLogger.Warning("Could not get appropriate size for font");
                    // break out of while loop
                    return true;
                }
            }
            else // room for imrpovement is too large
            {
                if (roomForImprovement.X < 0)
                    Consequence.X -= roomForImprovement.X;
                if (roomForImprovement.Y < 0)
                    Consequence.Y -= roomForImprovement.Y;
                returnValid = false;
            }
        }

        // Consequence too small, excess space between edge and numbers
        if (roomForImprovement.X > (pseudoFontSize * 1.1f) - pseudoFontSize)
        {
            Consequence.X -= roomForImprovement.X;
            returnValid = false;
        }
        if (roomForImprovement.Y > (pseudoFontSize * 1.1f) - pseudoFontSize)
        {
            Consequence.Y -= roomForImprovement.Y;
            returnValid = false;
        }
        return returnValid;
    }


    public override void _Notification(int what)
    {
        if (what == NotificationResized)
        {
            UpdateMinSizeWithConsequence();
        }
    }

    private bool IsConsequenceLargerThanBarRatio(Vector2 barRatio)
    {
        bool isLarger = false;
        if (Consequence.X > barRatio.X)
        {
            isLarger = true;
        }

        if (Consequence.Y > barRatio.Y)
        {
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

    private float GetNotchHBarLength(int notchSize,float fontSize, float fontDivide)
    {
        return (notchSize * fontSize) + fontDivide;
    }

    private float GetNotchVBarLength(int notchSize, float  fontSize, float fontDivide)
    {
        return (notchSize * fontSize) - fontDivide;
    }
}
