using FarmSim.Entities;
using FarmSim.Mobs;
using FarmSim.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Linq;
using UI;
using Utils;

namespace FarmSim.Player;

class Player : Entity, IHasMultiTool, IHasInventory
{
    public const int SightRadiusTiles = 12;
    public const int SightRadiusWorld = SightRadiusTiles * Renderer.TileSize;
    public const int SightRadiusWorldPow2 = SightRadiusWorld * SightRadiusWorld;
    private const double MovementSpeed = 200;
    public const int PickUpDistancePow2Const = Renderer.TileSize * Renderer.TileSize;//pick up within 1 tile
    public int PickUpDistancePow2 => PickUpDistancePow2Const;

    public Inventory Inventory { get; init; }

    private readonly ControllerManager _controllerManager;
    private readonly ViewportManager _viewportManager;
    private readonly UIOverlay _uiOverlay;

    public ITilePlacement TilePlacement;

    public MultiTool MultiTool { get; set; } = new MultiTool();

    public IAction PrimaryAction;
    public TelescopeResult TelescopePrimaryAction = TelescopeResult.None;
    public Entity HoveredEntity;

    private string _buildingKey;

    public string BuildingKey
    {
        get { return _buildingKey; }
        set
        {
            _buildingKey = value;
            if (_buildingKey == null)
            {
                TilePlacement = null;
            }
            else
            {
                var building = GlobalState.BuildingData.Buildings[_buildingKey];
                TilePlacement = new PointTilePlacement(building.Type, _buildingKey, building.Buildable);
            }
        }
    }

    public Player(
        Inventory inventory,
        ControllerManager controllerManager,
        ViewportManager viewportManager,
        UIOverlay uiOverlay)
    {
        Inventory = inventory;
        _controllerManager = controllerManager;
        _viewportManager = viewportManager;
        _uiOverlay = uiOverlay;
        // intentionally smaller than the player sprite so player can dodge more easily
        HitRadiusPow2 = 64;//8*8 (i.e. 8^2)
        HitboxYOffset = -50;
        // TODO: set up player correctly from metadat files
        EntitySpriteKey = "player";
        DefaultAnimationKey = "idle";
        InitDefaultAnimation();
    }

    public void PickUpItem(Item item)
    {
        Inventory.AddItem(item.InstanceInfo);
    }

    public void Update(GameTime gameTime)
    {
        if (_uiOverlay.State.IsMouseOverElement)
        {
            return;
        }
        UpdateMovement(gameTime);
        var mouseCoordinates = GetMouseHoveredCoordinates();
        UpdateFacingDirection(directionX: mouseCoordinates.WorldPosition.XInt - XInt, directionY: mouseCoordinates.WorldPosition.YInt - YInt);
        if (_buildingKey != null)
        {
            UpdateBuildingPlacement(mouseCoordinates.TilePosition);
        }
        else
        {
            UpdateAction(mouseCoordinates);
        }
    }

    private void UpdateMovement(GameTime gameTime)
    {
        var movementPerFrame = gameTime.ElapsedGameTime.TotalSeconds * MovementSpeed;
        if (_controllerManager.IsKeyDown(Keys.LeftShift))
        {
#if DEBUG
            movementPerFrame *= 5;
#else
            movementPerFrame *= 2;
            // TODO: consume hunger faster
#endif
        }
        var playerHasMoved = false;
        // normalise vector for diagnoal movement?
        if (_controllerManager.IsKeyDown(Keys.W))
        {
            Y -= movementPerFrame;
            playerHasMoved = true;
        }
        if (_controllerManager.IsKeyDown(Keys.S))
        {
            Y += movementPerFrame;
            playerHasMoved = true;
        }
        if (_controllerManager.IsKeyDown(Keys.A))
        {
            X -= movementPerFrame;
            playerHasMoved = true;
        }
        if (_controllerManager.IsKeyDown(Keys.D))
        {
            X += movementPerFrame;
            playerHasMoved = true;
        }
        playerHasMoved |= UpdateForces(gameTime);
        if (playerHasMoved)
        {
            XInt = (int)X;
            YInt = (int)Y;
            this.UpdateTileIndex();
        }
    }

    private void UpdateAction(((int XInt, int YInt) WorldPosition, (int TileX, int TileY) TilePosition) mouseCoordinates)
    {
        if (PrimaryAction != null)
        {
            var (xOffset, yOffset, shootingDirection) = GetActionOffsetsAndDirection();
            var mouseTile = GlobalState.TerrainManager.GetTile(tileX: mouseCoordinates.TilePosition.TileX, tileY: mouseCoordinates.TilePosition.TileY);
            TelescopePrimaryAction = PrimaryAction.Telescope(
                this,
                mouseTile,
                xOffset: xOffset,
                yOffset: yOffset,
                shootingDirection);
        }
        else
        {
            TelescopePrimaryAction = TelescopeResult.None;
        }
        UpdateHoveredMob(mouseCoordinates);
        if (_controllerManager.IsLeftMouseInitialPressed())
        {
            TelescopePrimaryAction.Invoke();
        }
        else if (_controllerManager.IsRightMouseInitialPressed())
        {
            InvokeSecondaryAction();
        }
    }

