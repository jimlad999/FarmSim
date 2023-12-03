using FarmSim.Terrain;
using FarmSim.Utils;
using System.Collections.Generic;
using System.Linq;

namespace FarmSim.Player;

interface ITilePlacement
{
    string BuildingTileset { get; }
    ICollection<BuildingType> Buildable { get; }
    bool AllTilesBuildable { get; }
    bool CommittedToBuild { get; set; }

    bool TileInRange(int tileX, int tileY);
    void Update(
        (int X, int Y) tilePlacementPosition,
        TerrainManager terrainManager,
        Tileset tileset);
    void PlaceBuildings(TerrainManager terrainManager);
}

// can be used for placing individual tiles + preview of placements
class PointTilePlacement : ITilePlacement
{
    private (int X, int Y) _tilePlacementPosition;

    public string BuildingTileset { get; init; }
    public ICollection<BuildingType> Buildable { get; init; }
    public bool AllTilesBuildable { get; private set; }
    public bool CommittedToBuild { get; set; }

    public PointTilePlacement(
        string buildingTileset,
        ICollection<BuildingType> buildable)
    {
        BuildingTileset = buildingTileset;
        Buildable = buildable;
    }

    public bool TileInRange(int tileX, int tileY)
    {
        return _tilePlacementPosition.X == tileX
            && _tilePlacementPosition.Y == tileY;
    }

    public void Update(
        (int X, int Y) tilePlacementPosition,
        TerrainManager terrainManager,
        Tileset tileset)
    {
        _tilePlacementPosition = tilePlacementPosition;
        var tile = terrainManager.GetTile(tileX: tilePlacementPosition.X, tileY: tilePlacementPosition.Y);
        AllTilesBuildable = BuildingTypeExtensions.YieldTilesets(tile)
            .All(key => tileset[key].IsBuildable(Buildable));
    }

    public void PlaceBuildings(TerrainManager terrainManager)
    {
        terrainManager.PlaceBuilding(
            BuildingTileset,
            topLeftX: _tilePlacementPosition.X,
            topLeftY: _tilePlacementPosition.Y,
            bottomRightX: _tilePlacementPosition.X,
            bottomRightY: _tilePlacementPosition.Y);
    }
}

class RangeTilePlacement : ITilePlacement
{
    private readonly (int X, int Y) _initialTilePlacementPosition;
    private int _topLeftX;
    private int _topLeftY;
    private int _bottomRightX;
    private int _bottomRightY;

    public ICollection<BuildingType> Buildable { get; init; }
    public string BuildingTileset { get; init; }
    public bool AllTilesBuildable { get; private set; }
    public bool CommittedToBuild { get; set; }

    public RangeTilePlacement(
        string buildingTileset,
        ICollection<BuildingType> buildable,
        (int X, int Y) initialTilePlacementPosition)
    {
        BuildingTileset = buildingTileset;
        Buildable = buildable;
        _initialTilePlacementPosition = initialTilePlacementPosition;
    }

    public bool TileInRange(int tileX, int tileY)
    {
        return _topLeftX <= tileX && tileX <= _bottomRightX
            && _topLeftY <= tileY && tileY <= _bottomRightY;
    }

    public void Update(
        (int X, int Y) tilePlacementPosition,
        TerrainManager terrainManager,
        Tileset tileset)
    {
        if (tilePlacementPosition.X < _initialTilePlacementPosition.X)
        {
            _topLeftX = tilePlacementPosition.X;
            _bottomRightX = _initialTilePlacementPosition.X;
        }
        else
        {
            _topLeftX = _initialTilePlacementPosition.X;
            _bottomRightX = tilePlacementPosition.X;
        }
        if (tilePlacementPosition.Y < _initialTilePlacementPosition.Y)
        {
            _topLeftY = tilePlacementPosition.Y;
            _bottomRightY = _initialTilePlacementPosition.Y;
        }
        else
        {
            _topLeftY = _initialTilePlacementPosition.Y;
            _bottomRightY = tilePlacementPosition.Y;
        }
        var tileRange = terrainManager.GetRange(
            topLeftX: _topLeftX,
            topLeftY: _topLeftY,
            bottomRightX: _bottomRightX,
            bottomRightY: _bottomRightY);
        AllTilesBuildable = tileRange.AllTilesAreBuildable(Buildable, tileset);
    }

    public void PlaceBuildings(TerrainManager terrainManager)
    {
        terrainManager.PlaceBuilding(
            BuildingTileset,
            topLeftX: _topLeftX,
            topLeftY: _topLeftY,
            bottomRightX: _bottomRightX,
            bottomRightY: _bottomRightY);
    }
}
