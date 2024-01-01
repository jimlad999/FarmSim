using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Utils;

namespace UI;

[DataContract]
public class Text : UIElement
{
    public enum FontWeight
    {
        Normal,
        Bold,
        // HACK: quick and dirty. Not technically "weight"
        Small,
    }

    // To avoid text clashing with TagRegex
    public const char PlaceholderLT = (char)171;//symbol '<<'
    public const char PlaceholderGT = (char)187;//symbol '>>'

    private static readonly Regex TagRegex = new Regex(@"<(\w*)>");
    // should be set in Game.LoadContent()
#pragma warning disable CA2211 // Non-constant fields should not be visible
    public static BitmapFont Normal;
    public static BitmapFont Bold;
    public static BitmapFont Small;
#pragma warning restore CA2211 // Non-constant fields should not be visible

    [DataMember]
    public string Value;

    [IgnoreDataMember]
    private ProcessedData[] ParsedValue;

    // For use with TextInput
    public int AlphaModifier = 255;

    // For use with TextInput
    public void UpdateValue(string newValue)
    {
        Value = newValue;
        ParseValue();
    }

    // For use with TextInput
    public int GetXPosition(int index)
    {
        if (index == 0)
        {
            return DestinationCache.X;
        }
        var sumLength = 0;
        double sumWidth = DestinationCache.X;
        foreach (var value in ParsedValue)
        {
            var stringValue = value.Value;
            sumLength += stringValue.Length;
            if (index < sumLength)
            {
                var measurement = GetFont(value.Weight).MeasureString(stringValue.Substring(0, (int)(stringValue.Length - (sumLength - index))));
                return (int)(sumWidth + measurement.Width);
            }
            else
            {
                var measurement = GetFont(value.Weight).MeasureString(stringValue);
                sumWidth += measurement.Width;
            }
        }
        return (int)sumWidth;
    }

    // Ignore margin since top/bottom/left/right can do it instead
    public override Rectangle PreComputeDestinationCache(Rectangle drawArea, Point offset)
    {
        float maxHeight = 0f;
        float sumWidth = 0f;
        foreach (var value in ParsedValue)
        {
            var measurement = GetFont(value.Weight).MeasureString(value.Value);
            if (measurement.Height > maxHeight)
            {
                maxHeight = measurement.Height;
            }
            sumWidth += measurement.Width;
        }
        var height = Ceiling(maxHeight);
        var width = Ceiling(sumWidth);
        var y = Utils.ComputePosition(VerticalAlignment, startValue: Top, endValue: Bottom, thisDimensionSize: height, parentDimensionSize: drawArea.Height);
        var x = Utils.ComputePosition(HorizontalAlignment, startValue: Left, endValue: Right, thisDimensionSize: width, parentDimensionSize: drawArea.Width);

        return new Rectangle(x: drawArea.X + offset.X + x, y: drawArea.Y + offset.Y + y, width: width, height: height);
    }

    public override void Update(GameTime gameTime, UIState state, UISpriteSheet uiSpriteSheet, ControllerManager controllerManager)
    {
        if (ParsedValue == null && !string.IsNullOrEmpty(Value))
        {
            ParseValue();
        }
        base.Update(gameTime, state, uiSpriteSheet, controllerManager);
    }

    private void ParseValue()
    {
        var weight = FontWeight.Normal;
        var color = ColorPalette.Black;
        var processed = 0;
        var remainingValue = Value;
        var parsedList = new List<ProcessedData>();
        do
        {
            var match = TagRegex.Match(remainingValue);
            string value = match.Success
                ? remainingValue.Substring(0, match.Index)
                : remainingValue;
            if (!string.IsNullOrEmpty(value))
            {
                parsedList.Add(
                    new ProcessedData(
                        value: value.Replace(PlaceholderGT, '>').Replace(PlaceholderLT, '<'),
                        weight: weight,
                        color: color));
                processed += value.Length;
            }
            if (match.Success)
            {
                // [1] because [0] will include the brackets
                var tag = match.Groups[1].Value;
                // + 2 for brackets
                var toSkip = 2 + tag.Length;
                remainingValue = remainingValue.Substring(toSkip + value.Length);
                processed += toSkip;
                switch (tag)
                {
                    case "b":
                        weight = weight == FontWeight.Normal ? FontWeight.Bold : FontWeight.Normal;
                        break;
                    case "small":
                        weight = weight == FontWeight.Normal ? FontWeight.Small : FontWeight.Normal;
                        break;
                    default:
                        var newColor = ColorPalette.Parse(tag);
                        color = color == newColor ? ColorPalette.Black : newColor;
                        break;
                }
            }
            else if (!string.IsNullOrEmpty(value))
            {
                remainingValue = remainingValue.Substring(value.Length);
            }
        } while (processed < Value.Length);
        ParsedValue = parsedList.ToArray();
    }

    public override void Draw(SpriteBatch spriteBatch, Rectangle drawArea, Point offset)
    {
        if (Hidden || string.IsNullOrEmpty(Value) || ParsedValue == null)
        {
            return;
        }
        if (DestinationCache == Rectangle.Empty || CachedDrawArea != drawArea || CachedOffset != offset)
        {
            CachedDrawArea = drawArea;
            CachedOffset = offset;
            DestinationCache = PreComputeDestinationCache(drawArea, offset);
        }
        if (!drawArea.Intersects(DestinationCache))
        {
            return;
        }
        float y = DestinationCache.Y;
        float x = DestinationCache.X;
        foreach (var value in ParsedValue)
        {
            var font = GetFont(value.Weight);
            var measurement = font.MeasureString(value.Value);
            var yDiff = DestinationCache.Height - measurement.Height;
            if (AlphaModifier != 255)
            {
                spriteBatch.DrawString(font, value.Value, new Vector2(x: x, y: y + yDiff), new Color(value.Color, AlphaModifier));
            }
            else
            {
                spriteBatch.DrawString(font, value.Value, new Vector2(x: x, y: y + yDiff), value.Color);
            }
            x += measurement.Width;
        }
    }

    private static int Ceiling(float value)
    {
        var truncate = (int)value;
        return value == truncate ? truncate : (int)(value + 1);
    }

    private static BitmapFont GetFont(FontWeight weight)
    {
        return weight == FontWeight.Normal ? Normal : weight == FontWeight.Small ? Small : Bold;
    }

    public struct ProcessedData
    {
        public string Value;
        public FontWeight Weight;
        public Color Color;

        public ProcessedData(string value, FontWeight weight, Color color)
        {
            Value = value;
            Weight = weight;
            Color = color;
        }
    }

}
