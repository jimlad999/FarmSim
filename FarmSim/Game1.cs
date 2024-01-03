using FarmSim.Entities;
using FarmSim.Mobs;
using FarmSim.Player;
using FarmSim.Projectiles;
using FarmSim.Rendering;
using FarmSim.Terrain;
using FarmSim.UI;
using FarmSim.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.BitmapFonts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UI;
using UI.Data;
using Utils;
using Utils.Rendering;
using ButtonState = UI.ButtonState;
using Effect = Microsoft.Xna.Framework.Graphics.Effect;

namespace FarmSim;

public class Game1 : Game
{
    private static readonly Random Rand = new();

    private readonly List<string> _screensToDraw = new();

    private GraphicsDeviceManager _graphics;
    private ControllerManager _controllerManager;
    private UIOverlay _uiOverlay;
    private SpriteBatch _spriteBatch;
    private Renderer _renderer;
    private TextInput _commandInput;
    private ActionBar _actionBar;
    private EntityContextMenu _contextMenu;
    private TabContainer _inventory;
    private InventoryTab _allInventoryTab;
    private bool _debugArcRange;
    private Effect _debugArcRangeEffect;
    private RenderTarget2D _entireScreen;
    private CustomMouseCursor _mousePointer;

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
        GlobalState.ViewportManager = ViewportManager.CenteredOnZeroZero(_controllerManager, _graphics);
        _graphics.SynchronizeWithVerticalRetrace = false;

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        Text.Normal = Content.Load<BitmapFont>("fonts/normal-32");
        Text.Bold = Content.Load<BitmapFont>("fonts/bold-32");
        Text.Small = Content.Load<BitmapFont>("fonts/normal-18");
        Text.Medium = Content.Load<BitmapFont>("fonts/normal-24");
        _mousePointer = new CustomMouseCursor(
            bucket: Content.Load<Texture2D>("ui/pointers/pointer-bucket"),
            chop: Content.Load<Texture2D>("ui/pointers/pointer-chop"),
            farm: Content.Load<Texture2D>("ui/pointers/pointer-farm"),
            harvest: Content.Load<Texture2D>("ui/pointers/pointer-harvest"),
            mine: Content.Load<Texture2D>("ui/pointers/pointer-mine"),
            projectile: Content.Load<Texture2D>("ui/pointers/pointer-projectile"),
            slash: Content.Load<Texture2D>("ui/pointers/pointer-slash"));
        var fogOfWarEffect = Content.Load<Effect>("shaders/fog-of-war");
        var fogOfWarInverseEffect = Content.Load<Effect>("shaders/fog-of-war-inverse");
        var outlineTileEffect = Content.Load<Effect>("shaders/outline-tile");
        var outlineEntityEffect = Content.Load<Effect>("shaders/outline-entity");
        _debugArcRangeEffect = Content.Load<Effect>("shaders/debug-arc-range");
        _entireScreen = new RenderTarget2D(
            _spriteBatch.GraphicsDevice,
            width: _spriteBatch.GraphicsDevice.Viewport.Width,
            height: _spriteBatch.GraphicsDevice.Viewport.Height);
        var pixel = ColoredPanel.Pixel = Content.Load<Texture2D>("pixel");

        GlobalState.AnimationManager = new AnimationManager();

        var resourceData = JsonConvert.DeserializeObject<Dictionary<string, ResourceData>>(File.ReadAllText("Content/entities/items/resources.json"));
        var itemData = GlobalState.ItemData = JsonConvert.DeserializeObject<ItemData[]>(File.ReadAllText("Content/entities/items/items.json"))
            .ToDictionary(i => i.Id);
        var mobData = JsonConvert.DeserializeObject<MobData[]>(File.ReadAllText("Content/entities/mobs/mobs.json"));
        var tilesetData = JsonConvert.DeserializeObject<TilesetData>(File.ReadAllText("Content/tilesets/tilesets.json"));
        var tileset = GlobalState.Tileset = new Tileset(_spriteBatch, tilesetData);
        foreach (var value in tilesetData.Data)
        {
            GlobalState.AnimationManager.GenerateTilesetAnimation(value.Key, value.Value);
        }
        var entitiesData = GlobalState.EntitiesData = JsonConvert.DeserializeObject<EntitiesData>(File.ReadAllText("Content/entities/entities.json"));
        var entitySpriteSheet = GlobalState.EntitySpriteSheet = new EntitySpriteSheet(_spriteBatch, entitiesData);
        foreach (var value in entitiesData.Data)
        {
            GlobalState.AnimationManager.GenerateGlobalEntityAnimation(value.Key, value.Value);
        }
        GlobalState.BuildingData = JsonConvert.DeserializeObject<BuildingData>(File.ReadAllText("Content/tilesets/buildings/buildings.json"));
        // Must be initialized before UI is created. Dependency is UI on building animations being set up (see BuildingSelectorButton).
        foreach (var building in GlobalState.BuildingData.Buildings.Values)
        {
            building.InitAnimations(tilesetData);
        }

