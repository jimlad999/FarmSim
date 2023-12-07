using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Runtime.Serialization;
using Utils;

namespace UI;

[DataContract]
public class Screen
{
    [DataMember]
    public UIElement[] UIElements = Array.Empty<UIElement>();

    public void Resize()
    {
        foreach (var uiElement in UIElements)
        {
            uiElement.Resize();
        }
    }

    public bool TryGetById(string id, out UIElement result)
    {
        foreach (var uiElement in UIElements)
        {
            if (uiElement.TryGetById(id, out result))
            {
                return true;
            }
        }

        result = null;
        return false;
    }

    public void Update(
        GameTime gameTime,
        UISpriteSheet uiSpriteSheet,
        ControllerManager controllerManager)
    {
        foreach (var uiElement in UIElements)
        {
            uiElement.Update(gameTime, uiSpriteSheet, controllerManager);
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        foreach (var uiElement in UIElements)
        {
            uiElement.Draw(spriteBatch, drawArea: spriteBatch.GraphicsDevice.Viewport.TitleSafeArea);
        }
    }
}