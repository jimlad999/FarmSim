using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Utils;

public class ControllerManager
{
    private const double InitialDebounce = 0.7;//seconds
    private const double RepeatDebounce = 0.2;//seconds
    private readonly Dictionary<Keys, (double Time, bool Repeated)> HeldKeys = new();
    public MouseState PreviousMouseState;
    public MouseState CurrentMouseState;
    public KeyboardState PreviousKeyboardState;
    public KeyboardState CurrentKeyboardState;

    public ControllerManager()
    {
        CurrentMouseState = Mouse.GetState();
        PreviousMouseState = CurrentMouseState;

        CurrentKeyboardState = Keyboard.GetState();
        PreviousKeyboardState = CurrentKeyboardState;
    }

    public void Update(GameTime gameTime)
    {
        PreviousMouseState = CurrentMouseState;
        CurrentMouseState = Mouse.GetState();

        PreviousKeyboardState = CurrentKeyboardState;
        CurrentKeyboardState = Keyboard.GetState();
        var pressedKeys = CurrentKeyboardState.GetPressedKeys();
        if (pressedKeys.Length > 0)
        {
            foreach (var key in pressedKeys)
            {
                if (HeldKeys.TryGetValue(key, out var value))
                {
                    HeldKeys[key] = (value.Time + gameTime.ElapsedGameTime.TotalSeconds, value.Repeated);
                }
                else
                {
                    HeldKeys[key] = (0.0, false);
                }
            }
        }
        else if (HeldKeys.Count > 0)
        {
            HeldKeys.Clear();
        }
    }

    public bool IsKeyInitialPressed(Keys key)
    {
        return PreviousKeyboardState.IsKeyUp(key)
            && CurrentKeyboardState.IsKeyDown(key);
    }

    public bool IsKeyPressedWithRepeat(Keys key)
    {
        if (!HeldKeys.TryGetValue(key, out var value))
        {
            return false;
        }
        else if (PreviousKeyboardState.IsKeyUp(key))
        {
            return true;
        }
        else if (value.Repeated)
        {
            if (value.Time >= RepeatDebounce)
            {
                HeldKeys[key] = (0.0, true);
                return true;
            }
        }
        else
        {
            if (value.Time >= InitialDebounce)
            {
                HeldKeys[key] = (0.0, true);
                return true;
            }
        }
        return false;
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
