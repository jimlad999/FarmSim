using System.Runtime.Serialization;
using Utils;

namespace UI;

[DataContract]
public class ToggleButton : Button
{
    protected override void UpdateState(ButtonState previousState, ControllerManager controllerManager)
    {
        if (State == ButtonState.Pressed)
        {
            return;
        }
        base.UpdateState(previousState, controllerManager);
    }

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
}
