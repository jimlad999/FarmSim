using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace UI;

public static class Utils
{
    public static int ComputePosition(
        Alignment? alignment,
        string startValue,
        string endValue,
        int thisDimensionSize,
        int parentDimensionSize)
    {
        int value = 0;
        if (alignment != null)
        {
            value = ToPixels(alignment.Value, thisDimensionSize: thisDimensionSize, parentDimensionSize: parentDimensionSize);
        }
        if (startValue != null)
        {
            value += ToPixels(startValue, parentDimensionSize);
        }
        else if (endValue != null)
        {
            if (alignment != null)
            {
                value -= ToPixels(endValue, parentDimensionSize);
            }
            else
            {
                value += parentDimensionSize - thisDimensionSize - ToPixels(endValue, parentDimensionSize);
            }
        }
        return value;
    }

    public static int ToPixels(string positionValue, int parentDimensionSize)
    {
        if (positionValue.EndsWith('%'))
        {
            var fraction = float.Parse(positionValue.Substring(0, positionValue.Length - 1)) / 100f;
            return (int)(parentDimensionSize * fraction);
        }
        return int.Parse(positionValue);
    }

    public static int ToPixels(Alignment alignment, int thisDimensionSize, int parentDimensionSize)
    {
        switch (alignment)
        {
            case Alignment.Top:
            case Alignment.Left:
                return 0;
            case Alignment.Bottom:
            case Alignment.Right:
                return parentDimensionSize - thisDimensionSize;
            case Alignment.Center:
                return (parentDimensionSize - thisDimensionSize) / 2;
            default:
                // Just draw something
                return 0;
        }
    }

    public static Point ComputePadding(string padding, Rectangle drawArea)
    {
        if (padding == null)
        {
            return Point.Zero;
        }
        var topPadding = ToPixels(padding, drawArea.Height);
        var leftPadding = ToPixels(padding, drawArea.Width);

        return new Point(x: leftPadding, y: topPadding);
    }

    public static Rectangle PreComputeDestinationCache(
        ref Dictionary<UIElement, Point> childOffsetCache,
        UIElement element,
        Point padding,
        Rectangle drawArea,
        Point offset)
    {
        childOffsetCache = new();
        if (element.Children.Length == 0)
        {
            return drawArea;
        }
        var parentX = ComputePosition(element.HorizontalAlignment, startValue: element.Left, endValue: element.Right, thisDimensionSize: drawArea.Width, parentDimensionSize: drawArea.Width);
        var parentY = ComputePosition(element.VerticalAlignment, startValue: element.Top, endValue: element.Bottom, thisDimensionSize: drawArea.Height, parentDimensionSize: drawArea.Height);

        var maxY = int.MinValue;
        var maxYHeight = 0;
        var childOffset = padding + offset;
        var maxDrawX = drawArea.Width + drawArea.X;
        foreach (var child in element.Children)
        {
            var childDestinationCache = child.PreComputeDestinationCache(drawArea, childOffset);
            if (childDestinationCache.X + childDestinationCache.Width + padding.X > maxDrawX)
            {
                childOffset = new Point(x: padding.X + offset.X, y: childOffset.Y + childDestinationCache.Height + padding.Y);
                childDestinationCache = child.PreComputeDestinationCache(drawArea, childOffset);
            }
            childOffsetCache[child] = childOffset;
            childOffset = new Point(x: childOffset.X + childDestinationCache.Width + padding.X, y: childOffset.Y);
            var childY = childDestinationCache.Y;
            var childHeight = childDestinationCache.Height;
            if (childY > maxY)
            {
                maxY = childY;
                maxYHeight = childHeight;
            }
            else if (childY == maxY && childHeight > maxYHeight)
            {
                maxYHeight = childHeight;
            }
        }
        var contentHeight = maxY - drawArea.Y + maxYHeight + padding.Y;
        return new Rectangle(
            x: drawArea.X + parentX,
            y: drawArea.Y + parentY,
            width: drawArea.Width - parentX,
            height: (contentHeight > drawArea.Height ? contentHeight : drawArea.Height) - parentY);
    }
}
