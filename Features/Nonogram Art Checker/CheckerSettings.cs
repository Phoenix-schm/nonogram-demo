using Features.NonogramGridCreation;
using Godot;
using System;

namespace Features.NonogramChecker;

public partial class CheckerSettings : Control
{
    public static Action ChangedImage;

    [Export] PackedScene fileDialogScene { get; set; }

    [Export] Label LevelName { get; set; }
    [Export] Label GridSize { get; set; }

    [Export] GridContainer ColorGrid { get; set; }
    [Export] Button FileSelect { get; set; }

    [ExportCategory("Rules")]
    [Export] Button RuleButton { get; set; }
    [Export] Button ExitRulesButton { get; set; }
    [Export] Container RulesContainer { get; set; }
    [Export] CanvasLayer RulesLayer { get; set; }

    [ExportCategory("Misc")]
    [Export] Button SaveButton { get; set; }
    [Export] Button ResetButton { get; set; }

    private FdGetNonogram fileDialog;

    private string saveDirectory = string.Empty;

    public override void _Ready()
    {
        if (NonogramPuzzleManager.Instance == null)
            return;

        SetCheckerSettings();

        RulesLayer.Visible = false;

        FileSelect.Pressed += OnFileSelectOpened;
        ResetButton.Pressed += ResetToDefaultColors;

        // Rules initialization
        RuleButton.Pressed += UpdateRulesVisibility;
        ExitRulesButton.Pressed += UpdateRulesVisibility;
        RulesContainer.GuiInput += ClickRulesBG;
    }

    #region RulesLogic
    private void ClickRulesBG(InputEvent @event)
    {
        if (@event is not InputEventMouseButton mouseButton)
            return;

        UpdateRulesVisibility();
    }

    private void UpdateRulesVisibility()
    {
        RulesLayer.Visible = !RulesLayer.Visible;
    }
    #endregion

    private void OnFileSelectOpened()
    {
        fileDialog = fileDialogScene.Instantiate() as FdGetNonogram;
        
        // initialize filedialog
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
        ChangedImage?.Invoke();

        SetCheckerSettings();
    }

    private void SetCheckerSettings()
    {
        ClearColorPickers();
        foreach (Color color in NonogramPuzzleManager.Instance.UniqueColorList)
        {
            if (color == Colors.White)
                continue;

            CheckerPickerButton button = new();
            button.Color = color;
            button.CustomMinimumSize = new Vector2(0, 150);
            button.SizeFlagsHorizontal = SizeFlags.ExpandFill;

            ColorGrid.AddChild(button);
            button.Owner = GetTree().Root;
        }

        string imagePath = NonogramPuzzleManager.Instance.Level.ResourcePath;
        string[] pathArray = imagePath.Split("/");
        LevelName.Text = $"Level Name: {pathArray[^1].Replace(".png", "").Capitalize()}";

        GridSize.Text = $"Grid Size: {NonogramPuzzleManager.Instance.Level.GetSize()}";
    }

    public void ClearColorPickers()
    {
        foreach (Node child in ColorGrid.GetChildren())
            child.QueueFree();
    }

    public void ResetToDefaultColors()
    {
        foreach (Node child in ColorGrid.GetChildren())
        {
            CheckerPickerButton button = child as CheckerPickerButton;

            // Offset by one because 0 is White
            button.OnColorChanged(NonogramPuzzleManager.Instance.GetDefaultColorFromList(button.GetIndex() + 1));
        }
    }
}
