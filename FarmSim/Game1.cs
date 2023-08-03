﻿using FarmSim.Rendering;
using FarmSim.Terrain;
using FarmSim.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Linq;

namespace FarmSim;

public class Game1 : Game
{
    private static readonly Random Rand = new Random();
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private ControllerManager _controllerManager;
    private TerrainManager _terrainManager;
    private ViewportManager _viewportManager;
    private Renderer _renderer;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        _controllerManager = new ControllerManager();
        _terrainManager = new TerrainManager(Rand.Next());
        _viewportManager = ViewportManager.CenteredOnZeroZero(_controllerManager, _graphics);

        for (int y = -10 * 64; y < 10 * 64; y += 64)
            for (int x = -20 * 64; x < 20 * 64; x += 64)
                //if ((x/ 64) % 2 == (y/ 64) % 2)
                    _terrainManager.GetChunk(x, y);
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        var tileset = new[]
        {
            "desert",
            "grass",
            "lava",
            "rock",
            "void",
            "water",
        }.ToDictionary(s => s, s => _graphics.LoadFromFile($"Content/terrain/tilesets/{s}.png"));
        _renderer = new Renderer(_viewportManager, _terrainManager, tileset);
    }

    protected override void Update(GameTime gameTime)
    {
        System.Diagnostics.Debug.WriteLine(("ElapsedGameTime", gameTime.ElapsedGameTime.TotalSeconds));
        if (gameTime.ElapsedGameTime.TotalSeconds > 0.02)
            System.Diagnostics.Debug.WriteLine("=============================================    Running slow    =============================================");

        _controllerManager.Update();
        _viewportManager.Update(gameTime);

        if (_controllerManager.CurrentKeyboardState.IsKeyDown(Keys.Escape))
        {
            Exit();
        }
        if (_controllerManager.IsKeyPressed(Keys.F12))
        {
            _terrainManager.Reseed(Rand.Next());
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin();

        _renderer.Draw(_spriteBatch);

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}