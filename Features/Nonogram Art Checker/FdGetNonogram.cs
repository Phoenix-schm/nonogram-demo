using Godot;
using System;

public partial class FdGetNonogram : Node
{
    [Export] public FileDialog fileDialog { get; set; }

    public string saveDirectory = String.Empty;

    public override void _Ready()
    {
        if (saveDirectory == String.Empty)
            fileDialog.CurrentDir = ProjectSettings.GlobalizePath("user://");
        else
            fileDialog.CurrentDir = saveDirectory;
    }
}
