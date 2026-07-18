using Godot;
using Godot.Collections;
using System;

namespace Features.NonogramGridCreation.BarGeneration;

[Tool]
public partial class HBarControl : BarControl
{
    protected override void DrawBarBackgrounds(float interval, float _ratio, int steps, float startPos)
    {
        Vector2[] barVectors = new Vector2[steps * 2];
        Color[] barColors = new Color[steps];

        float canvasPos = GridCreation.Instance.GetCanvasTransform().Origin.X;
        canvasPos += (interval / 2) * _ratio;

        // the font size as a float for far smoother transitioning
        float pseudoFontSize = (cellSize * fontSizeModifier) * _ratio;
        float fontDivide = pseudoFontSize / 4;

        // draw background of entire bar
        DrawRect(
            new Rect2(Vector2.Zero, new Vector2(Size.X, startPos)),
            Colors.SeaGreen
            );

        int index = 0;
        int colorIndex = 0;
        for (int i = 0; i < steps; i++)
        {
            // Get number list based on current notch
            string[] splitString = CreateStringArrayFromBarHint(i);

            calcNotchHeight = startPos - ((splitString.Length - 1) * pseudoFontSize) - fontDivide;

            barVectors[index++] = new Vector2(canvasPos, calcNotchHeight);
            barVectors[index++] = new Vector2(canvasPos, startPos);

            if (i % 2 == 0)
                barColors[colorIndex++] = NotchColor;
            else
                barColors[colorIndex++] = AltNotchColor;

            canvasPos += interval * _ratio;
        }

        DrawMultilineColors(barVectors, barColors, notchThickness);
    }

    protected override void DrawBarNumbers(float interval, float _ratio, int steps, float startPos)
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
        float blockDivide = pseudoFontSize / 6;  // arbitrary magic number. used for shifting between numbers

        for (int i = 0; i < steps; i++)
        {
            string[] numString = CreateStringArrayFromBarHint(i);

            float stringMargin = startPos - fontDivide;
            float blockMargin = startPos - blockDivide;
            // list numbers in column in reverse order
            for (int curNumber = numString.Length - 1; curNumber > 0; curNumber--)
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

                    if ((i + curNumber) % 2 == 0)
                        fontColor = AltFontColor1;
                    else
                        fontColor = AltFontColor2;
                }
                // Calculation for moving fonts across bar
                // Extra calculation at end for pushing digits to center
                float textStartH = canvasPos - cellSize / 2 * _ratio;

                // if using a colored grid
                if (NonogramPuzzleManager.Instance.isColorful)
                {
                    Color bgColor;
                    if (newString == "0")
                    {
                        bgColor = i % 2 == 0 ? NotchColor : AltNotchColor;
                        bgColor.A = 0;
                    }
                    else
                        bgColor = barHints[i].colorList[curNumber];

                    // modify font color to contrast with bg color
                    fontColor = CheckLuminence(bgColor);

                    DrawLine(
                        new Vector2(canvasPos, blockMargin),
                        new Vector2(canvasPos, blockMargin - pseudoFontSize),
                        bgColor,
                        notchThickness * .85f
                        );
                }

                DrawString(fontChoice, new Vector2(textStartH, stringMargin),
                    newString, HorizontalAlignment.Center,
                    cellSize * _ratio, Mathf.RoundToInt(fontSize * _fontSizeModifier),
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

    protected override float GetStartingPos(Transform2D viewport)
    {
        float yStartPos = viewport.Origin.Y + this.Size.Y;
        if (yStartPos <= Size.Y)
            // Clamp so that it can't go beyond size of bar
            yStartPos = Size.Y;

        return yStartPos;
    }
}
