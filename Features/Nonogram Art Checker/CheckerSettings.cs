using Features.NonogramGridCreation;
using Godot;
using System;

namespace Features.NonogramChecker;

public partial class CheckerSettings : Control
{
    [Export] Label LevelName { get; set; }
    [Export] Label GridSize { get; set; }

    [Export] GridContainer ColorGrid { get; set; }

    // turn color rects into PanelContainer scene with a button as a child.
    //      connect button signals to panel container
    // force buttons to be square by making min x and y same

    public override void _Ready()
    {
        if (NonogramPuzzleManager.Instance == null)
            return;

        foreach (Color color in NonogramPuzzleManager.Instance.UniqueColorList)
        {
            if (color == Colors.White)
                continue;

            ColorRect newRect = new();
            newRect.Color = color;
            newRect.CustomMinimumSize = new Vector2(0, 150);
            newRect.SizeFlagsHorizontal = SizeFlags.ExpandFill;

            ColorGrid.AddChild(newRect);
            newRect.Owner = GetTree().Root;
        }
        string thing = NonogramPuzzleManager.Instance.Level.ResourcePath;
        string[] newThing = thing.Split("/");
        LevelName.Text = $"Level Name: {newThing[^1].Replace(".png", "").Capitalize()}";

        GridSize.Text = $"Grid Size: {NonogramPuzzleManager.Instance.Level.GetSize()}";
    }
}
