using Features.NonogramGridCreation;
using Godot;
using System;

namespace Features.NonogramChecker;

public partial class CheckerSettings : Control
{
    [Export] PackedScene fileDialogScene { get; set; }

    [Export] Label LevelName { get; set; }
    [Export] Label GridSize { get; set; }

    [Export] GridContainer ColorGrid { get; set; }
    [Export] Button FileSelect { get; set; }

    private FdGetNonogram fileDialog;

    private string saveDirectory;

    //TODO: Create color picket button script that stores reference to own index pos

    public override void _Ready()
    {
        if (NonogramPuzzleManager.Instance == null)
            return;

        SetCheckerSettings();

        FileSelect.Pressed += FileSelect_Pressed;
    }

    private void FileSelect_Pressed()
    {
        fileDialog = fileDialogScene.Instantiate() as FdGetNonogram;

        fileDialog.fileDialog.FileSelected += FileDialog_FileSelected;
        fileDialog.fileDialog.CloseRequested += FileDialog_CloseRequested;
        fileDialog.fileDialog.DirSelected += FileDialog_DirSelected;

        fileDialog.saveDirectory = saveDirectory;

        GetTree().Root.AddChild(fileDialog);
        fileDialog.Owner = GetTree().Root;
    }

    private void FileDialog_DirSelected(string dir)
    {
        saveDirectory = dir;
    }

    private void FileDialog_CloseRequested()
    {
        fileDialog.QueueFree();
    }

    private void FileDialog_FileSelected(string path)
    {
        Image texture = GD.Load(path) as Image;
        if (texture == null)
            return;

        NonogramPuzzleManager.Instance.CreateNonogram(texture);
        ClearColorPickers();

        SetCheckerSettings();
    }

    private void SetCheckerSettings()
    {
        foreach (Color color in NonogramPuzzleManager.Instance.UniqueColorList)
        {
            if (color == Colors.White)
                continue;

            ColorPickerButton button = new();
            button.Color = color;
            button.CustomMinimumSize = new Vector2(0, 150);
            button.SizeFlagsHorizontal = SizeFlags.ExpandFill;

            ColorGrid.AddChild(button);
            button.Owner = GetTree().Root;
            button.ColorChanged += (Color newcolor) => ChangeColor(newcolor, button.GetIndex());
        }
        string thing = NonogramPuzzleManager.Instance.Level.ResourcePath;
        string[] newThing = thing.Split("/");
        LevelName.Text = $"Level Name: {newThing[^1].Replace(".png", "").Capitalize()}";

        GridSize.Text = $"Grid Size: {NonogramPuzzleManager.Instance.Level.GetSize()}";
    }

    private void ChangeColor(Color color, int index)
    {
        NonogramPuzzleManager.Instance.SetManualColorList(index + 1, color);
    }

    public void ClearColorPickers()
    {
        foreach (Node child in ColorGrid.GetChildren())
        {
            (child as ColorPickerButton).ColorChanged -= (Color newcolor) => ChangeColor(newcolor, child.GetIndex());
            child.QueueFree();
        }
    }

    public override void _EnterTree()
    {
    }

    public override void _ExitTree()
    {
        ClearColorPickers();
    }
}
