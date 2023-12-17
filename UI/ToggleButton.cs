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
}
