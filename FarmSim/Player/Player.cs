﻿using FarmSim.Entities;
using FarmSim.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using UI;
using Utils;

namespace FarmSim.Player;

class Player : Entity, IHasMultiTool
{
    public const int SightRadius = 12;//tiles
    private const double MovementSpeed = 200;
    private const int PickUpDistancePow2 = Renderer.TileSize * Renderer.TileSize;//pick up within 1 tile

    public readonly Inventory Inventory;

    private readonly ControllerManager _controllerManager;
    private readonly ViewportManager _viewportManager;
    private readonly UIOverlay _uiOverlay;

    public ITilePlacement TilePlacement;

    public MultiTool MultiTool { get; set; } = new MultiTool();

    private IAction PrimaryAction = new MultiToolActions();//DEBUG
    //private IAction PrimaryAction = new FireProjectileActions();//DEBUG

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

    public bool TryPickUpItem(Item item)
    {
        var itemXDiff = XInt - item.XInt;
        var itemYDiff = YInt - item.YInt;
        var itemDistancePow2 = itemXDiff * itemXDiff + itemYDiff * itemYDiff;
        if (itemDistancePow2 <= PickUpDistancePow2)
        {
            Inventory.AddItem(item.InstanceInfo);
            return true;
        }
        return false;
    }

    public void Update(GameTime gameTime)
    {
        if (_uiOverlay.State.IsMouseOverElement)
        {
            return;
        }
        UpdateMovement(gameTime);
        UpdateFacingDirectionToMouse();
        if (_buildingKey != null)
        {
            UpdateBuildingPlacement();
        }
        else
        {
            UpdateAction();
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

    private void UpdateFacingDirectionToMouse()
    {
        var mouseScreenPosition = _controllerManager.CurrentMouseState.Position;
        var moouseWorldPosition = _viewportManager.ConvertScreenCoordinatesToWorldCoordinates(mouseScreenPosition.X, mouseScreenPosition.Y);
        UpdateFacingDirection(directionX: moouseWorldPosition.X - XInt, directionY: moouseWorldPosition.Y - YInt);
    }

    private void UpdateAction()
    {
        if (_controllerManager.IsLeftMouseInitialPressed())
        {
            var (xOffset, yOffset, shootingDirection) = GetActionOffsetsAndDirection();
            PrimaryAction.Invoke(
                this,
                xOffset: xOffset,
                yOffset: yOffset,
                shootingDirection);
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

    private void UpdateBuildingPlacement()
    {
        if (TilePlacement != null)
        {
            var mouseTilePosition = GetHoveredTileCoordinates();
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

    private (int X, int Y) GetHoveredTileCoordinates()
    {
        var screenPosition = _controllerManager.CurrentMouseState.Position;
        var tilePosition = _viewportManager.ConvertScreenCoordinatesToTileCoordinates(screenPosition.X, screenPosition.Y);
        return tilePosition;
    }
}
