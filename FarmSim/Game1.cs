using FarmSim.Rendering;
using FarmSim.Terrain;
using FarmSim.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UI;
using UI.Data;
using Utils;

namespace FarmSim;

public class Game1 : Game
{
    private static readonly Random Rand = new Random();

    private readonly List<string> _screensToDraw = new();

    private GraphicsDeviceManager _graphics;
    private ControllerManager _controllerManager;
    private TerrainManager _terrainManager;
    private ViewportManager _viewportManager;
    private UIOverlay _uiOverlay;
    private Player.Player _player;
    private SpriteBatch _spriteBatch;
    private Tileset _tileset;
    private EntitySpriteSheet _entitySpriteSheet;
    private UISpriteSheet _uiSpriteSheet;
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
                _terrainManager.GetChunk(x, y);
        base.Initialize();
    }

    protected override void LoadContent()
    {
        Text.Normal = Content.Load<SpriteFont>("fonts/GameFont");
        Text.Bold = Content.Load<SpriteFont>("fonts/GameFontBold");
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        var tilesetData = JsonConvert.DeserializeObject<TilesetData>(File.ReadAllText("Content/tilesets/tilesets.json"));
        _tileset = new Tileset(_spriteBatch, tilesetData);
        var entitiesData = JsonConvert.DeserializeObject<EntitiesData>(File.ReadAllText("Content/entities/entities.json"));
        _entitySpriteSheet = new EntitySpriteSheet(_spriteBatch, entitiesData);
        var uiSpriteData = JsonConvert.DeserializeObject<UISpriteData>(File.ReadAllText("Content/ui/ui.json"));
        _uiSpriteSheet = new UISpriteSheet(_spriteBatch, uiSpriteData);
        var screenData = JsonConvert.DeserializeObject<ScreensData>(File.ReadAllText("Content/ui/screens.json"));
        var screens = screenData.Screens
            .Select(s => (
                s.ScreenName,
                Screen: JsonConvert.DeserializeObject<Screen>(File.ReadAllText($"{screenData.BaseFolder}/{s.ScreenFilename}"), new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto
                })
            ))
            .ToDictionary(a => a.ScreenName, a => a.Screen);
        //debug
        _screensToDraw.Add("hud");
        _uiOverlay = new UIOverlay(
            screens,
            _uiSpriteSheet,
            _controllerManager);
        // TODO: find a better place for UI interactions to sit (should sit within the Game, i.e. not the library)
        if (_uiOverlay.TryGetById<Button>("build-button", out var buildButton))
        {
            buildButton.EventHandler += (Button sender, Button.ButtonState state, Button.ButtonState previousState) =>
            {
                if (previousState != Button.ButtonState.Pressed && state == Button.ButtonState.Pressed)
                {
                    _uiOverlay.NextRefresh(() => _screensToDraw.Add("buildscreen"));
                }
            };
        }
        if (_uiOverlay.TryGetById<Button>("close-button", out var closeButton))
        {
            closeButton.EventHandler += (Button sender, Button.ButtonState state, Button.ButtonState previousState) =>
            {
                if (previousState != Button.ButtonState.Pressed && state == Button.ButtonState.Pressed)
                {
                    _uiOverlay.NextRefresh(() => _screensToDraw.Remove("buildscreen"));
                }
            };
        }
        _player = new Player.Player(
            _controllerManager,
            _viewportManager,
            _terrainManager,
            _tileset,
            _uiOverlay);
        _viewportManager.Tracking = _player;
        _renderer = new Renderer(
            _viewportManager,
            _terrainManager,
            _tileset,
            _entitySpriteSheet,
            _player);
    }

    protected override void Update(GameTime gameTime)
    {
        _controllerManager.Update();
        _uiOverlay.Update(gameTime, _screensToDraw);
        _viewportManager.Update(gameTime);
        _player.Update(gameTime);

        if (_controllerManager.IsKeyInitialPressed(Keys.Escape))
        {
            Exit();
        }
        if (_controllerManager.IsKeyInitialPressed(Keys.F12))
        {
            _terrainManager.Reseed(Rand.Next());
            _renderer.ClearLODCache();
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
#if DEBUG
        if (_controllerManager.IsLeftMouseDown())
        {
            var (x, y) = _viewportManager.ConvertScrenCoordinatesToTileCoordinates(screenX: _controllerManager.CurrentMouseState.X, screenY: _controllerManager.CurrentMouseState.Y);
            var climateNoiseVal = _terrainManager._terrainGenerator.GetClimateNoiseVal(x, y);
            var regionNoiseVal = _terrainManager._terrainGenerator.GetRegionNoiseVal(x, y);
            var tileNoiseVal = _terrainManager._terrainGenerator.GetTileNoiseVal(x, y);
            System.Diagnostics.Debug.WriteLine((
                $"(x: {x}, y: {y})",
                //$"(screenX: {_controllerManager.CurrentMouseState.X}, screenY: {_controllerManager.CurrentMouseState.Y})",
                //_viewportManager.Viewport,
                (Math.Sqrt(TerrainGenerator.DistanceSquaredFromCenterOfContinent(x, y)), x.Mod(256), x % 256),
                ("region noise", regionNoiseVal, TerrainGenerator.GetIntercontinentalRegionType(regionNoiseVal: regionNoiseVal, climateNoiseVal: climateNoiseVal)),
                ("tile noise", tileNoiseVal)
            ));
        }
        else
        {
            System.Diagnostics.Debug.WriteLine(("ElapsedGameTime", gameTime.ElapsedGameTime.TotalSeconds));
            if (gameTime.ElapsedGameTime.TotalSeconds > 0.02)
                System.Diagnostics.Debug.WriteLine("=============================================    Running slow    =============================================");
        }
#endif
        _renderer.Draw(_spriteBatch);

        // should UIOverlay be inside the Renderer instead?
        _spriteBatch.Begin();
        _uiOverlay.Draw(_spriteBatch, _screensToDraw);
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}