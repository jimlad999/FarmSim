using FarmSim.Rendering;
using FarmSim.Terrain;
using FarmSim.Utils;
using Microsoft.Xna.Framework;

namespace FarmSim.Player;

class Player
{
    private readonly ControllerManager _controllerManager;
    private readonly ViewportManager _viewportManager;
    private readonly TerrainManager _terrainManager;
    private readonly Tileset _tileset;

    private string _buildingTileset = "wood-floor";

    public Player(
        ControllerManager controllerManager,
        ViewportManager viewportManager,
        TerrainManager terrainManager,
        Tileset tileset)
    {
        _controllerManager = controllerManager;
        _viewportManager = viewportManager;
        _terrainManager = terrainManager;
        _tileset = tileset;

        // DEBUG:
        var tilePlacementBuildable = _tileset[_buildingTileset].Buildable;
        TilePlacement = new PointTilePlacement(_buildingTileset, tilePlacementBuildable);
    }

    public ITilePlacement TilePlacement { get; private set; }

    public void Update(GameTime gameTime)
    {
        if (_buildingTileset != null
            && _controllerManager.IsLeftMouseInitialPressed())
        {
            var tilePosition = GetHoveredTileCoordinates();
            var tileTerrain = _terrainManager.GetTile(tilePosition.X, tilePosition.Y).Terrain;
            var terrain = _tileset[tileTerrain];
            var tilePlacementBuildable = _tileset[_buildingTileset].Buildable;
            if (terrain.IsBuildable(tilePlacementBuildable))
            {
                // TODO: identify point placement vs range placement
                TilePlacement = new RangeTilePlacement(_buildingTileset, tilePlacementBuildable, tilePosition);
                TilePlacement.CommittedToBuild = true;
            }
        }
        if (TilePlacement != null)
        {
            var tilePlacementPosition = GetHoveredTileCoordinates();
            TilePlacement.Update(
                tilePlacementPosition,
                _terrainManager,
                _tileset);

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
                var tilePlacementBuildable = _tileset[_buildingTileset].Buildable;
                TilePlacement = new PointTilePlacement(_buildingTileset, tilePlacementBuildable);
            }
        }
    }

    public (int X, int Y) GetHoveredTileCoordinates()
    {
        var screenPosition = _controllerManager.CurrentMouseState.Position;
        var tilePosition = _viewportManager.ConvertScrenCoordinatesToTileCoordinates(screenPosition.X, screenPosition.Y);
        return tilePosition;
    }
}
