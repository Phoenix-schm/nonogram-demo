using Common;
using Godot;
using Godot.Collections;
using System;

namespace Features.NonogramGridCreation;

[Tool]
public partial class GridCreation : Container
{
    public static GridCreation Instance { get; set; }

    public static event Action OnGridFinishedInitializing;

    [ExportCategory("Line Genration")]
    [Export] public int DividerCount { get; set; }
    [Export] public Color MainLineColor { get; set; } = Colors.Black;
    [Export] public Color DividerLineColor { get; set; } = Colors.Black;


    public Vector2I cellCount;
    public Vector2 gridSize;
    public float cellSize;     // uniform square size

    public bool isInitialized;

    private float mainLineWidth;
    private float dividerLineWidth;

    [ExportToolButton("Redraw Grid")]
    public Callable RedrawButton => Callable.From(QueueRedraw);

    public override void _Draw()
    {
        if (NonogramPuzzleManager.Instance == null)
            return;
        InitializeGrid();

        mainLineWidth = cellSize * .09f;
        dividerLineWidth = cellSize * .185f;

        DrawGrid();

        OnGridFinishedInitializing?.Invoke();
        // TODO: Send signal to bars when finished initializing
    }

    private void InitializeGrid()
    {
        gridSize = GetViewportRect().Size;
        cellSize = CalculateCellSize(gridSize);
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
        DrawMainGridLines(MainLineColor, mainLineWidth);
        if (DividerCount <= 0)
        {
            // Less than zero safety check
            GameLogger.Error("Dividier Count less than 0.");
            return;
        }
        DrawDividerLines(DividerLineColor, dividerLineWidth);
    }

    private void DrawMainGridLines(Color lineColor, float lineWidth)
    {
        Vector2[] mainsPos = new Vector2[cellCount.X * 2];

        int iterator = 0;
        for (int x = 0; x < cellCount.X;)
        {
            mainsPos[iterator++] = new Vector2(x * cellSize, 0);
            mainsPos[iterator++] = new Vector2(x * cellSize, cellCount.Y * cellSize);
            x++;
        }

        DrawMultiline(mainsPos, lineColor, lineWidth);

        // Rotate draw
        DrawSetTransform(Vector2.Zero, float.Pi / 2, Vector2.One);

        Vector2[] dividersPos = new Vector2[cellCount.Y * 2];

        iterator = 0;
        for (int y = 0; y < cellCount.Y;)
        {
            dividersPos[iterator++] = new Vector2(y * cellSize, 0);
            dividersPos[iterator++] = new Vector2(y * cellSize, -cellCount.X * cellSize);
            y++;
        }

        DrawMultiline(dividersPos, lineColor, lineWidth);
        // reset rotation
        DrawSetTransform(Vector2.Zero, 0, Vector2.One);
    }

    /// <summary>
    /// Draws divider lines in iterations of DividerCount. 
    /// Must be separate function so that lines appear on top of main lines
    /// </summary>
    /// <param name="lineColor"></param>
    /// <param name="lineWidth"></param>
    private void DrawDividerLines(Color lineColor, float lineWidth)
    {
        int dividerAmount = Mathf.FloorToInt(cellCount.X / DividerCount);
        // offset divider amount to adjust for showing lines from one end to the other
        Vector2[] mainsPos = new Vector2[(dividerAmount + 1) * 2];

        int iterator = 0;
        for (int x = 0; x < cellCount.X + 1;)
        {
            if (x == cellCount.X || (x % DividerCount) == 0)
            {
                mainsPos[iterator++] = new Vector2(x * cellSize, 0);
                mainsPos[iterator++] = new Vector2(x * cellSize, cellCount.Y * cellSize);
            }
            x++;
        }

        DrawMultiline(mainsPos, lineColor, lineWidth);

        // Rotate draw
        DrawSetTransform(Vector2.Zero, float.Pi / 2, Vector2.One);

        dividerAmount = Mathf.FloorToInt(cellCount.Y / DividerCount);
        Vector2[] dividersPos = new Vector2[(dividerAmount + 1) * 2];

        iterator = 0;
        for (int y = 0; y < cellCount.Y + 1;)
        {
            if (y == cellCount.Y || (y % DividerCount) == 0)
            {
                dividersPos[iterator++] = new Vector2(y * cellSize, 0);
                dividersPos[iterator++] = new Vector2(y * cellSize, -cellCount.X * cellSize);
            }
            y++;
        }

        DrawMultiline(dividersPos, lineColor, lineWidth);
        // reset rotation
        DrawSetTransform(Vector2.Zero, 0, Vector2.One);
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
