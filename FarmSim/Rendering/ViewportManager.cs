using FarmSim.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace FarmSim.Rendering;

internal class ViewportManager
{
    private const float MaxZoom = 1f;
    private const float MinZoom = 1f / 64f;
    private readonly ControllerManager _controllerManager;
    private double _x;
    private double _y;
    private double _width;
    private double _height;

    public float Zoom { get; private set; } = 1f;
    public Viewport Viewport { get; private set; }
    public int ScrollSpeed { get; set; } = 500;

    public ViewportManager(
        ControllerManager controllerManager,
        Viewport viewport)
    {
        _controllerManager = controllerManager;

        Viewport = viewport;
        _x = Viewport.X;
        _y = Viewport.Y;
        _width = Viewport.Width;
        _height = Viewport.Height;
    }

    internal void Update(GameTime gameTime)
    {
        bool stateChanged = false;
        var keyboardState = _controllerManager.CurrentKeyboardState;
        var viewportScroll = gameTime.ElapsedGameTime.TotalSeconds * ScrollSpeed / Zoom;
        if (keyboardState.IsKeyDown(Keys.Up))
        {
            _y -= viewportScroll;
            stateChanged = true;
        }
        if (keyboardState.IsKeyDown(Keys.Down))
        {
            _y += viewportScroll;
            stateChanged = true;
        }
        if (keyboardState.IsKeyDown(Keys.Left))
        {
            _x -= viewportScroll;
            stateChanged = true;
        }
        if (keyboardState.IsKeyDown(Keys.Right))
        {
            _x += viewportScroll;
            stateChanged = true;
        }

        if (Zoom < MaxZoom && _controllerManager.IsMouseScrollingUp())
        {
            Zoom *= 2;
            if (Zoom > MaxZoom) Zoom = MaxZoom;
            _x += _width / 4;
            _y += _height / 4;
            _width /= 2;
            _height /= 2;
            stateChanged = true;
        }
        else if (Zoom > MinZoom && _controllerManager.IsMouseScrollingDown())
        {
            Zoom /= 2;
            if (Zoom < MinZoom) Zoom = MinZoom;
            _x -= _width / 2;
            _y -= _height / 2;
            _width *= 2;
            _height *= 2;
            stateChanged = true;
        }

        if (stateChanged)
        {
            Viewport = new Viewport(
                x: (int)_x,
                y: (int)_y,
                width: (int)_width,
                height: (int)_height);
        }
    }

    internal (int X, int Y) ConvertScrenCoordinatesToTileCoordinates(int screenX, int screenY)
    {
        return (
            X: (int)Math.Floor((screenX / Zoom + Viewport.X) / Renderer.TileSize),
            Y: (int)Math.Floor((screenY / Zoom + Viewport.Y) / Renderer.TileSize)
        );
    }

    public static ViewportManager CenteredOnZeroZero(ControllerManager controllerManager, GraphicsDeviceManager graphics)
    {
        var width = (int)(graphics.PreferredBackBufferWidth / MinZoom);
        var height = (int)(graphics.PreferredBackBufferHeight / MinZoom);
        return new ViewportManager(
            controllerManager,
            new Viewport(
                x: -width / 2,
                y: -height / 2,
                width: width,
                height: height))
        {
            Zoom = MinZoom
        };
    }
}
