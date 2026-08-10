using Godot;
using Godot.Collections;
using System;

namespace Features.NonogramGridCreation.BarGeneration;

[Tool]
public partial class VBarControl : BarControl
{
    protected override void DrawNotchBackgrounds(float _ratio, int steps, float startPos)
    {
        Vector2[] barVectors = new Vector2[steps * 2];
        Color[] barColors = new Color[steps];

        float canvasPos = GridCreation.Instance.GetCanvasTransform().Origin.Y;

        canvasPos += (cellSize / 2) * _ratio;

        // the font size as a float for far smoother transitioning
        float pseudoFontSize = (cellSize * fontSizeModifier) * _ratio;
        float font_divide = pseudoFontSize * .1f;   // the slight offset from the edge of the last number

        int index = 0;
        int colorIndex = 0;
        for (int i = 0; i < steps; i++)
        {
            // Get length of numbers in notch
            float calcNotchHeight = startPos - (CreateStringArrayFromNotchHint(i).Length * pseudoFontSize) - font_divide * 2;

            barVectors[index++] = new Vector2(startPos, canvasPos);
            barVectors[index++] = new Vector2(calcNotchHeight, canvasPos);

            if (i % 2 == 0)
                barColors[colorIndex++] = NotchColor;
            else
                barColors[colorIndex++] = AltNotchColor;

            canvasPos += cellSize * _ratio;
        }

        DrawMultilineColors(barVectors, barColors, notchThickness);
    }

    protected override void DrawNotchNumbers(float interval, float _ratio, int steps, float startPos)
    {
        float canvasPos = GridCreation.Instance.GetCanvasTransform().Origin.Y; // starting pos going down
        canvasPos += (interval / 2) * _ratio;

        float pseudoFontSize = cellSize * fontSizeModifier * _ratio;
        fontSize = Mathf.RoundToInt(pseudoFontSize);

        float blockDivide = pseudoFontSize * .1f; // for shifting color blocks left
        for (int notchIndex = 0;  notchIndex < steps; notchIndex++)
        {
            string[] stringArray = CreateStringArrayFromNotchHint(notchIndex);
            
            // how far from right numbers start from
            float stringMargin = startPos - blockDivide;
            // the amount between color blocks
            float blockMargin = startPos - blockDivide;

            // list numbers in row in reverse order
            for (int curNumber = stringArray.Length - 1; curNumber >= 0; curNumber--)
            {
                // Get the number from string aray
                string newString = stringArray[curNumber];
                Color fontColor = MainFontColor;

                FontVariation fontChoice = (FontVariation)GetThemeDefaultFont();
                float _fontSizeModifier = fontSizeModifier;

                // if newString is double digit
                if (newString.Length > 1)
                {
                    fontChoice = AltFont;
                    _fontSizeModifier *= .95f;

                    if ((notchIndex + curNumber) % 2 == 0)
                        fontColor = AltFontColor1;
                    else
                        fontColor = AltFontColor2;
                }

                if (NonogramPuzzleManager.Instance.IsColorful)
                {
                    Color blockBGColor;
                    if (newString == "0")
                    {
                        blockBGColor = notchIndex % 2 == 0 ? NotchColor : AltNotchColor;
                        blockBGColor.A = 0;
                    }
                    else
                        blockBGColor = NonogramPuzzleManager.Instance.GetColorFromList(barHint[notchIndex].colorList[curNumber]);

                    // modify font color to contrast with bg color
                    fontColor = (blockBGColor.Luminance >= .5f) ? Colors.Black : Colors.White;

                    DrawLine(
                        new Vector2(blockMargin, canvasPos),    // start of line (right side)
                        new Vector2(blockMargin - pseudoFontSize, canvasPos), // end of line (left side)
                        blockBGColor,
                        notchThickness * .9f
                        );
                }

                int newFontSize = Mathf.RoundToInt((fontSize * _fontSizeModifier));
                
                if (newFontSize <= 0)
                    newFontSize = 1;

                // calculation for moving fonts downwards
                float textStartV = canvasPos + cellSize / 3.5f * _ratio;
                // width for centering text
                float stringWidth = cellSize * _fontSizeModifier;

                // Draw current number
                DrawString(fontChoice, new Vector2(stringMargin - stringWidth, textStartV),
                    newString, HorizontalAlignment.Center,
                    stringWidth, newFontSize, fontColor);

                // update margin to push next numbers
                // extra length added to pseudoFontSize due to changes in width size
                stringMargin -= pseudoFontSize * _ratio;
                blockMargin -= pseudoFontSize * _ratio;
            }

            // add onto initial position
            canvasPos += interval * _ratio;
        }
    }

    protected override Array<RC_NotchHint> GetNotchHints()
    {
        return NonogramPuzzleManager.Instance.v_barHint;
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

    protected override void DrawWholeBarBackground(float startPos)
    {
        // draw background of entire bar. This is to prevent 
        DrawRect(
            new Rect2(Vector2.Zero, new Vector2(startPos, GridCreation.Instance.cellCount.Y * cellSize)),
            BGColor
            );
    }
}
