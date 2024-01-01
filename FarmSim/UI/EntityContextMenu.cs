using FarmSim.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UI;
using Utils;
using UIUtils = UI.Utils;

namespace FarmSim.UI;

[DataContract]
class EntityContextMenu : Panel
{
    public const string MenuItemHeight = "24";
    public const int MenuItemWidthInt = 120;
    public const string MenuItemWidth = "120";

    // Default available context buttons. Static so the event handlers can be attached once and the each menu item can be set as desired (based on the context)
    // Should this not be static here but instead store the static menu items in Game? This would allow customisations to be set on this element within the json file.
    public readonly static ToggleButton Follow = CreateMenuItem("Follow");
    public readonly static ToggleButton StopFollowing = CreateMenuItem("Stop following");
    public readonly static ToggleButton Work = CreateMenuItem("Work");
    public readonly static ToggleButton Feed = CreateMenuItem("Feed");

    [DataMember]
    public string Padding;
    [IgnoreDataMember]
    private Point? PaddingComputed;

    [IgnoreDataMember]
    public Entity TrackingEntity;
    [IgnoreDataMember]
    private int XCache;
    [IgnoreDataMember]
    private int YCache;
    [IgnoreDataMember]
    private bool UpdateRunAtLeastOnce;
    [IgnoreDataMember]
    private Dictionary<UIElement, Point> ChildOffsetCache = new();

    public void SetMenuItems(IEnumerable<ToggleButton> menuItems, Entity trackingEntity)
    {
        Clear();
        TrackingEntity = trackingEntity;
        Children.AddRange(menuItems);
    }

    public void Clear()
    {
        foreach (var child in Children)
        {
            (child as ToggleButton)?.ResetState();
        }
        Children.Clear();
        TrackingEntity = null;
    }

    public override Rectangle PreComputeDestinationCache(Rectangle drawArea, Point offset)
    {
        if (TrackingEntity == null)
        {
            return Rectangle.Empty;
        }
        if (MarginComputed == null)
        {
            MarginComputed = UIUtils.ComputePaddingOrMargin(Margin, drawArea);
        }
        if (PaddingComputed == null)
        {
            PaddingComputed = UIUtils.ComputePaddingOrMargin(Padding, drawArea);
        }
        return UIUtils.PreComputeDestinationCacheVerticallLayout(
            ref ChildOffsetCache,
            this,
            PaddingComputed.Value,
            drawArea,
            offset,
            positionParentBasedOnChildrenDimensions: true);
    }

    public override void Update(GameTime gameTime, UIState state, UISpriteSheet uiSpriteSheet, ControllerManager controllerManager)
    {
        if (TrackingEntity == null)
        {
            UpdateRunAtLeastOnce = false;
            DestinationCache = Rectangle.Empty;
            return;
        }
        var (x, y) = GlobalState.ViewportManager.ConvertWorldCoordinatesToScreenCoordinates(TrackingEntity.XInt, TrackingEntity.YInt);
        var xOffset = (int)(TrackingEntity.HitRadius * GlobalState.ViewportManager.Zoom);
        var yOffset = (int)(TrackingEntity.Height * GlobalState.ViewportManager.Zoom);
        var screen = GlobalState.ViewportManager.ScreenDimensions;
        var halfScreenWidth = screen.Width / 2;
        var leftQuarterScreenWidth = screen.Width / 4;
        var rightQuarterScreenWidth = screen.Width - leftQuarterScreenWidth;
        if (x < leftQuarterScreenWidth || (x > halfScreenWidth && x < rightQuarterScreenWidth))
        {
            x += xOffset;
        }
        else
        {
            x -= xOffset + MenuItemWidthInt;
        }
        y -= yOffset;
        if (x != XCache || y != YCache)
        {
            DestinationCache = Rectangle.Empty;
            XCache = x;
            YCache = y;
            Left = x.ToString();
            Top = y.ToString();
        }
        UpdateRunAtLeastOnce = true;
        base.Update(gameTime, state, uiSpriteSheet, controllerManager);
    }

    public override void Draw(SpriteBatch spriteBatch, Rectangle drawArea, Point offset)
    {
        if (TrackingEntity == null || !UpdateRunAtLeastOnce)
        {
            return;
        }
        base.Draw(spriteBatch, drawArea, offset);
    }

    protected override void DrawChildren(SpriteBatch spriteBatch)
    {
        foreach (var child in Children)
        {
            child.Draw(spriteBatch, DestinationCache, offset: ChildOffsetCache[child]);
        }
    }

    private static ToggleButton CreateMenuItem(string menuItemText)
    {
        var menuItem = new ToggleButton
        {
            // should we simply rely on the texture dimension and create dedicated textures?
            Width = MenuItemWidth,
            Height = MenuItemHeight,
            PressedTexture = "button-pressed",
            ReleasedTexture = "button-released",
            SelectedTexture = "button-selected",
        };
        menuItem.Children.Add(new Text
        {
            Value = $"<small><white>{menuItemText}",
            HorizontalAlignment = Alignment.Center,
            VerticalAlignment = Alignment.Center,
        });
        return menuItem;
    }
}
