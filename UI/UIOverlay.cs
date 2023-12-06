﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Utils;

namespace UI;

public class UIOverlay
{
    private readonly Dictionary<string, Screen> _screens;
    private readonly UISpriteSheet _uiSpriteSheet;
    private readonly ControllerManager _controllerManager;

    public UIOverlay(
        Dictionary<string, Screen> screens,
        UISpriteSheet uiSpriteSheet,
        ControllerManager controllerManager)
    {
        _screens = screens;
        _uiSpriteSheet = uiSpriteSheet;
        _controllerManager = controllerManager;
    }

    public void Resize()
    {
        foreach (var screen in _screens.Values)
        {
            screen.Resize();
        }
    }

    public void Update(GameTime gameTime, Stack<string> screensToDraw)
    {
        foreach (var screen in screensToDraw)
        {
            _screens[screen].Update(gameTime, _uiSpriteSheet, _controllerManager);
        }
    }

    public void Draw(SpriteBatch spriteBatch, Stack<string> screensToDraw)
    {
        foreach (var screen in screensToDraw)
        {
            _screens[screen].Draw(spriteBatch);
        }
    }
}