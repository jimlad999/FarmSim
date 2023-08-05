using Microsoft.Xna.Framework.Input;

namespace FarmSim.Utils;

internal class ControllerManager
{
    public MouseState PreviousMouseState { get; private set; }
    public MouseState CurrentMouseState { get; private set; }
    public KeyboardState PreviousKeyboardState { get; private set; }
    public KeyboardState CurrentKeyboardState { get; private set; }

    public ControllerManager()
    {
        CurrentMouseState = Mouse.GetState();
        PreviousMouseState = CurrentMouseState;

        CurrentKeyboardState = Keyboard.GetState();
        PreviousKeyboardState = CurrentKeyboardState;
    }

    public void Update()
    {
        PreviousMouseState = CurrentMouseState;
        CurrentMouseState = Mouse.GetState();

        PreviousKeyboardState = CurrentKeyboardState;
        CurrentKeyboardState = Keyboard.GetState();
    }

    public int GetMouseScrollDelta()
    {
        return CurrentMouseState.ScrollWheelValue - PreviousMouseState.ScrollWheelValue;
    }

    public bool IsMouseScrollingUp()
    {
        return GetMouseScrollDelta() > 0;
    }

    public bool IsMouseScrollingDown()
    {
        return GetMouseScrollDelta() < 0;
    }

    public bool IsKeyPressed(Keys key)
    {
        return PreviousKeyboardState.IsKeyUp(key)
            && CurrentKeyboardState.IsKeyDown(key);
    }
}
