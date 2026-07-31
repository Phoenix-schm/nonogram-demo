using Godot;
using Godot.Collections;
using System;

namespace Features.NonogramGridCreation.BarGeneration;

[Tool]
public partial class HBarControl : BarControl
{
    protected override void DrawNotchBackgrounds(float viewportScale, int notches, float startPos)
    {
        Vector2[] barVectors = new Vector2[notches * 2];
        Color[] barColors = new Color[notches];

        float canvasPos = GridCreation.Instance.GetCanvasTransform().Origin.X;
        // start from half cellsize
        canvasPos += (cellSize / 2) * viewportScale;

        // the font size as a float for far smoother transitioning
        float pseudoFontSize = (cellSize * fontSizeModifier) * viewportScale;
        float fontDivide = pseudoFontSize / 4;

        int index = 0;
        int colorIndex = 0;
        for (int i = 0; i < notches; i++)
        {
            // Get number list based on current notch
            string[] splitString = CreateStringArrayFromNotchHint(i);

            float calcNotchHeight = startPos - ((splitString.Length) * pseudoFontSize) - fontDivide; // how long/tall notch is

            barVectors[index++] = new Vector2(canvasPos, calcNotchHeight);
            barVectors[index++] = new Vector2(canvasPos, startPos);

            if (i % 2 == 0)
                barColors[colorIndex++] = NotchColor;
            else
                barColors[colorIndex++] = AltNotchColor;

            canvasPos += cellSize * viewportScale;  // increase canvasPos
        }

        DrawMultilineColors(barVectors, barColors, notchThickness);
    }

    protected override void DrawNotchNumbers(float interval, float _ratio, int steps, float startPos)
    {
        // initial starting pos begins at grid origin
        float canvasPos = GridCreation.Instance.GetCanvasTransform().Origin.X;
        // Push canvas pos over half the size of cell size and multiplied by ratio for accurate scale
        canvasPos += (interval / 2) * _ratio;

        // the font size as a float for smoother transitioning
        // font initialization
        float pseudoFontSize = (cellSize * fontSizeModifier) * _ratio;
        fontSize = Mathf.RoundToInt(cellSize);

        // how far numbers start from
        float fontDivide = pseudoFontSize / 4;    // arbitrary magic number. used for shifting numbers from start
        float blockDivide = pseudoFontSize / 10;  // arbitrary magic number. used for shifting between numbers

        for (int i = 0; i < steps; i++)
        {
            string[] numString = CreateStringArrayFromNotchHint(i);

            float stringMargin = startPos - fontDivide;
            float blockMargin = startPos - blockDivide;
            // list numbers in column in reverse order
            for (int curNumber = numString.Length - 1; curNumber >= 0; curNumber--)
            {
                // Get the number from string aray
                string newString = numString[curNumber];
                Color fontColor = MainFontColor;

                FontVariation fontChoice = (FontVariation)GetThemeDefaultFont();
                float _fontSizeModifier = fontSizeModifier;

                // if newString is double digit
                if (newString.Length > 1)
                {
                    fontChoice = AltFont;
                    _fontSizeModifier *= .9f;

                    // change font color for better readability
                    if ((i + curNumber) % 2 == 0)
                        fontColor = AltFontColor1;
                    else
                        fontColor = AltFontColor2;
                }

                // if using a colored grid
                if (NonogramPuzzleManager.Instance.isColorful)
                {
                    Color blockBGColor;
                    if (newString == "0")
                    {
                        blockBGColor = i % 2 == 0 ? NotchColor : AltNotchColor;
                        blockBGColor.A = 0;
                    }
                    else
                        blockBGColor = barHint[i].colorList[curNumber];

                    // modify font color to contrast with bg color
                    fontColor = CheckLuminence(blockBGColor);

                    
                    DrawLine(
                        new Vector2(canvasPos, blockMargin),
                        new Vector2(canvasPos, blockMargin - pseudoFontSize),
                        blockBGColor,
                        notchThickness * .9f
                        );
                }

                int newFontSize = Mathf.RoundToInt((fontSize * _fontSizeModifier));

                if (newFontSize <= 0)
                    newFontSize = 1;

                // Calculation for moving fonts across bar
                // Extra calculation at end for pushing digits to center
                float textStartH = canvasPos - cellSize / 2 * _ratio;

                DrawString(fontChoice, new Vector2(textStartH, stringMargin),
                    newString, HorizontalAlignment.Center,
                    cellSize * _ratio, newFontSize,
                    fontColor
                    );

                // Update margin to push numbers up
                stringMargin -= pseudoFontSize;
                blockMargin -= pseudoFontSize;
            }

            // add onto initial position
            canvasPos += interval * _ratio;
        }
    }

    protected override Array<RC_NotchHint> GetNotchHints()
    {
        return NonogramPuzzleManager.Instance.h_barHint;
    }

    protected override int GetNotchCount()
    {
        return NonogramPuzzleManager.CellCount.X;
    }

    protected override float GetStartingPos(Transform2D viewport)
    {
        float yStartPos = viewport.Origin.Y + this.Size.Y;
        if (yStartPos <= Size.Y)
            // Clamp so that it can't go beyond size of bar
            yStartPos = Size.Y;

        return yStartPos;
    }

    protected override void DrawWholeBarBackground(float startPos)
    {
        // draw background of entire bar
        DrawRect(
            new Rect2(Vector2.Zero, new Vector2(GridCreation.Instance.cellCount.X * GridCreation.Instance.cellSize, startPos)),
            BGColor
            );
    }
}
