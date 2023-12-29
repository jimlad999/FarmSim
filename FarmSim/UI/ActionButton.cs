using System.Runtime.Serialization;
using UI;
using Utils;

namespace FarmSim.UI;

[DataContract]
class ActionButton : ToggleButton
{
    public bool IsActive => Children.Count > 0;

    protected override void UpdateState(ButtonState previousState, ControllerManager controllerManager)
    {
        // Only active if there is an action associated with this.
        // Children will be updated on the fly based on user interactions in the main menu.
        if (Children.Count == 0)
        {
            ResetState();
            if (Texture == null)
            {
                State = ButtonState.Released;
                Texture = ReleasedTexture;
                TextureStale = true;
            }
        }
        else
        {
            base.UpdateState(previousState, controllerManager);
        }
    }

    public void SetOption(ActionIcon newAction)
    {
        if (IsActive)
        {
            var oldAction = (ActionIcon)Children[0];
            EventHandler -= oldAction.ButtonEventHandler;
            Children.Clear();
        }
        Children.Add(newAction);
        EventHandler += newAction.ButtonEventHandler;
    }
}
