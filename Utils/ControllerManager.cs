using Microsoft.Xna.Framework.Input;

namespace Utils;

public class ControllerManager
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

    public bool IsKeyInitialPressed(Keys key)
    {
        return PreviousKeyboardState.IsKeyUp(key)
            && CurrentKeyboardState.IsKeyDown(key);
    }

    public bool IsKeyDown(Keys key)
    {
        return CurrentKeyboardState.IsKeyDown(key);
    }

    public bool IsKeyUp(Keys key)
    {
        return CurrentKeyboardState.IsKeyUp(key);
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

    public bool IsLeftMouseInitialPressed()
    {
        return CurrentMouseState.LeftButton == ButtonState.Pressed
            && PreviousMouseState.LeftButton == ButtonState.Released;
    }

    public bool IsLeftMouseDown()
    {
        return CurrentMouseState.LeftButton == ButtonState.Pressed;
    }

    public bool IsLeftMouseUp()
    {
        return CurrentMouseState.LeftButton == ButtonState.Released;
    }
}
