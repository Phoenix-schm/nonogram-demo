using Common;
using Godot;
using Godot.Collections;
using System;

namespace Features.NonogramGridCreation;

[Tool]
public partial class GridCreation : Container
{
    public static GridCreation Instance { get; private set; }

    public static event Action OnGridFinishedInitializing;

    [ExportCategory("Line Genration")]
    [Export] public int DividerCount { get; set; }
    [Export] public Color MainLineColor { get; set; } = Colors.Black;
    [Export] public Color DividerLineColor { get; set; } = Colors.Black;

    [Export(PropertyHint.Range, "0.01, 1, 0.01")]
    public float MainLineWidthMult { get; set; } = .09f;
    [Export(PropertyHint.Range, "0.01, 1, 0.01")]
    public float DividerLineWidthMult { get; set; } = .185f;


    public Vector2I cellCount;
    public Vector2 gridSize;
    public float cellSize;     // uniform square size

    public float mainLineWidth;
    public float dividerLineWidth;

    [ExportToolButton("Redraw Grid")]
    public Callable RedrawButton => Callable.From(QueueRedraw);

    public override void _Draw()
    {
        if (NonogramPuzzleManager.Instance == null)
            return;

        cellCount = NonogramPuzzleManager.CellCount;

        InitializeGrid();

        mainLineWidth = cellSize * MainLineWidthMult;
        dividerLineWidth = cellSize * DividerLineWidthMult;

        DrawGrid();

        OnGridFinishedInitializing?.Invoke();
        // TODO: Send signal to bars when finished initializing
    }

    private void InitializeGrid()
    {
        gridSize = GetViewportRect().Size;
        cellSize = CalculateCellSize(gridSize);
        //GameLogger.Debug($"Grid Size: {gridSize}");
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

    private void DrawGrid()
    {
        DrawMainGridLines();

        if (DividerCount <= 0)
        {
            // Less than zero safety check
            GameLogger.Error("Dividier Count less than 0.");
            return;
        }
        DrawDividerLines();
    }

    private void DrawMainGridLines()
    {
        Vector2[] vertLines = new Vector2[(cellCount.X) * 2]; // line positions

        int iterator = 0;
        for (int x = 0; x < cellCount.X;)
        {
            vertLines[iterator++] = new Vector2((x * cellSize), 0);  // Start of line
            vertLines[iterator++] = new Vector2((x * cellSize), cellCount.Y * cellSize); // End of line
            x++;
        }

        DrawMultiline(vertLines, MainLineColor, mainLineWidth);

        // Rotate draw for horizontal lines
        DrawSetTransform(Vector2.Zero, float.Pi / 2, Vector2.One);

        Vector2[] horizLines = new Vector2[cellCount.Y * 2];

        iterator = 0;
        for (int y = 0; y < cellCount.Y;)
        {
            horizLines[iterator++] = new Vector2((y * cellSize), 0);       // Start of line
            horizLines[iterator++] = new Vector2((y * cellSize), -cellCount.X * cellSize); // End of line
            y++;    
        }

        DrawMultiline(horizLines, MainLineColor, mainLineWidth);
        // reset rotation
        DrawSetTransform(Vector2.Zero, 0, Vector2.One);
    }

    /// <summary>
    /// Draws divider lines in iterations of DividerCount. 
    /// Must be separate function so that lines appear on top of main lines
    private void DrawDividerLines()
    {
        int dividerAmount = Mathf.FloorToInt(cellCount.X / DividerCount);
        // offset divider amount to adjust for showing lines from one end to the other
        Vector2[] mainsPos = new Vector2[(dividerAmount + 1) * 2];

        int iterator = 0;
        for (int x = 0; x < cellCount.X;)
        {
            if ((x % DividerCount) == 0)
            {
                mainsPos[iterator++] = new Vector2((x * cellSize), 0);    // Start
                mainsPos[iterator++] = new Vector2((x * cellSize), cellCount.Y * cellSize);  // end
            }
            x++;
        }

        DrawMultiline(mainsPos, DividerLineColor, dividerLineWidth);

        // Additional line due to strange behavior of using just lineWidth.
        // last x line is somehow showing full line width instead of half like the rest of the last lines.
        DrawLine(
            new Vector2(cellCount.X * cellSize, 0), 
            new Vector2(cellCount.X *cellSize, cellCount.Y * cellSize),
            DividerLineColor, dividerLineWidth / 2);

        // Rotate draw
        DrawSetTransform(Vector2.Zero, float.Pi / 2, Vector2.One);

        dividerAmount = Mathf.FloorToInt(cellCount.Y / DividerCount);
        Vector2[] dividersPos = new Vector2[(dividerAmount + 1) * 2]; // offset by one for the additional line at the end of the grid

        iterator = 0;
        for (int y = 0; y < cellCount.Y + 1;)
        {
            if (y == cellCount.Y || (y % DividerCount) == 0)
            {
                dividersPos[iterator++] = new Vector2((y * cellSize), 0);     // start
                dividersPos[iterator++] = new Vector2((y * cellSize), -cellCount.X * cellSize);  // end
            }
            y++;
        }

        DrawMultiline(dividersPos, DividerLineColor, dividerLineWidth);
        // reset rotation
        DrawSetTransform(Vector2.Zero, 0, Vector2.One);
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

        BarSizeControl.OnBarControlInitialized += QueueRedraw;

        Instance = this;
    }

    public override void _ExitTree()
    {
        BarSizeControl.OnBarControlInitialized -= QueueRedraw;
    }
}
