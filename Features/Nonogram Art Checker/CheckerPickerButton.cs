using Features.NonogramGridCreation;
using Godot;
using System;

namespace Features.NonogramChecker;

public partial class CheckerPickerButton : ColorPickerButton
{
    public override void _EnterTree()
    {
        ColorChanged += OnColorChanged;
    }

    public void OnColorChanged(Color color)
    {
        // Offset by one because 0 is White color
        NonogramPuzzleManager.Instance.SetManualColorList(GetIndex() + 1, color);
    }

    public override void _ExitTree()
    {
        ColorChanged -= OnColorChanged;
    }

}
