using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace UI;

// assume fill from top down
[DataContract]
public class Log : UIElement
{
    public enum Level
    {
        Info,
        Debug,
    }

    // default font weight
    [DataMember]
    public Text.FontWeight Weight = Text.FontWeight.Normal;
    [DataMember]
    public string InfoColor = "white";
    [DataMember]
    public string DebugColor = "gray";

    public void PushText(IEnumerable<(string newValue, Level level)> values)
    {
        int yOffset = DestinationCache.Height;
        foreach (var (newValue, level) in values)
        {
            var color = InfoColor;
            switch (level)
            {
                case Level.Info:
                    color = InfoColor;
                    break;
                case Level.Debug:
                    color = DebugColor;
                    break;
            }
            var newText = new Text
            {
                Top = (yOffset + 1).ToString(),
                Left = "0",
                Weight = Weight,
            };
            newText.UpdateValue($"<{color}>{newValue}");
            var childDestination = newText.PreComputeDestinationCache(DestinationCache, Point.Zero);
            yOffset += childDestination.Height;
            Children.Add(newText);
        }
        Resize();
    }

    public override Rectangle PreComputeDestinationCache(Rectangle drawArea, Point offset)
    {
        if (MarginComputed == null)
        {
            MarginComputed = Utils.ComputePaddingOrMargin(Margin, drawArea);
        }
        var margin = MarginComputed.Value;
        var height = 0;
        foreach (var child in Children)
        {
            var childDestination = child.PreComputeDestinationCache(drawArea, offset);
            height += childDestination.Height;
        }
        return new Rectangle(
            x: drawArea.X + offset.X + margin.X,
            y: drawArea.Y + offset.Y + margin.Y,
            width: drawArea.Width - offset.X - margin.X * 2,
            height: height);
    }
}
