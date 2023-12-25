using FarmSim.Entities;
using FarmSim.Rendering;
using FarmSim.Terrain;
using FarmSim.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using UI;
using Utils;

namespace FarmSim.Player;

class Player : Entity
{
    private const double MovementSpeed = 200;
    public const int SightRadius = 12;//tiles

    private readonly ControllerManager _controllerManager;
    private readonly ViewportManager _viewportManager;
    private readonly TerrainManager _terrainManager;
    private readonly SpriteSheet _spriteSheet;
    private readonly UIOverlay _uiOverlay;

    public Player(
        ControllerManager controllerManager,
        ViewportManager viewportManager,
        TerrainManager terrainManager,
        SpriteSheet spriteSheet,
        UIOverlay uiOverlay)
    {
        _controllerManager = controllerManager;
        _viewportManager = viewportManager;
        _terrainManager = terrainManager;
        _spriteSheet = spriteSheet;
        _uiOverlay = uiOverlay;
        EntitySpriteKey = "player";
        // intentionally smaller than the player sprite so player can dodge more easily
        HitRadiusPow2 = 64;//8*8 (i.e. 8^2)
        HitboxYOffset = -50;
    }

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
                var building = GlobalState.BuildingData.Buildings[BuildingKey];
                TilePlacement = new PointTilePlacement(building.Type, BuildingKey, building.Buildable);
            }
        }
    }
    public ITilePlacement TilePlacement;

    private IAction PrimaryAction = new FireProjectileActions();

    public void Update(GameTime gameTime)
    {
        if (_uiOverlay.State.IsMouseOverElement)
        {
            return;
        }
        UpdateMovement(gameTime);
        UpdateFacingDirectionToMouse();
        if (BuildingKey != null)
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
        if (playerHasMoved)
        {
            XInt = (int)X;
            YInt = (int)Y;
            UpdateTilePosition();
            _terrainManager.UpdateSight(tileX: TileX, tileY: TileY, SightRadius);
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
            var mouseScreenPosition = _controllerManager.CurrentMouseState.Position;
            var moouseWorldPosition = _viewportManager.ConvertScreenCoordinatesToWorldCoordinates(mouseScreenPosition.X, mouseScreenPosition.Y);
            var xOffset = FacingDirection == FacingDirection.Left ? -32
                : FacingDirection == FacingDirection.Right ? 32
                : 0;
            var yOffset = HitboxYOffset;
            var shootingDirection = new Vector2(x: moouseWorldPosition.X - XInt - xOffset, y: moouseWorldPosition.Y - YInt - yOffset);
            shootingDirection.Normalize();
            PrimaryAction.Invoke(
                this,
                xOffset: xOffset,
                yOffset: yOffset,
                shootingDirection);
        }
    }

    private void PerformPrimaryAction()
    {
        throw new NotImplementedException();
    }

    private void UpdateBuildingPlacement()
    {
        if (_controllerManager.IsLeftMouseInitialPressed())
        {
            var mouseTilePosition = GetHoveredTileCoordinates();
            var tileTerrain = _terrainManager.GetTile(mouseTilePosition.X, mouseTilePosition.Y).Terrain;
            var terrain = _spriteSheet.Tileset[tileTerrain];
            if (terrain.IsBuildable(TilePlacement.Buildable))
            {
                // TODO: identify point placement vs range placement depending on what is being built
                // buildings are range, stations are single
                TilePlacement = new RangeTilePlacement(TilePlacement.BuildingType, BuildingKey, TilePlacement.Buildable, mouseTilePosition);
                TilePlacement.CommittedToBuild = true;
            }
        }
        if (TilePlacement != null)
        {
            var mouseTilePosition = GetHoveredTileCoordinates();
            TilePlacement.Update(
                mouseTilePosition,
                _terrainManager,
                _spriteSheet);

            if (TilePlacement.CommittedToBuild
                && _controllerManager.IsLeftMouseUp())
            {
                if (TilePlacement.AllTilesBuildable)
                {
                    TilePlacement.PlaceBuildings(_terrainManager);

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
                TilePlacement = new PointTilePlacement(TilePlacement.BuildingType, BuildingKey, tilePlacementBuildable);
                TilePlacement.Update(
                    mouseTilePosition,
                    _terrainManager,
                    _spriteSheet);
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
