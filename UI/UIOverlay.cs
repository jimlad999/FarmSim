using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Utils;

namespace UI;

public class UIOverlay
{
    private readonly Dictionary<string, Screen> _screens;
    private readonly UISpriteSheet _uiSpriteSheet;
    private readonly ControllerManager _controllerManager;
    // Updates caused by UI interactions are deferred till next update to avoid unthread safe operations
    // Should just use thread safe collections to avoid this?
    private List<Action> _nextRefresh = new();

    public UIState State;

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

    // Currently only returns first matching id.
    // Is it a good idea to require global ids across all screens?
    public bool TryGetById<T>(string id, out T result) where T : UIElement
    {
        foreach (var screen in _screens.Values)
        {
            if (screen.TryGetById(id, out result))
            {
                return true;
            }
        }

        result = null;
        return false;
    }

    public void Update(GameTime gameTime, List<string> screensToDraw)
    {
        State = new UIState();
        if (_nextRefresh.Count > 0)
        {
            foreach (var action in _nextRefresh)
            {
                action();
            }
            _nextRefresh = new();
        }
        // Only update latest screen so that overlaying a new screen on top doesn't allow the user to interact with the one below
        if (screensToDraw.Count > 0)
        {
            _screens[screensToDraw[^1]].Update(gameTime, State, _uiSpriteSheet, _controllerManager);
        }
    }

    public void Draw(SpriteBatch spriteBatch, List<string> screensToDraw)
    {
        foreach (var screen in screensToDraw)
        {
            _screens[screen].Draw(spriteBatch);
        }
    }

    public void NextRefresh(Action action)
    {
        _nextRefresh.Add(action);
    }
}
