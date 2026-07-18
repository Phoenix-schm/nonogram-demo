using Godot;
using Godot.Collections;
using System;

namespace Features.NonogramGridCreation.BarGeneration;

/// <summary>
/// Responsible for storing information of the bar hints.
/// Such as the numbers, colors, and how much of the bar has been completed
/// </summary>
[Tool]
public partial class R_BarHint : RefCounted
{
    public Array<int> numberList;           // list of numbers in a row/column
    public Array<Color> colorList;          // list of colors coinciding with the numbersList
    public Array<int> correctNumbersList;   // a list of 0's and 1's representing if a player has filled out a row/column correctly
                                            //      0 indicates "incorrect". 1 indicates "correct"
                                            // NOTE: Should not be literal correctness. Just whether numbers align with filled cells
}
