using FarmSim.Entities;
using FarmSim.Rendering;
using FarmSim.Terrain;
using FarmSim.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
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

    public void Update(GameTime gameTime)
    {
        UpdateMovement(gameTime);
        if (BuildingKey != null
            && !_uiOverlay.State.IsMouseOverElement)
        {
            UpdateBuildingPlacement();
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
            YInt = (int)Y;
            FacingDirection = FacingDirection.Up;
            playerHasMoved = true;
        }
        if (_controllerManager.IsKeyDown(Keys.S))
        {
            Y += movementPerFrame;
            YInt = (int)Y;
            FacingDirection = FacingDirection.Down;
            playerHasMoved = true;
        }
        if (_controllerManager.IsKeyDown(Keys.A))
        {
            X -= movementPerFrame;
            XInt = (int)X;
            FacingDirection = FacingDirection.Left;
            playerHasMoved = true;
        }
        if (_controllerManager.IsKeyDown(Keys.D))
        {
            X += movementPerFrame;
            XInt = (int)X;
            FacingDirection = FacingDirection.Right;
            playerHasMoved = true;
        }
        if (playerHasMoved)
        {
            TileX = XInt / Renderer.TileSize;
            if (XInt < 0) --TileX;
            TileY = YInt / Renderer.TileSize;
            if (YInt < 0) --TileY;
            _terrainManager.UpdateSight(tileX: TileX, tileY: TileY, SightRadius);
        }
    }

    private void UpdateBuildingPlacement()
    {
        if (_controllerManager.IsLeftMouseInitialPressed())
        {
            var tilePosition = GetHoveredTileCoordinates();
            var tileTerrain = _terrainManager.GetTile(tilePosition.X, tilePosition.Y).Terrain;
            var terrain = _spriteSheet.Tileset[tileTerrain];
            if (terrain.IsBuildable(TilePlacement.Buildable))
            {
                // TODO: identify point placement vs range placement depending on what is being built
                // buildings are range, stations are single
                TilePlacement = new RangeTilePlacement(TilePlacement.BuildingType, BuildingKey, TilePlacement.Buildable, tilePosition);
                TilePlacement.CommittedToBuild = true;
            }
        }
        if (TilePlacement != null)
        {
            var tilePlacementPosition = GetHoveredTileCoordinates();
            TilePlacement.Update(
                tilePlacementPosition,
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
                    tilePlacementPosition,
                    _terrainManager,
                    _spriteSheet);
            }
        }
    }

    private (int X, int Y) GetHoveredTileCoordinates()
    {
        var screenPosition = _controllerManager.CurrentMouseState.Position;
        var tilePosition = _viewportManager.ConvertScrenCoordinatesToTileCoordinates(screenPosition.X, screenPosition.Y);
        return tilePosition;
    }
}
