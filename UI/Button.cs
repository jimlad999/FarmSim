using Microsoft.Xna.Framework;
using System.Runtime.Serialization;
using Utils;

namespace UI;

public delegate void ButtonEventHandler(Button sender, ButtonState state, ButtonState previoudState);

[DataContract]
public class Button : SpriteUIElement
{
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
        UpdateState(previousState, controllerManager);

        base.Update(gameTime, state, uiSpriteSheet, controllerManager);

        if (previousState != State && EventHandler != null)
        {
            EventHandler.Invoke(this, State, previousState);
        }
    }

    // For use with ButtonGroups
    public void ResetState()
    {
        if (State != ButtonState.Released)
        {
            var previousState = State;
            State = ButtonState.Released;
            Texture = ReleasedTexture;
            TextureStale = true;
            if (EventHandler != null)
            {
                EventHandler.Invoke(this, State, previousState);
            }
        }
    }

    protected override void UpdateUIState(UIState state, ControllerManager controllerManager)
    {
        if (!state.IsMouseOverInteractiveElement && DestinationCache.Contains(controllerManager.CurrentMouseState.Position))
        {
            state.IsMouseOverInteractiveElement = true;
        }
        base.UpdateUIState(state, controllerManager);
    }

    protected virtual void UpdateState(ButtonState previousState, ControllerManager controllerManager)
    {
        var selected = DestinationCache != Rectangle.Empty
            && DestinationCache.Contains(controllerManager.CurrentMouseState.Position);
        //or selected with controller
        if (selected && controllerManager.IsLeftMouseDown())
        {
            if (State != ButtonState.Pressed)
            {
                State = ButtonState.Pressed;
                Texture = PressedTexture;
                TextureStale = true;
            }

        }
        else if (selected)
        {
            if (State != ButtonState.Selected)
            {
                State = ButtonState.Selected;
                Texture = SelectedTexture;
                TextureStale = true;
            }
        }
        else
        {
            if (State != ButtonState.Released || Texture == null)
            {
                State = ButtonState.Released;
                Texture = ReleasedTexture;
                TextureStale = true;
            }
        }
    }
}
