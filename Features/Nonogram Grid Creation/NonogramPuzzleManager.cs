using Common;
using Godot;
using System;

namespace Features.NonogramGridCreation;

[Tool]
public partial class NonogramPuzzleManager : PanelContainer
{
    public static NonogramPuzzleManager Instance;

    [ExportCategory("Grid Generation")]
    [Export] Image Level { get; set; }
    [Export] bool IsColorful { get; set; }

    public Vector2I CellCount;

    public override void _Ready()
    {
        if (Level == null)
        {
            GameLogger.Error("Level image not initialized");
            return;
        }

        CellCount = Level.GetSize();
        GridCreation.Instance.cellCount = CellCount;
        GridCreation.Instance.QueueRedraw();
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
