using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

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
        if (int.TryParse(positionValue, out var intValue))
        {
            return intValue;
        }
        if (positionValue.Contains('-'))
        {
            return positionValue.Split("-")
                .Select(value => ToPixels(value.Trim(), parentDimensionSize))
                .Aggregate((agg, value) => agg - value);
        }
        if (positionValue.Contains('+'))
        {
            return positionValue.Split("+", System.StringSplitOptions.RemoveEmptyEntries)
                .Select(value => ToPixels(value.Trim(), parentDimensionSize))
                .Aggregate((agg, value) => agg - value);
        }
        // currently falls into the default case. empty is expected for "-x +/- y" as this will result in "", "x", "y"
        //if (string.IsNullOrEmpty(positionValue))
        //{
        //    return 0;
        //}

        // Just draw something
        return 0;
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

    public static Point ComputePaddingOrMargin(string padding, Rectangle drawArea)
    {
        if (padding == null)
        {
            return Point.Zero;
        }
        int topPadding;
        int leftPadding;
        if (padding.Trim().Contains(" "))
        {
            var paddingParsed = padding.Split(" ", System.StringSplitOptions.RemoveEmptyEntries);
            if (paddingParsed.Length == 2)
            {
                topPadding = ToPixels(paddingParsed[0], drawArea.Height);
                leftPadding = ToPixels(paddingParsed[1], drawArea.Width);
                return new Point(x: leftPadding, y: topPadding);
            }
        }
        topPadding = ToPixels(padding, drawArea.Height);
        leftPadding = ToPixels(padding, drawArea.Width);

        return new Point(x: leftPadding, y: topPadding);
    }

    public static Rectangle PreComputeDestinationCache(
        ref Dictionary<UIElement, Point> childOffsetCache,
        UIElement element,
        Point padding,
        Rectangle drawArea,
        Point offset,
        bool positionParentBasedOnChildrenDimensions = false)
    {
        childOffsetCache = new();
        if (element.Children.Count == 0)
        {
            return drawArea;
        }

        var maxY = int.MinValue;
        var maxYHeight = 0;
        var childOffset = padding + offset;
        var maxDrawX = drawArea.Width + drawArea.X;
        var maxWidth = padding.X;
        var partialSumWidth = padding.X;
        foreach (var child in element.Children)
        {
            var childDestinationCache = child.PreComputeDestinationCache(drawArea, childOffset);
            partialSumWidth += childDestinationCache.Width + padding.X;
            if (partialSumWidth > maxWidth)
            {
                maxWidth = partialSumWidth;
            }
            if (childDestinationCache.X + childDestinationCache.Width + padding.X > maxDrawX)
            {
                partialSumWidth = 0;
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
        var contentHeight = maxY - drawArea.Y + maxYHeight;
        int parentX;
        int parentY;
        int containerWidth;
        int containerHeight;
        var margin = element.MarginComputed.Value;
        if (positionParentBasedOnChildrenDimensions)
        {
            parentX = ComputePosition(element.HorizontalAlignment, startValue: element.Left, endValue: element.Right, thisDimensionSize: maxWidth, parentDimensionSize: drawArea.Width);
            parentY = ComputePosition(element.VerticalAlignment, startValue: element.Top, endValue: element.Bottom, thisDimensionSize: contentHeight, parentDimensionSize: drawArea.Height);
            containerWidth = maxWidth;
            containerHeight = contentHeight;
            // Realign the parent and the child offsets as the parents dimensions could have potentially changed based on the sum of the childrens dimensions.
            if (offset.X != 0 || offset.Y != 0)
            {
                parentX += offset.X;
                parentY += offset.Y;
                foreach (var value in childOffsetCache)
                {
                    var currentOffset = childOffsetCache[value.Key];
                    childOffsetCache[value.Key] = new Point(x: currentOffset.X - offset.X, y: currentOffset.Y - offset.Y);
                }
            }
        }
        else
        {
            parentX = ComputePosition(element.HorizontalAlignment, startValue: element.Left, endValue: element.Right, thisDimensionSize: drawArea.Width, parentDimensionSize: drawArea.Width);
            parentY = ComputePosition(element.VerticalAlignment, startValue: element.Top, endValue: element.Bottom, thisDimensionSize: drawArea.Height, parentDimensionSize: drawArea.Height);
            containerWidth = drawArea.Width - parentX - margin.X * 2;
            containerHeight = (contentHeight > drawArea.Height ? contentHeight : drawArea.Height) - parentY - margin.Y * 2;
        }
        return new Rectangle(
            x: drawArea.X + parentX + margin.X,
            y: drawArea.Y + parentY + margin.Y,
            width: containerWidth,
            height: containerHeight);
    }
}
