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
        return Utils.PreComputeDestinationCache(
            ref ChildOffsetCache,
            this,
            PaddingComputed.Value,
            drawArea,
            offset);
    }

    public override void Draw(SpriteBatch spriteBatch, Rectangle drawArea, Point offset)
    {
        if (Hidden)
        {
            return;
        }
        if (PaddingComputed == null)
        {
            PaddingComputed = Utils.ComputePadding(Padding, drawArea);
        }
        base.Draw(spriteBatch, drawArea, offset);
    }

    protected override void DrawChildren(SpriteBatch spriteBatch)
    {
        foreach (var child in Children)
        {
            child.Draw(spriteBatch, DestinationCache, ChildOffsetCache[child]);
        }
    }
}
