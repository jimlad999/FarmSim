using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
    }

    private static readonly Regex TagRegex = new Regex(@"<(\w*)>");
    // should be set in Game.LoadContent()
#pragma warning disable CA2211 // Non-constant fields should not be visible
    public static SpriteFont Normal;
    public static SpriteFont Bold;
    public static Color Black = Color.Black;
    public static Color White = Color.White;
    public static Color Red = Color.Red;
    public static Color Green = Color.Green;
    public static Color Blue = Color.Blue;
#pragma warning restore CA2211 // Non-constant fields should not be visible

    [DataMember]
    public string Value;

    [IgnoreDataMember]
    private ProcessedData[] ParsedValue;

    // Ignore margin since top/bottom/left/right can do it instead
    public override Rectangle PreComputeDestinationCache(Rectangle drawArea, Point offset)
    {
        float maxHeight = 0f;
        float sumWidth = 0f;
        foreach (var value in ParsedValue)
        {
            var measurement = GetFont(value.Weight).MeasureString(value.Value);
            if (measurement.Y > maxHeight)
            {
                maxHeight = measurement.Y;
            }
            sumWidth += measurement.X;
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
            var weight = FontWeight.Normal;
            var color = Black;
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
                            value: value,
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
                        case "black":
                            color = Black;
                            break;
                        case "white":
                            color = color == White ? Black : White;
                            break;
                        case "red":
                            color = color == Red ? Black : Red;
                            break;
                        case "green":
                            color = color == Green ? Black : Green;
                            break;
                        case "blue":
                            color = color == Blue ? Black : Blue;
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
        base.Update(gameTime, state, uiSpriteSheet, controllerManager);
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
            var yDiff = DestinationCache.Height - measurement.Y;
            spriteBatch.DrawString(font, value.Value, new Vector2(x: x, y: y + yDiff), value.Color);
            x += measurement.X;
        }
    }

    private static int Ceiling(float value)
    {
        var truncate = (int)value;
        return value == truncate ? truncate : (int)(value + 1);
    }

    private static SpriteFont GetFont(FontWeight weight)
    {
        return weight == FontWeight.Normal ? Normal : Bold;
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