    private void UpdateHoveredMob(((int XInt, int YInt) WorldPosition, (int TileX, int TileY) TilePosition) mouseCoordinates)
    {
        var (mouseWorldX, mouseWorldY) = mouseCoordinates.WorldPosition;
        var (mouseTileX, mouseTileY) = mouseCoordinates.TilePosition;
        var (mob, _, _) = GlobalState.MobManager.GetEntitiesInRange(xTileStart: mouseTileX - 1, xTileEnd: mouseTileX + 1, yTileStart: mouseTileY - 1, yTileEnd: mouseTileY + 1)
            .Select(mob =>
            {
                var xDiffMouse = mouseWorldX - mob.XInt - mob.HitboxXOffset;
                var yDiffMouse = mouseWorldY - mob.YInt - mob.HitboxYOffset;
                var xDiffPlayer = XInt - mob.XInt;
                var yDiffPlayer = YInt - mob.YInt;
                return (
                    mob,
                    mouseDistancePow2: xDiffMouse * xDiffMouse + yDiffMouse * yDiffMouse,
                    playerDistancePow2: Renderer.RenderFogOfWar ? xDiffPlayer * xDiffPlayer + yDiffPlayer * yDiffPlayer : 0
                );
            })
            .Where(a => a.mouseDistancePow2 <= a.mob.HitRadiusPow2 && a.playerDistancePow2 <= SightRadiusWorldPow2)
            .OrderBy(a => a.mouseDistancePow2)
            .FirstOrDefault();
        HoveredEntity = mob;
    }

    private void InvokeSecondaryAction()
    {
        if (HoveredEntity != null)
        {

        }
    }

    private (int xOffset, int yOffset, Vector2 shootingDirection) GetActionOffsetsAndDirection()
    {
        var mouseScreenPosition = _controllerManager.CurrentMouseState.Position;
        var moouseWorldPosition = _viewportManager.ConvertScreenCoordinatesToWorldCoordinates(mouseScreenPosition.X, mouseScreenPosition.Y);
        var xOffset = PrimaryAction.CreatesProjectile
            ? FacingDirection == FacingDirection.Left ? -32
            : FacingDirection == FacingDirection.Right ? 32
            : 0
            : 0;
        var yOffset = PrimaryAction.CreatesProjectile || FacingDirection != FacingDirection.Down
            ? HitboxYOffset
            : HitboxYOffset / 2;
        var shootingDirection = new Vector2(x: moouseWorldPosition.X - XInt - xOffset, y: moouseWorldPosition.Y - YInt - yOffset);
        shootingDirection.Normalize();
        return (xOffset, yOffset, shootingDirection);
    }

#if DEBUG
    public ArcRange GetWeaponRange(out int xOffsetOut, out int yOffsetOut)
    {
        var (xOffset, yOffset, shootingDirection) = GetActionOffsetsAndDirection();
        xOffsetOut = xOffset;
        yOffsetOut = yOffset;
        return MultiTool.WeaponRange(this, xOffset: xOffset, yOffset: yOffset, shootingDirection);
    }
#endif

    private void UpdateBuildingPlacement((int TileX, int TileY) mouseTilePosition)
    {
        if (TilePlacement != null)
        {
            TilePlacement.Update(mouseTilePosition);

            if (TilePlacement.AllTilesBuildable && _controllerManager.IsLeftMouseInitialPressed())
            {
                // TODO: identify point placement vs range placement depending on what is being built
                // buildings are range, stations are single
                TilePlacement = new RangeTilePlacement(TilePlacement.BuildingType, _buildingKey, TilePlacement.Buildable, mouseTilePosition);
                TilePlacement.CommittedToBuild = true;
                TilePlacement.Update(mouseTilePosition);
            }
            else if (TilePlacement.CommittedToBuild && _controllerManager.IsLeftMouseUp())
            {
                if (TilePlacement.AllTilesBuildable)
                {
                    TilePlacement.PlaceBuildings();

                    // TODO: Notify renderer to drop chunk pre-renders
                }
                //else
                //{
                //    // cancel build
                //    // TODO: notify player of invalid building
                //}

                // TODO: work out if
                // * clear building command
                // * start next building with current selection (currently what is below)
                var tilePlacementBuildable = TilePlacement.Buildable;
                TilePlacement = new PointTilePlacement(TilePlacement.BuildingType, _buildingKey, tilePlacementBuildable);
                TilePlacement.Update(mouseTilePosition);
            }
        }
    }

    private ((int XInt, int YInt) WorldPosition, (int TileX, int TileY) TilePosition) GetMouseHoveredCoordinates()
    {
        var screenPosition = _controllerManager.CurrentMouseState.Position;
        var worldPosition = _viewportManager.ConvertScreenCoordinatesToWorldCoordinates(screenPosition.X, screenPosition.Y);
        var tilePosition = _viewportManager.ConvertScreenCoordinatesToTileCoordinates(screenPosition.X, screenPosition.Y);
        return (WorldPosition: worldPosition, TilePosition: tilePosition);
    }
}
