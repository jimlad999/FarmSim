using Microsoft.Xna.Framework;
using System;
using System.Runtime.Serialization;
using Utils;

namespace UI;

public delegate void ButtonEventHandler(Button sender, Button.ButtonState state, Button.ButtonState previoudState);

[DataContract]
public class Button : SpriteUIElement
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

    [IgnoreDataMember]
    public ButtonState State;
    [IgnoreDataMember]
    public ButtonEventHandler EventHandler;

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
        var previousState = State;
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

        base.Update(gameTime, state, uiSpriteSheet, controllerManager);

        if (previousState != State && EventHandler != null)
        {
            EventHandler.Invoke(this, State, previousState);
        }
    }
}
