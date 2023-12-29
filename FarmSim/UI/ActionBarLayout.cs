using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UI;
using Utils;
using UIUtils = UI.Utils;

namespace FarmSim.UI;

[DataContract]
class ActionBarLayout : Panel
{
    [DataMember]
    public Button LeftButton;
    [DataMember]
    public Button RightButton;
    [DataMember]
    public ActionBar ActionBar;
    [DataMember]
    public string Padding;
    [IgnoreDataMember]
    private Point? PaddingComputed;
    [IgnoreDataMember]
    private Dictionary<UIElement, Point> ChildOffsetCache = new();

    public override bool TryGetById<T>(string id, out T result)
    {
        return base.TryGetById(id, out result)
            || LeftButton.TryGetById(id, out result)
            || ActionBar.TryGetById(id, out result)
            || RightButton.TryGetById(id, out result);
    }

    public override Rectangle PreComputeDestinationCache(Rectangle drawArea, Point offset)
    {
        if (MarginComputed == null)
        {
            MarginComputed = UIUtils.ComputePaddingOrMargin(Margin, drawArea);
        }
        if (PaddingComputed == null)
        {
            PaddingComputed = UIUtils.ComputePaddingOrMargin(Padding, drawArea);
        }
        return UIUtils.PreComputeDestinationCache(
            ref ChildOffsetCache,
            this,
            PaddingComputed.Value,
            drawArea,
            offset,
            positionParentBasedOnChildrenDimensions: true);
    }

    public override void Update(GameTime gameTime, UIState state, UISpriteSheet uiSpriteSheet, ControllerManager controllerManager)
    {
        if (Children.Count == 0)
        {
            // order is important to render things in corret place
            Children.Add(LeftButton);
            Children.Add(ActionBar);
            Children.Add(RightButton);
        }
        // reflow based on children resizing
        if (DestinationCache != Rectangle.Empty && Children.Exists(c => c.DestinationCache == Rectangle.Empty))
        {
            DestinationCache = Rectangle.Empty;
        }
        base.Update(gameTime, state, uiSpriteSheet, controllerManager);
    }

    protected override void DrawChildren(SpriteBatch spriteBatch)
    {
        foreach (var child in Children)
        {
            child.Draw(spriteBatch, DestinationCache, offset: ChildOffsetCache[child]);
        }
    }
}
