using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Runtime.Serialization;

namespace UI;

[DataContract]
public class Tab : ToggleButton
{
    // Drawn only if this is the selected tab in the parent TabContainer.
    // Drawn in the drawArea determined by the TabContainer.
    [DataMember]
    public UIElement[] TabContent = Array.Empty<UIElement>();

    // For use with TabContainer
    public void Select()
    {
        if (State != ButtonState.Pressed)
        {
            var previousState = State;
            State = ButtonState.Pressed;
            Texture = PressedTexture;
            TextureStale = true;
            if (EventHandler != null)
            {
                EventHandler.Invoke(this, State, previousState);
            }
        }
    }

    public override bool TryGetById<T>(string id, out T result)
    {
        foreach (var child in TabContent)
        {
            if (child.TryGetById(id, out result))
            {
                return true;
            }
        }
        return base.TryGetById(id, out result);
    }

    // drawArea here should be the TabContainer.TabContentPanel
    public void DrawTabContent(SpriteBatch spriteBatch, Rectangle drawArea, Point offset)
    {
        if (!Hidden)
        {
            foreach (var child in TabContent)
            {
                child.Draw(spriteBatch, drawArea, offset);
            }
        }
    }
}
