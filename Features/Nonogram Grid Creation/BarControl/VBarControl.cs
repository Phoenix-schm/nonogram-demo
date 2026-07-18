using Godot;
using Godot.Collections;
using System;

namespace Features.NonogramGridCreation.BarGeneration;

[Tool]
public partial class VBarControl : BarControl
{
    protected override void DrawBarBackgrounds(float interval, float _ratio, int steps, float startPos)
    {
        Vector2[] barVectors = new Vector2[steps * 2];
        Color[] barColors = new Color[steps];

        float canvasPos = GridCreation.Instance.GetCanvasTransform().Origin.Y;
        canvasPos += (interval / 2) * _ratio;

        // the font size as a float for far smoother transitioning
        float pseudoFontSize = (cellSize * fontSizeModifier) * 1.1f * _ratio;

        // draw background of entire bar
        DrawRect(
            new Rect2(Vector2.Zero, new Vector2(startPos, GridCreation.Instance.cellCount.Y * GridCreation.Instance.cellSize)),
            Colors.SeaGreen
            );

        int index = 0;
        int colorIndex = 0;
        for (int i = 0; i < steps; i++)
        {
            // Get number list based on current notch
            string[] splitString = CreateStringArrayFromBarHint(i);

            calcNotchHeight = startPos - ((splitString.Length) * pseudoFontSize);

            barVectors[index++] = new Vector2(startPos, canvasPos);
            barVectors[index++] = new Vector2(calcNotchHeight, canvasPos);

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
        float canvasPos = GridCreation.Instance.GetCanvasTransform().Origin.Y;
        canvasPos += (interval / 2) * _ratio;

        float pseudoFontSize = cellSize * fontSizeModifier * _ratio;
        fontSize = Mathf.RoundToInt(cellSize);

        float blockDivide = pseudoFontSize / 5; // for chifting color blocks left

        for (int i = 0;  i < steps; i++)
        {
            string[] splitString = CreateStringArrayFromBarHint(i);

            // how far from right numbers start from
            float stringMargin = startPos;
            float blockMargin = startPos - blockDivide;

            // list numbers in row in reverse order
            for (int curNumber = splitString.Length - 1; curNumber >= 0; curNumber--)
            {
                // Get the number from string aray
                string newString = splitString[curNumber];
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

                float stringWidth = cellSize * 1.1f * fontSizeModifier;

                // calculation for moving fonts across bar
                // extra calculation at end for pushin double digits
                float textStartV = canvasPos + cellSize / 3.5f * _ratio;

                DrawString(fontChoice, new Vector2(stringMargin - stringWidth, textStartV),
                    newString, HorizontalAlignment.Center,
                    stringWidth, Mathf.RoundToInt(fontSize * _fontSizeModifier), fontColor);

                // update margin to push next numbers
                // extra length added to pseudoFontSize due to changes in width size
                stringMargin -= (pseudoFontSize * 1.1f) * _ratio;
                blockMargin -= pseudoFontSize * 1.1f * _ratio;
            }

            // add onto initial position
            canvasPos += interval * _ratio;
        }
    }

    protected override Array<R_BarHint> GetBarHints()
    {
        return NonogramPuzzleManager.Instance.v_hints;
    }

    protected override int GetNotchCount()
    {
        return NonogramPuzzleManager.CellCount.Y;
    }

    protected override float GetStartingPos(Transform2D viewport)
    {
        float xStartPos = viewport.Origin.X + Size.X;
        if (xStartPos <= Size.X)
            //Clamp so that it doesn't go beyond size of bar
            xStartPos = Size.X;

        return xStartPos;
    }
}
