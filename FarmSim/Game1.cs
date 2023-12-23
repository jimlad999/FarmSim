using FarmSim.Mobs;
using FarmSim.Player;
using FarmSim.Rendering;
using FarmSim.Terrain;
using FarmSim.UI;
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
using ButtonState = UI.ButtonState;

namespace FarmSim;

public class Game1 : Game
{
    private static readonly Random Rand = new();

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
    private MobManager _mobManager;
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
        GlobalState.TerrainManager = _terrainManager = new TerrainManager(Rand.Next());
        _viewportManager = ViewportManager.CenteredOnZeroZero(_controllerManager, _graphics);
        _graphics.SynchronizeWithVerticalRetrace = false;

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        Text.Normal = Content.Load<SpriteFont>("fonts/GameFont");
        Text.Bold = Content.Load<SpriteFont>("fonts/GameFontBold");
        var fogOfWarEffect = Content.Load<Effect>("shaders/fog-of-war");
        var fogOfWarOverlay = Content.Load<Texture2D>("fog-of-war-overlay");
        var pixel = Content.Load<Texture2D>("pixel");

        GlobalState.BuildingData = JsonConvert.DeserializeObject<BuildingData>(File.ReadAllText("Content/tilesets/buildings/buildings.json"));

        var mobData = JsonConvert.DeserializeObject<MobData[]>(File.ReadAllText("Content/entities/mobs/mobs.json"));
        var tilesetData = JsonConvert.DeserializeObject<TilesetData>(File.ReadAllText("Content/tilesets/tilesets.json"));
        GlobalState.Tileset = _tileset = new Tileset(_spriteBatch, tilesetData);
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
        _screensToDraw.Add("hud");
        _uiOverlay = new UIOverlay(
            screens,
            _uiSpriteSheet,
            _controllerManager);
        // TODO: find a better place for UI interactions to sit (should sit within the Game, i.e. not the library)
        if (_uiOverlay.TryGetById("build-button", out Button buildButton))
        {
            buildButton.EventHandler += (Button sender, ButtonState state, ButtonState previousState) =>
            {
                if (previousState != ButtonState.Pressed && state == ButtonState.Pressed)
                {
                    _uiOverlay.NextRefresh(() => _screensToDraw.Add("buildscreen"));
                }
            };
        }
        if (_uiOverlay.TryGetById("building-selector", out BuildingSelector buildingSelector))
        {
            buildingSelector.EventHandler += (Button sender, ButtonState state, ButtonState previousState) =>
            {
                if (previousState != ButtonState.Pressed && state == ButtonState.Pressed && sender is BuildingSelectorButton buildingSelectorButton)
                {
                    _player.BuildingKey = buildingSelectorButton.BuildingKey;
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
        _viewportManager.UIOverlay = _uiOverlay;
        _terrainManager.UpdateSightInit(tileX: _player.TileX, tileY: _player.TileY, Player.Player.SightRadius);
        _mobManager = new MobManager(mobData, _player, _terrainManager);
        _renderer = new Renderer(
            _viewportManager,
            _terrainManager,
            _tileset,
            _entitySpriteSheet,
            _player,
            _mobManager,
            fogOfWarEffect,
            fogOfWarOverlay,
            pixel);
    }

    protected override void Update(GameTime gameTime)
    {
        _controllerManager.Update();
        _uiOverlay.Update(gameTime, _screensToDraw);
        _viewportManager.Update(gameTime);
        _player.Update(gameTime);
        _mobManager.Update(gameTime);

        if (_controllerManager.IsKeyInitialPressed(Keys.Escape))
        {
            // if not just HUD displayed
            if (_screensToDraw.Count > 1)
            {
                _screensToDraw.RemoveAt(_screensToDraw.Count - 1);
            }
            else if (_player.BuildingKey != null)
            {
                _player.BuildingKey = null;
            }
            else
            {
                Exit();
            }
        }
        if (_controllerManager.IsKeyInitialPressed(Keys.F12))
        {
            _terrainManager.Reseed(Rand.Next());
            _terrainManager.UpdateSightInit(tileX: _player.TileX, tileY: _player.TileY, Player.Player.SightRadius);
            _mobManager.Clear();
            _renderer.ClearLODCache();
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
#if DEBUG
        if (gameTime.ElapsedGameTime.TotalSeconds > 0.02)
            System.Diagnostics.Debug.WriteLine(("Running slow", gameTime.ElapsedGameTime.TotalSeconds));
#endif
        _renderer.Draw(_spriteBatch);
        // should UIOverlay be inside the Renderer instead?
        _spriteBatch.Begin();
        _uiOverlay.Draw(_spriteBatch, _screensToDraw);
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}