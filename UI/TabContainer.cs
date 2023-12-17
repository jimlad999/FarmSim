using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using System.Runtime.Serialization;
using Utils;

namespace UI;

[DataContract]
public class TabContainer : UIElement
{
    [DataMember]
    public ButtonGroup Tabs;
    [DataMember]
    public Panel TabContentPanel;

    public override bool TryGetById<T>(string id, out T result)
    {
        if (Tabs != null && Tabs.TryGetById<T>(id, out result))
        {
            return true;
        }
        if (TabContentPanel != null && TabContentPanel.TryGetById<T>(id, out result))
        {
            return true;
        }
        return base.TryGetById(id, out result);
    }

    public override void Update(GameTime gameTime, UIState state, UISpriteSheet uiSpriteSheet, ControllerManager controllerManager)
    {
        if (Hidden)
        {
            return;
        }
        if (Children.Length > 0)
        {
            if (Tabs == null)
            {
                Tabs = Children.OfType<ButtonGroup>().First();
            }
            if (TabContentPanel == null)
            {
                TabContentPanel = Children.OfType<Panel>().First();
            }
            Children = Array.Empty<UIElement>();
        }
        Tabs.Update(gameTime, state, uiSpriteSheet, controllerManager);
        TabContentPanel.Update(gameTime, state, uiSpriteSheet, controllerManager);
        if (Tabs.SelectedButton == null && Tabs.Children.First() is Tab firstTab)
        {
            firstTab.Select();
            // double update of tabs and its children to ensure state is correct at this point.
            Tabs.Update(gameTime, state, uiSpriteSheet, controllerManager);
        }
        if (Tabs.SelectedButton != null && Tabs.SelectedButton is Tab selectedTab)
        {
            foreach (var child in selectedTab.TabContent)
            {
                child.Update(gameTime, state, uiSpriteSheet, controllerManager);
            }
        }
    }

    public override void Draw(SpriteBatch spriteBatch, Rectangle drawArea, Point offset)
    {
        if (Hidden)
        {
            return;
        }
        if (DestinationCache == Rectangle.Empty || CachedDrawArea != drawArea || CachedOffset != offset)
        {
            CachedDrawArea = drawArea;
            CachedOffset = offset;
            DestinationCache = PreComputeDestinationCache(drawArea, offset);
        }
        if (TabContentPanel != null)
        {
            TabContentPanel.Draw(spriteBatch, DestinationCache, offset: Point.Zero);
        }
        if (Tabs != null)
        {
            Tabs.Draw(spriteBatch, DestinationCache, offset: Point.Zero);
            if (Tabs.SelectedButton != null && Tabs.SelectedButton is Tab selectedTab)
            {
                selectedTab.DrawTabContent(spriteBatch, TabContentPanel.DestinationCache, offset: Point.Zero);
            }
        }
    }
}
