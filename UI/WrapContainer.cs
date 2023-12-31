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
        if (MarginComputed == null)
        {
            MarginComputed = Utils.ComputePaddingOrMargin(Margin, drawArea);
        }
        if (PaddingComputed == null)
        {
            PaddingComputed = Utils.ComputePaddingOrMargin(Padding, drawArea);
        }
        return Utils.PreComputeDestinationCacheHorizontalLayout(
            ref ChildOffsetCache,
            this,
            PaddingComputed.Value,
            drawArea,
            offset);
    }

    protected override void DrawChildren(SpriteBatch spriteBatch)
    {
        foreach (var child in Children)
        {
            child.Draw(spriteBatch, DestinationCache, ChildOffsetCache[child]);
        }
    }
}
