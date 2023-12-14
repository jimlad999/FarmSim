using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace UI;

[DataContract]
public class WrapContainer : UIElement
{
    [DataMember]
    public string Padding;
    [IgnoreDataMember]
    private Point? PaddingComputed;
    [IgnoreDataMember]
    private Dictionary<UIElement, Point> ChildOffsetCache = new();

    public override Rectangle PreComputeDestinationCache(Rectangle drawArea, Point offset)
    {
        ChildOffsetCache = new();
        if (Children.Length == 0)
        {
            return drawArea;
        }

        var maxY = int.MinValue;
        var maxYHeight = 0;
        var padding = PaddingComputed.Value;
        var childOffset = padding + offset;
        var maxDrawX = drawArea.Width + drawArea.X;
        foreach (var child in Children)
        {
            var childDestinationCache = child.PreComputeDestinationCache(drawArea, childOffset);
            if (childDestinationCache.X + childDestinationCache.Width + padding.X > maxDrawX)
            {
                childOffset = new Point(x: padding.X + offset.X, y: childOffset.Y + childDestinationCache.Height + padding.Y);
                childDestinationCache = child.PreComputeDestinationCache(drawArea, childOffset);
            }
            ChildOffsetCache[child] = childOffset;
            childOffset = new Point(x: childOffset.X + childDestinationCache.Width + padding.X, y: childOffset.Y);
            var y = childDestinationCache.Y;
            var height = childDestinationCache.Height;
            if (y > maxY)
            {
                maxY = y;
                maxYHeight = height;
            }
            else if (y == maxY && height > maxYHeight)
            {
                maxYHeight = height;
            }
        }
        return new Rectangle(
            x: drawArea.X,
            y: drawArea.Y,
            width: drawArea.Width,
            height: maxY - drawArea.Y + maxYHeight + padding.Y);
    }

    public override void Draw(SpriteBatch spriteBatch, Rectangle drawArea, Point offset)
    {
        if (!Hidden)
        {
            if (PaddingComputed == null)
            {
                PaddingComputed = ComputePadding(drawArea);
            }
            if (DestinationCache == Rectangle.Empty || CachedDrawArea != drawArea || CachedOffset != offset)
            {
                CachedDrawArea = drawArea;
                CachedOffset = offset;
                DestinationCache = PreComputeDestinationCache(drawArea, offset);
            }
            else if (!drawArea.Intersects(DestinationCache))
            {
                return;
            }
            foreach (var child in Children)
            {
                child.Draw(spriteBatch, DestinationCache, ChildOffsetCache[child]);
            }
        }
    }

    protected Point ComputePadding(Rectangle drawArea)
    {
        if (Padding == null)
        {
            return Point.Zero;
        }
        var topPadding = Utils.ToPixels(Padding, drawArea.Height);
        var leftPadding = Utils.ToPixels(Padding, drawArea.Width);

        return new Point(x: leftPadding, y: topPadding);
    }
}
