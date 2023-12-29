using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Utils;

namespace UI;

[DataContract]
public class ButtonGroup : UIElement
{
    [IgnoreDataMember]
    protected Dictionary<UIElement, Point> ChildOffsetCache = new();
    [IgnoreDataMember]
    public Button SelectedButton;
    [IgnoreDataMember]
    private bool AttachedButtonEventListeners;

    public override Rectangle PreComputeDestinationCache(Rectangle drawArea, Point offset)
    {
        if (MarginComputed == null)
        {
            MarginComputed = Utils.ComputePaddingOrMargin(Margin, drawArea);
        }
        return Utils.PreComputeDestinationCache(
            ref ChildOffsetCache,
            this,
            padding: Point.Zero,
            drawArea,
            offset);
    }

    public override void Update(
        GameTime gameTime,
        UIState state,
        UISpriteSheet uiSpriteSheet,
        ControllerManager controllerManager)
    {
        if (Hidden)
        {
            return;
        }
        if (!AttachedButtonEventListeners)
        {
            foreach (var button in Children.OfType<ToggleButton>())
            {
                button.EventHandler += ListenForButtonEvent;
            }
            AttachedButtonEventListeners = true;
        }

        base.Update(gameTime, state, uiSpriteSheet, controllerManager);
    }

    protected override void DrawChildren(SpriteBatch spriteBatch)
    {
        foreach (var child in Children)
        {
            child.Draw(spriteBatch, DestinationCache, ChildOffsetCache[child]);
        }
    }

    private void ListenForButtonEvent(Button sender, ButtonState state, ButtonState previoudState)
    {
        if (state == ButtonState.Pressed && sender != SelectedButton)
        {
            foreach (var button in Children.OfType<ToggleButton>())
            {
                if (button != sender && button.State == ButtonState.Pressed)
                {
                    // will result in this method (ListenForButtonEvent) being called
                    // but will be ignored since state will equal Released instead of Pressed.
                    button.ResetState();
                }
            }
            SelectedButton = sender;
        }
    }
}