        var uiSpriteData = JsonConvert.DeserializeObject<UISpriteData>(File.ReadAllText("Content/ui/ui.json"));
        var uiSpriteSheet = new UISpriteSheet(_spriteBatch, uiSpriteData);
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
            uiSpriteSheet,
            _controllerManager);
        GlobalState.ConsolidatedZoningData = new();
        foreach (var buildableData in tilesetData.Data)
        {
            GlobalState.ConsolidatedZoningData.Add(buildableData.Key, buildableData.Value);
        }
        foreach (var buildableData in entitiesData.Data)
        {
            GlobalState.ConsolidatedZoningData.Add(buildableData.Key, buildableData.Value);
        }
        GlobalState.TerrainManager = new TerrainManager(Rand.Next(), resourceData);
        GlobalState.ItemManager = new ItemManager(itemData, entitiesData.Data);
        GlobalState.MobManager = new MobManager(mobData, entitiesData.Data);
        GlobalState.ProjectileManager = new ProjectileManager(entitiesData.Data);
        GlobalState.PlayerManager = new PlayerManager();

        var player = GlobalState.PlayerManager.ActivePlayer = new Player.Player(
            // TODO: Pull this from save state (once saving has been implemented)
            new Inventory(new()),
            _controllerManager,
            _uiOverlay);
        GlobalState.PlayerManager.AddPlayer(GlobalState.PlayerManager.ActivePlayer);
        GlobalState.ViewportManager.Tracking = player;
        GlobalState.ViewportManager.UIOverlay = _uiOverlay;
#if DEBUG
        Renderer.RenderFogOfWar = false;
        Renderer.RenderTelescopedPlayerAction = true;
