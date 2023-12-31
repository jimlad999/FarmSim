using FarmSim.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using UI;
using Utils;

namespace FarmSim.Rendering;

internal class ViewportManager
{
    private const float MaxZoom = 1f;
    private const float MinZoom = 1f / 64f;
    private const float DefaultZoom = 1f / 2f;
    private readonly ControllerManager _controllerManager;
    private double _x;
    private double _y;
    private double _width;
    private double _widthHalf;
    private double _height;
    private double _heightHalf;

    public Viewport ScreenDimensions;
    public float Zoom = 1f;
    public Viewport Viewport;
    public int ScrollSpeed = 500;
    public Player.Player Tracking;
    public UIOverlay UIOverlay;

    public ViewportManager(
        ControllerManager controllerManager,
        Viewport screenDimensions,
        Viewport viewport)
    {
        _controllerManager = controllerManager;

        ScreenDimensions = screenDimensions;
        Viewport = viewport;
        _x = Viewport.X;
        _y = Viewport.Y;
        _width = Viewport.Width;
        _widthHalf = _width / 2;
        _height = Viewport.Height;
        _heightHalf = _height / 2;
    }

    internal void Update(GameTime gameTime)
    {
        bool stateChanged = false;
        var keyboardState = _controllerManager.CurrentKeyboardState;
        var viewportScroll = gameTime.ElapsedGameTime.TotalSeconds * ScrollSpeed / Zoom;
        if (Tracking == null)
        {
            // TODO: consider consolidating controller management in a single location
            // to avoid spreading control management around to different classes.
            if (UIOverlay?.State.IsMouseOverElement == false)
            {
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
            }
        }
        else
        {
            stateChanged = Tracking.X != (_x + _widthHalf) || Tracking.Y != (_y + _heightHalf);
            if (stateChanged)
            {
                _x = Tracking.X - _widthHalf;
                _y = Tracking.Y - _heightHalf;
            }
        }

        if (stateChanged)
        {
            UpdateViewport();
        }
    }

    private void UpdateViewport()
    {
        Viewport = new Viewport(
            x: (int)_x,
            y: (int)_y,
            width: (int)_width,
            height: (int)_height);
    }

    public void ZoomIn()
    {
        if (Zoom <= MinZoom)
        {
            return;
        }
        Zoom /= 2;
        if (Zoom < MinZoom) Zoom = MinZoom;
        _x -= _width / 2;
        _y -= _height / 2;
        _width *= 2;
        _widthHalf = _width / 2;
        _height *= 2;
        _heightHalf = _height / 2;
        UpdateViewport();
    }

    public void ZoomOut()
    {
        if (Zoom >= MaxZoom)
        {
            return;
        }
        Zoom *= 2;
        if (Zoom > MaxZoom) Zoom = MaxZoom;
        _x += _width / 4;
        _y += _height / 4;
        _width /= 2;
        _widthHalf = _width / 2;
        _height /= 2;
        _heightHalf = _height / 2;
        UpdateViewport();
    }

    public (int X, int Y) ConvertWorldCoordinatesToScreenCoordinates(int worldX, int worldY)
    {
        return (
            X: (int)((worldX - Viewport.X) * Zoom),
            Y: (int)((worldY - Viewport.Y) * Zoom)
        );
    }

    public (int X, int Y) ConvertScreenCoordinatesToWorldCoordinates(int screenX, int screenY)
    {
        return (
            X: (int)(screenX / Zoom + Viewport.X),
            Y: (int)(screenY / Zoom + Viewport.Y)
        );
    }

    public (int X, int Y) ConvertScreenCoordinatesToTileCoordinates(int screenX, int screenY)
    {
        return (
            X: (int)Math.Floor((screenX / Zoom + Viewport.X) / Renderer.TileSize),
            Y: (int)Math.Floor((screenY / Zoom + Viewport.Y) / Renderer.TileSize)
        );
    }

    public (int xOffset, int yOffset, int xTileStart, int yTileStart, int xTileEnd, int yTileEnd) GetTileDimensions(int minBuffer, int maxBuffer)
    {
        var xOffset = Viewport.X.Mod(Renderer.TileSize);
        var yOffset = Viewport.Y.Mod(Renderer.TileSize);
        var xTileStart = (Viewport.X - xOffset) / Renderer.TileSize - minBuffer;
        var yTileStart = (Viewport.Y - yOffset) / Renderer.TileSize - minBuffer;
        var xTileEnd = (Viewport.X + Viewport.Width) / Renderer.TileSize + maxBuffer;
        var yTileEnd = (Viewport.Y + Viewport.Height) / Renderer.TileSize + maxBuffer;
        return (xOffset, yOffset, xTileStart, yTileStart, xTileEnd, yTileEnd);
    }

    public static ViewportManager CenteredOnZeroZero(ControllerManager controllerManager, GraphicsDeviceManager graphics)
    {
        var width = (int)(graphics.PreferredBackBufferWidth / DefaultZoom);
        var height = (int)(graphics.PreferredBackBufferHeight / DefaultZoom);
        return new ViewportManager(
            controllerManager,
            screenDimensions: new Viewport(
                x: 0,
                y: 0,
                width: graphics.PreferredBackBufferWidth,
                height: graphics.PreferredBackBufferHeight),
            viewport: new Viewport(
                x: -width / 2,
                y: -height / 2,
                width: width,
                height: height))
        {
            Zoom = DefaultZoom
        };
    }
}
