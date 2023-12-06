using Microsoft.Xna.Framework;
using System.Runtime.Serialization;
using Utils;

namespace UI;

[DataContract]
public class Button : UIElement
{
    public enum ButtonState
    {
        Released,
        Selected,
        Pressed,
    }

    [DataMember]
    public string ReleasedTexture;
    [DataMember]
    public string SelectedTexture;
    [DataMember]
    public string PressedTexture;
    // key lookup to action
    [DataMember]
    public string PressedAction;

    [IgnoreDataMember]
    public ButtonState State;

    public override void Update(
        GameTime gameTime,
        UISpriteSheet uiSpriteSheet,
        ControllerManager controllerManager)
    {
        var selected = DestinationCache != Rectangle.Empty
            && DestinationCache.Contains(controllerManager.CurrentMouseState.Position);
        //or selected with controller
        if (selected && controllerManager.IsLeftMouseDown())
        {
            State = ButtonState.Pressed;
            if (Texture != PressedTexture)
            {
                Texture = PressedTexture;
                TextureStale = true;
            }
        }
        else if (selected)
        {
            State = ButtonState.Selected;
            if (Texture != SelectedTexture)
            {
                Texture = SelectedTexture;
                TextureStale = true;
            }
        }
        else
        {
            State = ButtonState.Released;
            if (Texture != ReleasedTexture)
            {
                Texture = ReleasedTexture;
                TextureStale = true;
            }
        }

        base.Update(gameTime, uiSpriteSheet, controllerManager);
    }
}
