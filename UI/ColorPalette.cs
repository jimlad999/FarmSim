using Microsoft.Xna.Framework;

namespace UI;
public static class ColorPalette
{
#pragma warning disable CA2211 // Non-constant fields should not be visible
    public static Color Black = Color.Black;
    public static Color Gray = new Color(170, 170, 170, 255);
    public static Color DarkGray = new Color(120, 120, 120, 255);
    public static Color White = Color.White;
    public static Color Red = Color.Red;
    public static Color Green = Color.Green;
    public static Color Blue = Color.Blue;
#pragma warning restore CA2211 // Non-constant fields should not be visible

    public static Color Parse(string color)
    {
        switch (color)
        {
            case "black":
                return Black;
            case "white":
                return White;
            case "gray":
                return Gray;
            case "darkgray":
                return DarkGray;
            case "red":
                return Red;
            case "green":
                return Green;
            case "blue":
                return Blue;
        }
        return Black;
    }
}
