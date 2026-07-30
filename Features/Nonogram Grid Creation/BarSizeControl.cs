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
    int minimumFontSize = 26;

    public async override void _Ready()
    {
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        GetViewport().Connect(StaticStringRef.s_size_changed, Callable.From(OnSizeChanged));

        cellCount = NonogramPuzzleManager.CellCount;
        h_longestHint = GetLargetBarHintNotch(NonogramPuzzleManager.Instance.h_hints);
        v_longestHint = GetLargetBarHintNotch(NonogramPuzzleManager.Instance.v_hints);

        UpdateMinSizeWithConsequence();
    }

    public override void _Notification(int what)
    {
        if (what == NotificationResized)
        {
            // Forces consequence to initialize properly at ready
            UpdateMinSizeWithConsequence();
        }
    }

    private async void UpdateMinSizeWithConsequence()
    {
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        viewportSize = GetParentAreaSize();
        gridViewportSize = GridCreation.Instance.GetViewportRect().Size;

        // How much room is left for bars to fill up
        // will be used to set minimum bar size control
        Consequence = viewportSize - gridViewportSize;

        defaultBarRatio = viewportSize / 4;
        secondaryBarRatio = viewportSize / 3;

        bool canSetMinimum = false;

        while (!canSetMinimum)
        {
            gridViewportSize = viewportSize - Consequence;
            canSetMinimum = UpdateConsequence();
        }

        // set minimum consequence
        CustomMinimumSize = Consequence;
        OnBarControlInitialized?.Invoke();
        useSecondaryBarRatio = false;

        //GameLogger.Info(cellSize);

        //GameLogger.Info($"Consequence: {Consequence}");
        //GameLogger.Info($"Grid Size: {gridViewportSize}");
        //GameLogger.Info($"font modifier: {defaultFontSizeModifier}");
    }

    bool useSecondaryBarRatio;

    private bool UpdateConsequence()
    {
        bool returnValid = true;

        cellSize = CalculateCellSize(gridViewportSize);

        // starting font size for calculating bar length
        float pseudoFontSize = cellSize * defaultFontSizeModifier;

        // Calculate length/height of bars based on font size
        float calcHLength = GetNotchHBarLength(h_longestHint, pseudoFontSize, Mathf.RoundToInt(pseudoFontSize / 2));
        float calcVLength = GetNotchVBarLength(v_longestHint, pseudoFontSize, Mathf.RoundToInt(pseudoFontSize / 4));

        Vector2 calcLongestNotches = new Vector2(calcVLength, calcHLength);

        // Calculate how much room is left between consequence and longest notches
        Vector2 roomForImprovement = Consequence - calcLongestNotches;

        // longest notches are off screen
        if (roomForImprovement.X < 0 || roomForImprovement.Y < 0)
        {
            // if Consequence is already taking up 1/4th screen
            if (!useSecondaryBarRatio && (Consequence.X >= defaultBarRatio.X || Consequence.Y >= defaultBarRatio.Y))
            {
                // Try modifying font size
                defaultFontSizeModifier -= .05f;
                returnValid = false;

                // if font size too small, try to use secondary bar ratio
                if ((cellSize * defaultFontSizeModifier) < minimumFontSize)
                {
                    defaultFontSizeModifier = .8f;
                    //GameLogger.Warning("Using secondary ratio");
                    useSecondaryBarRatio = true;
                }
            }
            // if Consequence is already taking up 1/3rd of screen
            else if (useSecondaryBarRatio && (Consequence.X >= secondaryBarRatio.X || Consequence.Y >= secondaryBarRatio.Y))
            {
                defaultFontSizeModifier -= .025f;
                returnValid = false;

                if ((cellSize * defaultFontSizeModifier) < minimumFontSize)
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

    /// <summary> 
    /// level of abstraction for accessing async function through signal
    /// </summary>
    private void OnSizeChanged()
    {
        UpdateMinSizeWithConsequence();
    }

    private bool IsConsequenceLargerThanBarRatio(Vector2 barRatio)
    {
        bool isLarger = false;
        if (Consequence.X > barRatio.X)
            isLarger = true;

        if (Consequence.Y > barRatio.Y)
            isLarger = true;

        return isLarger;

    }

    public static int GetLargetBarHintNotch(Array<RC_NotchHint> barHint)
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

    private float GetNotchHBarLength(int longestNotchSize,float fontSize, float fontDivide)
    {
        return (longestNotchSize * fontSize) + fontDivide;
    }

    private float GetNotchVBarLength(int notchSize, float  fontSize, float fontDivide)
    {
        // arbitrary magic number to push vBarLength further from side
        return (notchSize * fontSize * 1.1f) + fontDivide * 2;
    }
}
