using Godot;
using System;

namespace Features.NonogramChecker;

public partial class CheckerSettings : PanelContainer
{
    [Export] Label LevelName { get; set; }
    [Export] Label GridSize { get; set; }
}
