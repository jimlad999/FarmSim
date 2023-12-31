using Microsoft.Xna.Framework;
using System;
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

    public static Rectangle PreComputeDestinationCacheHorizontalLayout(
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

        var margin = element.MarginComputed.Value;
        return PreComputeDestinationCache(
            ref childOffsetCache,
            element,
            primaryOffset: offset.X,
            secondaryOffset: offset.Y,
            primaryPadding: padding.X,
            secondaryPadding: padding.Y,
            primaryMargin: margin.X,
            secondaryMargin: margin.Y,
            drawAreaPrimaryDraw: drawArea.X,
            drawAreaSecondaryDraw: drawArea.Y,
            drawAreaPrimaryDimension: drawArea.Width,
            drawAreaSecondaryDimension: drawArea.Height,
            positionParentBasedOnChildrenDimensions: positionParentBasedOnChildrenDimensions,
            primaryPoint: p => p.X,
            secondaryPoint: p => p.Y,
            primaryDimension: r => r.Width,
            secondaryDimension: r => r.Height,
            createPoint: (primary, secondary) => new Point(x: primary, y: secondary),
            createRectangle: (primaryDraw, secondaryDraw, primaryDimension, secondaryDimension) =>
                new Rectangle(x: primaryDraw, y: secondaryDraw, width: primaryDimension, height: secondaryDimension),
            preComputeDestinationCache: (child, primaryChildOffset, secondaryChildOffset) =>
                child.PreComputeDestinationCache(drawArea, new Point(x: primaryChildOffset, y: secondaryChildOffset)),
            computePrimaryDraw: thisDimensionSize =>
                ComputePosition(element.HorizontalAlignment, startValue: element.Left, endValue: element.Right, thisDimensionSize: thisDimensionSize, parentDimensionSize: drawArea.Width),
            computeSecondaryDraw: thisDimensionSize =>
                ComputePosition(element.VerticalAlignment, startValue: element.Top, endValue: element.Bottom, thisDimensionSize: thisDimensionSize, parentDimensionSize: drawArea.Height));
    }

    public static Rectangle PreComputeDestinationCacheVerticallLayout(
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

        var margin = element.MarginComputed.Value;
        return PreComputeDestinationCache(
            ref childOffsetCache,
            element,
            primaryOffset: offset.Y,
            secondaryOffset: offset.X,
            primaryPadding: padding.Y,
            secondaryPadding: padding.X,
            primaryMargin: margin.Y,
            secondaryMargin: margin.X,
            drawAreaPrimaryDraw: drawArea.Y,
            drawAreaSecondaryDraw: drawArea.X,
            drawAreaPrimaryDimension: drawArea.Height,
            drawAreaSecondaryDimension: drawArea.Width,
            positionParentBasedOnChildrenDimensions: positionParentBasedOnChildrenDimensions,
            primaryPoint: p => p.Y,
            secondaryPoint: p => p.X,
            primaryDimension: r => r.Height,
            secondaryDimension: r => r.Width,
            createPoint: (primary, secondary) => new Point(x: secondary, y: primary),
            createRectangle: (primaryDraw, secondaryDraw, primaryDimension, secondaryDimension) =>
                new Rectangle(x: secondaryDraw, y: primaryDraw, width: secondaryDimension, height: primaryDimension),
            preComputeDestinationCache: (child, primaryChildOffset, secondaryChildOffset) =>
                child.PreComputeDestinationCache(drawArea, new Point(x: secondaryChildOffset, y: primaryChildOffset)),
            computePrimaryDraw: thisDimensionSize =>
                ComputePosition(element.VerticalAlignment, startValue: element.Top, endValue: element.Bottom, thisDimensionSize: thisDimensionSize, parentDimensionSize: drawArea.Height),
            computeSecondaryDraw: thisDimensionSize =>
                ComputePosition(element.HorizontalAlignment, startValue: element.Left, endValue: element.Right, thisDimensionSize: thisDimensionSize, parentDimensionSize: drawArea.Width));
    }

    private static Rectangle PreComputeDestinationCache(
        ref Dictionary<UIElement, Point> childOffsetCache,
        UIElement element,
        int primaryOffset,
        int secondaryOffset,
        int primaryPadding,
        int secondaryPadding,
        int primaryMargin,
        int secondaryMargin,
        int drawAreaPrimaryDraw,
        int drawAreaSecondaryDraw,
        int drawAreaPrimaryDimension,
        int drawAreaSecondaryDimension,
        bool positionParentBasedOnChildrenDimensions,
        Func<Point, int> primaryPoint,
        Func<Point, int> secondaryPoint,
        Func<Rectangle, int> primaryDimension,
        Func<Rectangle, int> secondaryDimension,
        // (primary, secondary) => Point
        Func<int, int, Point> createPoint,
        // (primaryDraw, secondaryDraw, primaryDimension, secondaryDimension) => Rectangle
        Func<int, int, int, int, Rectangle> createRectangle,
        // (child, primaryChildOffset, secondaryChildOffset) => Rectangle
        Func<UIElement, int, int, Rectangle> preComputeDestinationCache,
        // (thisDimensionSize) => ComputePosition(...)
        Func<int, int> computePrimaryDraw,
        Func<int, int> computeSecondaryDraw)
    {
        var maxSecondaryDraw = int.MinValue;
        var maxSecondaryDimension = 0;
        var primaryChildOffset = primaryOffset + primaryPadding;
        var secondaryChildOffset = secondaryOffset + secondaryPadding;
        var maxPrimaryDraw = drawAreaPrimaryDimension + drawAreaPrimaryDraw;
        var maxPrimaryDimension = primaryPadding;
        var partialSumPrimaryDimension = maxPrimaryDimension;
        foreach (var child in element.Children)
        {
            var childDestinationCache = preComputeDestinationCache(child, primaryChildOffset, secondaryChildOffset);
            partialSumPrimaryDimension += primaryDimension(childDestinationCache) + primaryPadding;
            if (partialSumPrimaryDimension > maxPrimaryDimension)
            {
                maxPrimaryDimension = partialSumPrimaryDimension;
            }
            if (primaryPoint(childDestinationCache.Location) + primaryDimension(childDestinationCache) + primaryPadding > maxPrimaryDraw)
            {
                partialSumPrimaryDimension = 0;
                primaryChildOffset = primaryPadding + primaryOffset;
                secondaryChildOffset += secondaryDimension(childDestinationCache) + secondaryPadding;
                childDestinationCache = preComputeDestinationCache(child, primaryChildOffset, secondaryChildOffset);
            }
            childOffsetCache[child] = createPoint(primaryChildOffset, secondaryChildOffset);
            primaryChildOffset += primaryDimension(childDestinationCache) + primaryPadding;
            var childSecondaryDraw = secondaryPoint(childDestinationCache.Location);
            var childSecondaryDimension = secondaryDimension(childDestinationCache);
            if (childSecondaryDraw > maxSecondaryDraw)
            {
                maxSecondaryDraw = childSecondaryDraw;
                maxSecondaryDimension = childSecondaryDimension;
            }
            else if (childSecondaryDraw == maxSecondaryDraw && childSecondaryDimension > maxSecondaryDimension)
            {
                maxSecondaryDimension = childSecondaryDimension;
            }
        }
        var finalSecondaryDimension = maxSecondaryDraw + maxSecondaryDimension + secondaryPadding - drawAreaSecondaryDraw - secondaryOffset;
        int parentPrimaryDraw;
        int parentSecondaryDraw;
        int parentPrimaryDimension;
        int parentSecondaryDimension;
        if (positionParentBasedOnChildrenDimensions)
        {
            parentPrimaryDraw = computePrimaryDraw(maxPrimaryDimension);
            parentSecondaryDraw = computeSecondaryDraw(finalSecondaryDimension);
            parentPrimaryDimension = maxPrimaryDimension;
            parentSecondaryDimension = finalSecondaryDimension;
            // Realign the parent and the child offsets as the parents dimensions could have potentially changed based on the sum of the childrens dimensions.
            if (primaryOffset != 0 || secondaryOffset != 0)
            {
                parentPrimaryDraw += primaryOffset;
                parentSecondaryDraw += secondaryOffset;
                foreach (var value in childOffsetCache)
                {
                    var currentOffset = childOffsetCache[value.Key];
                    childOffsetCache[value.Key] = createPoint(primaryPoint(currentOffset) - primaryOffset, secondaryPoint(currentOffset) - parentSecondaryDraw);
                }
            }
        }
        else
        {
            parentPrimaryDraw = computePrimaryDraw(drawAreaPrimaryDimension);
            parentSecondaryDraw = computeSecondaryDraw(drawAreaSecondaryDimension);
            parentPrimaryDimension = drawAreaPrimaryDimension - parentPrimaryDraw - primaryMargin * 2;
            parentSecondaryDimension = (finalSecondaryDimension > drawAreaSecondaryDimension ? finalSecondaryDimension : drawAreaSecondaryDimension) - parentSecondaryDraw - secondaryMargin * 2;
        }
        return createRectangle(
            drawAreaPrimaryDraw + parentPrimaryDraw + primaryMargin,
            drawAreaSecondaryDraw + parentSecondaryDraw + secondaryMargin,
            parentPrimaryDimension,
            parentSecondaryDimension);
    }
}
