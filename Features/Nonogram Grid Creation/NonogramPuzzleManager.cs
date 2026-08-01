using Common;
using Features.NonogramGridCreation.BarGeneration;
using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Features.NonogramGridCreation;

[Tool]
public partial class NonogramPuzzleManager : PanelContainer
{
    public static event Action OnManualColorDictChanged;

    public static NonogramPuzzleManager Instance { get; private set; }

    [ExportCategory("Grid Generation")]
    [Export] public Image Level { get; set; }

    public static Vector2I CellCount { get; private set; }

    // *** Hint Generation ***
    public Array<RC_NotchHint> h_barHint = new();
    public Array<RC_NotchHint> v_barHint = new();

    // Black and White are the two colors in a monochrome grid
    // more than that and it's a color grid
    public bool IsColorful { get { return UniqueColorList.Count > 2; } }

    public List<Color> UniqueColorList { get; private set; } = [Colors.White];   // Default 0 index to White

    public Godot.Collections.Dictionary<int, Color> ManualColorDict = new();

    public Array<int> FullColorGrid { get; private set; } = new();

    public override void _Ready()
    {
        CreateNonogram(Level);
    }

    public void CreateNonogram(Image levelImage)
    {
        Level = levelImage;

        if (Level == null)
        {
            GameLogger.Error("Level image not initialized");
            return;
        }

        CellCount = Level.GetSize();

        ManualColorDict.Clear();
        UniqueColorList = [Colors.White];
        FullColorGrid = new();

        h_barHint = new();
        v_barHint = new();

        // create hints for h and v bars
        for (int x = 0; x < CellCount.X; x++)
            h_barHint.Add(GetNotchHint(x, CellCount.Y, false));
        for (int y = 0; y < CellCount.Y; y++)
        {
            v_barHint.Add(GetNotchHint(y, CellCount.X, true));
            CreateGridFromColorList(y, CellCount.X, true);
        }

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
            if ((curColor == Colors.White || curColor.A < 1) && curAmount > 0)
            {
                notchHint.numberList.Add(curAmount);
                curAmount = 0;
                CheckAddColor(ref notchHint, prevColor);
                continue;
            }
            else if (curColor == Colors.White || curColor.A < 1) // move on
                continue;

            // if there's a color swap, and we have a count going
            if (prevColor != curColor && curAmount > 0)
            {
                notchHint.numberList.Add(curAmount);
                curAmount = 0;
                CheckAddColor(ref notchHint, prevColor);
            }

            // Add to count when have the same color
            if ((curAmount > 0 && curColor == prevColor) || curAmount == 0)
                curAmount += 1;
            else
            {
                notchHint.numberList.Add(curAmount);
                curAmount = 0;
                CheckAddColor(ref notchHint, curColor);
            }
            prevColor = curColor;
        }

        // last check in case of notch being filled with color all the way to the end
        if (curAmount > 0)
        {
            notchHint.numberList.Add(curAmount);
            CheckAddColor(ref notchHint, curColor);
        }

        return notchHint;
    }

    /// <summary>
    /// Adds to the unique color list if the color is a new one, and adds it's specific index to notchHint
    /// </summary>
    /// <param name="notchHint"></param>
    /// <param name="checkColor"></param>
    private void CheckAddColor(ref RC_NotchHint notchHint, Color checkColor)
    {
        if (!UniqueColorList.Contains(checkColor)) // if this color wasn't added before
            UniqueColorList.Add(checkColor);

        int index = UniqueColorList.IndexOf(checkColor);
        //ManualColorDict.Add(index, checkColor);
        ManualColorDict[index] = checkColor;

        notchHint.colorList.Add(index);
    }

    /// <summary>
    /// Creates a version of the grid made up of only numbers associated their respective uniqueColorList index
    /// </summary>
    /// <param name="notch"></param>
    /// <param name="altNotch"></param>
    /// <param name="isVertical"></param>
    private void CreateGridFromColorList(int notch, int altNotch, bool isVertical)
    {
        for (int cell = 0; cell < altNotch;  cell++)
        {
            Vector2I index = isVertical ? new(cell, notch) : new(notch, cell);
            Color curColor = Level.GetPixelv(index);

            int colorIndex = 0; // defaul to white at zero index
            if (UniqueColorList.Contains(curColor))
                colorIndex = UniqueColorList.IndexOf(curColor);

            FullColorGrid.Add(colorIndex);
        }
    }

    public Color GetColorFromList(int colorIndex)
    {
        return ManualColorDict[colorIndex];
    }

    public void ResetManualColorList()
    {
        foreach (int index in ManualColorDict.Keys)
        {
            ManualColorDict[index] = UniqueColorList[index];
        }
    }

    public void SetManualColorList(int index, Color newColor)
    {
        ManualColorDict[index] = newColor;
        OnManualColorDictChanged?.Invoke();
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

        Instance = this;
    }
}