#endif
        _renderer = new Renderer(
            tileset,
            entitySpriteSheet,
            fogOfWarEffect,
            fogOfWarInverseEffect,
            outlineTileEffect,
            outlineEntityEffect,
            pixel);

        // TODO: find a better place for UI interactions to sit (should sit within the Game, i.e. not the library)
        _allInventoryTab = new InventoryTab(GlobalState.PlayerManager.ActivePlayer.Inventory, Array.Empty<Tags>(), text: "All");
        _uiOverlay.TryGetById("inventory", out _inventory);
        if (_uiOverlay.TryGetById("entity-context-menu", out _contextMenu))
        {
            player.ContextMenu = _contextMenu;
            EntityContextMenu.Follow.EventHandler += (Button sender, ButtonState state, ButtonState previousState) =>
            {
                if (previousState != ButtonState.Pressed && state == ButtonState.Pressed)
                {
                    if (_contextMenu.TrackingEntity is Mob mob)
                    {
                        // TODO: Add more behaviours (e.g. attack nearby aggressive mobs)
                        mob.AddBehaviours(player, new FollowActivePlayerBehaviour());
                    }
                    _uiOverlay.NextRefresh(() => _contextMenu.Clear());
                }
            };
            EntityContextMenu.StopFollowing.EventHandler += (Button sender, ButtonState state, ButtonState previousState) =>
            {
                if (previousState != ButtonState.Pressed && state == ButtonState.Pressed)
                {
                    if (_contextMenu.TrackingEntity is Mob mob)
                    {
                        // Should match EntityContextMenu.Follow
                        mob.RemoveBehaviours<FollowActivePlayerBehaviour>(player);
                    }
                    _uiOverlay.NextRefresh(() => _contextMenu.Clear());
                }
            };
            EntityContextMenu.Feed.EventHandler += (Button sender, ButtonState state, ButtonState previousState) =>
            {
                if (previousState != ButtonState.Pressed && state == ButtonState.Pressed)
                {
                    if (_contextMenu.TrackingEntity is Mob mob)
                    {
                        // TODO: update to better UX allowing user to select which food to give.
                        // Then move this to the commit action of that UI instead. Here would simply enable showing the new UI mentioned above.
                        var itemToFeed = player.Inventory.FirstOrDefault();
                        if (itemToFeed != null)
                        {
                            player.Inventory.RemoveItem(itemToFeed);
                            if (mob.Feed(player, itemToFeed))
                            {
                                GlobalState.MobManager.Tame(mob, player);
                            }
                        }
                    }
                    _uiOverlay.NextRefresh(() => _contextMenu.Clear());
                }
            };
            EntityContextMenu.Work.EventHandler += (Button sender, ButtonState state, ButtonState previousState) =>
            {
                if (previousState != ButtonState.Pressed && state == ButtonState.Pressed)
                {
                    _uiOverlay.NextRefresh(() => _contextMenu.Clear());
                }
            };
        }
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
                    GlobalState.PlayerManager.ActivePlayer.BuildingKey = buildingSelectorButton.BuildingKey;
                    _uiOverlay.NextRefresh(() => _screensToDraw.Remove("buildscreen"));
                }
            };
        }
        if (_uiOverlay.TryGetById("action-bar", out _actionBar))
        {
            var actionButton1 = (ActionButton)_actionBar.Children[0];
            actionButton1.SetOption(new ActionIcon("multi-tool-attack", (Button sender, ButtonState state, ButtonState previousState) =>
            {
                if (previousState != ButtonState.Pressed && state == ButtonState.Pressed)
                {
                    GlobalState.PlayerManager.ActivePlayer.PrimaryAction = new MultiToolAction();
                }
            }));
            // make default selection for player
            actionButton1.Select();
            // Manually set SelectedButton since the event listeners won't be attached yet.
            // Once the Update(GameTime) runs once the event listeners will be attached.
            _actionBar.SelectedButton = actionButton1;
            ((ActionButton)_actionBar.Children[1]).SetOption(new ActionIcon("magic-missile", (Button sender, ButtonState state, ButtonState previousState) =>
            {
                if (previousState != ButtonState.Pressed && state == ButtonState.Pressed)
                {
                    GlobalState.PlayerManager.ActivePlayer.PrimaryAction = new FireProjectileAction();
                }
            }));
        }
        if (_uiOverlay.TryGetById("command-input", out _commandInput)
            && _uiOverlay.TryGetById("command-log", out Log commandLog))
        {
            _commandInput.EventHandler += (TextInput sender, string value) =>
            {
                value = value.Trim();
                var logOutput = new List<(string, Log.Level)>
                {
                    (value, Log.Level.Info)
                };
                if (value == "/?")
                {
                    logOutput.Add(("Commands:", Log.Level.Debug));
                    logOutput.Add((" /SET", Log.Level.Debug));
                    logOutput.Add((" /SPAWN", Log.Level.Debug));
                }
                else if (value.StartsWith("/SET ", StringComparison.OrdinalIgnoreCase))
                {
                    var setOperation = value.Substring(5, value.Length - 5).Replace(" ", string.Empty);
                    if (setOperation == "/?")
                    {
                        logOutput.Add(("SET options:", Log.Level.Debug));
                        logOutput.Add((" RenderFogOfWar : bool", Log.Level.Debug));
                        logOutput.Add((" DebugArcRange : bool", Log.Level.Debug));
                    }
                    else if (setOperation.StartsWith("RenderFogOfWar=", StringComparison.OrdinalIgnoreCase))
                    {
                        Renderer.RenderFogOfWar = setOperation.EndsWith("true", StringComparison.OrdinalIgnoreCase);
                        logOutput.Add(($"Fog of war {(Renderer.RenderFogOfWar ? "enabled" : "disabled")}.", Log.Level.Debug));
                    }
                    else if (setOperation.StartsWith("RenderTelescopedPlayerAction=", StringComparison.OrdinalIgnoreCase))
                    {
                        Renderer.RenderTelescopedPlayerAction = setOperation.EndsWith("true", StringComparison.OrdinalIgnoreCase);
                        logOutput.Add(($"Telescoping player action {(Renderer.RenderTelescopedPlayerAction ? "enabled" : "disabled")}.", Log.Level.Debug));
                    }
                    else if (setOperation.StartsWith("DebugArcRange=", StringComparison.OrdinalIgnoreCase))
                    {
                        _debugArcRange = setOperation.EndsWith("true", StringComparison.OrdinalIgnoreCase);
                        logOutput.Add(($"Debug arc range {(_debugArcRange ? "enabled" : "disabled")}.", Log.Level.Debug));
                    }
                    else
                    {
                        logOutput.Add(("Unknow SET operation. Type SET /? for help...", Log.Level.Debug));
                    }
                }
                else if (value.StartsWith("/SPAWN", StringComparison.OrdinalIgnoreCase))
                {
                    var spawnOperation = value.Substring(6, value.Length - 6).Trim();

                    if (!int.TryParse(spawnOperation, out var spawnCount))
                    {
                        if (string.IsNullOrEmpty(spawnOperation))
                        {
                            spawnCount = 1;
                        }
                    }
                    if (spawnCount == 0)
                    {
                        logOutput.Add(("Spawn command not recognised.", Log.Level.Debug));
                        logOutput.Add(("Usage:", Log.Level.Debug));
                        logOutput.Add((" /SPAWN [count]", Log.Level.Debug));
                    }
                    else
                    {
                        for (int i = 0; i < spawnCount; ++i)
                        {
                            GlobalState.MobManager.SpawnMobs();
                            GlobalState.MobManager.ResetSpawnWaitTime();
                        }
                        logOutput.Add(("Spawned mobs around player.", Log.Level.Debug));
                    }
                }
                else
                {
                    logOutput.Add(("Unknow command. Type /? for help...", Log.Level.Debug));
                }
                _uiOverlay.NextRefresh(() =>
                {
                    commandLog.PushText(logOutput);
                });
            };
        }
    }

    protected override void Update(GameTime gameTime)
    {
        _controllerManager.Update(gameTime);
        if (_controllerManager.IsKeyInitialPressed(Keys.Escape))
        {
            // if not just HUD displayed
            if (_screensToDraw.Count > 1)
            {
                if (_screensToDraw.Contains("command-console") && _commandInput.Value.Length > 0)
                {
                    _commandInput.Clear();
                }
                else
                {
                    _screensToDraw.RemoveAt(_screensToDraw.Count - 1);
                }
            }
            else if (GlobalState.PlayerManager.ActivePlayer.BuildingKey != null)
            {
                GlobalState.PlayerManager.ActivePlayer.BuildingKey = null;
            }
            else if (_contextMenu.TrackingEntity != null)
            {
                _contextMenu.Clear();
            }
            else
            {
                Exit();
            }
        }
        else if (_controllerManager.IsRightMouseInitialPressed())
        {
            if (GlobalState.PlayerManager.ActivePlayer.BuildingKey != null)
            {
                GlobalState.PlayerManager.ActivePlayer.BuildingKey = null;
            }
            // else (see) Player.InvokeSecondaryAction
        }
        else if (_controllerManager.IsLeftMouseInitialPressed())
        {
            // player left click outside any UI element then they are probably not interacting with the context menu and it should be closed
            if (_contextMenu.TrackingEntity != null && !_uiOverlay.State.IsMouseOverElement)
            {
                _contextMenu.Clear();
            }
            // else (see) Player.InvokeAction
        }
        else if (_controllerManager.IsKeyInitialPressed(Keys.I))
        {
            _allInventoryTab.RefreshInventory();
            _inventory.Tabs.SetButtons(_allInventoryTab);
            _screensToDraw.Add("inventory");
        }
        else if ((_controllerManager.IsKeyInitialPressed(Keys.OemTilde) || _controllerManager.IsKeyInitialPressed(Keys.OemQuestion)) && !_screensToDraw.Contains("command-console"))
        {
            _screensToDraw.Add("command-console");
            // let '/' registery as a key press for the command console
            if (_controllerManager.IsKeyInitialPressed(Keys.OemTilde))
            {
                _commandInput.IgnoreLastKeyPress();
            }
        }
        else if (_controllerManager.IsScrolling(out var scrollResult))
        {
            if (_controllerManager.IsKeyDown(Keys.LeftControl) || _controllerManager.IsKeyDown(Keys.RightControl))
            {
                if (scrollResult.ScrollDirection == ControllerManager.ScrollDirection.Up)
                {
                    GlobalState.ViewportManager.ZoomOut();
                }
                else if (scrollResult.ScrollDirection == ControllerManager.ScrollDirection.Down)
                {
                    GlobalState.ViewportManager.ZoomIn();
                }
            }
            else if (scrollResult.ScrollDirection == ControllerManager.ScrollDirection.Up)
            {
                _actionBar.CycleNext();
            }
            else if (scrollResult.ScrollDirection == ControllerManager.ScrollDirection.Down)
            {
                _actionBar.CyclePrevious();
            }
        }
        else if (_controllerManager.IsKeyInitialPressed(Keys.F12))
        {
            GlobalState.AnimationManager.Clear();
            GlobalState.TerrainManager.Reseed(Rand.Next());
            EntityManager.Reset();
            _renderer.ClearLODCache();
        }

        _uiOverlay.Update(gameTime, _screensToDraw);
        GlobalState.ViewportManager.Update(gameTime);
        EntityManager.Update(gameTime);
        GlobalState.AnimationManager.Update(gameTime);
        _mousePointer.Update(_uiOverlay);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
#if DEBUG
        if (gameTime.ElapsedGameTime.TotalSeconds > 0.02)
            System.Diagnostics.Debug.WriteLine(("Running slow", gameTime.ElapsedGameTime.TotalSeconds));
        if (_debugArcRange)
        {
            using (RenderTargetScope.Create(_spriteBatch, _entireScreen, begin: false))
            {
                _renderer.Draw(_spriteBatch);
            }
            var halfScreenWidth = _spriteBatch.GraphicsDevice.Viewport.Width / 2f;
            var halfScreenHeight = _spriteBatch.GraphicsDevice.Viewport.Height / 2f;
            var weaponRange = GlobalState.PlayerManager.ActivePlayer.GetWeaponRange(out var xOffset, out var yOffset);
            _debugArcRangeEffect.Parameters["HalfScreenWidth"].SetValue(halfScreenWidth);
            _debugArcRangeEffect.Parameters["HalfScreenHeight"].SetValue(halfScreenHeight);
            _debugArcRangeEffect.Parameters["Scale"].SetValue(GlobalState.ViewportManager.Zoom);
            _debugArcRangeEffect.Parameters["XOffset"].SetValue((xOffset + (int)(weaponRange.FacingDirection.X * 30)) * GlobalState.ViewportManager.Zoom);
            _debugArcRangeEffect.Parameters["YOffset"].SetValue((yOffset - (int)(weaponRange.FacingDirection.Y * 30)) * GlobalState.ViewportManager.Zoom);
            _debugArcRangeEffect.Parameters["ReachPow2"].SetValue(weaponRange.ReachPow2 * GlobalState.ViewportManager.Zoom);
            _debugArcRangeEffect.Parameters["ArcCrosses0"].SetValue(weaponRange.ArcCrosses0);
            _debugArcRangeEffect.Parameters["FacingDirectionRadiansMin"].SetValue((float)weaponRange.FacingDirectionRadiansMin);
            _debugArcRangeEffect.Parameters["FacingDirectionRadiansMax"].SetValue((float)weaponRange.FacingDirectionRadiansMax);
            var mouseDirection = new Vector2(
                x: _controllerManager.CurrentMouseState.X - halfScreenWidth + xOffset * GlobalState.ViewportManager.Zoom,
                y: _controllerManager.CurrentMouseState.Y - halfScreenHeight - yOffset * GlobalState.ViewportManager.Zoom);
            mouseDirection.Normalize();
            _spriteBatch.Begin(blendState: BlendState.AlphaBlend, effect: _debugArcRangeEffect);
            _spriteBatch.Draw(_entireScreen, _entireScreen.Bounds, Color.White);
            _spriteBatch.End();
        }
        else
#endif
        {
            _renderer.Draw(_spriteBatch);
        }
        // should UIOverlay be inside the Renderer instead?
        _spriteBatch.Begin(blendState: BlendState.NonPremultiplied);
        _uiOverlay.Draw(_spriteBatch, _screensToDraw);
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}