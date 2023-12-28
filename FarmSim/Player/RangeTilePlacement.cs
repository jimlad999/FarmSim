using FarmSim.Terrain;
using FarmSim.Utils;
using System.Collections.Generic;
using System.Linq;

namespace FarmSim.Player;

interface ITilePlacement
{
    public BuildingType BuildingType { get; }
    string BuildingKey { get; }
    ICollection<Zoning> Buildable { get; }
    bool AllTilesBuildable { get; }
    bool CommittedToBuild { get; set; }

    bool TileInRange(int tileX, int tileY);
    bool TileTopOfRange(int tileX, int tileY);
    bool TileBottomOfRange(int tileX, int tileY);
    void Update((int X, int Y) tilePlacementPosition);
    void PlaceBuildings();
}

// can be used for placing individual tiles + preview of placements
class PointTilePlacement : ITilePlacement
{
    private (int X, int Y) _tilePlacementPosition;

    public BuildingType BuildingType { get; init; }
    public string BuildingKey { get; init; }
    public ICollection<Zoning> Buildable { get; init; }
    public bool AllTilesBuildable { get; private set; }
    public bool CommittedToBuild { get; set; }

    public PointTilePlacement(
        BuildingType buildingType,
        string buildingKey,
        ICollection<Zoning> buildable)
    {
        BuildingType = buildingType;
        BuildingKey = buildingKey;
        Buildable = buildable;
    }

    public bool TileInRange(int tileX, int tileY)
    {
        return _tilePlacementPosition.X == tileX
            && _tilePlacementPosition.Y == tileY;
    }

    public bool TileTopOfRange(int tileX, int tileY)
    {
        return _tilePlacementPosition.X == tileX
            && _tilePlacementPosition.Y == tileY;
    }

    public bool TileBottomOfRange(int tileX, int tileY)
    {
        return _tilePlacementPosition.X == tileX
            && _tilePlacementPosition.Y == tileY;
    }

    public void Update((int X, int Y) tilePlacementPosition)
    {
        _tilePlacementPosition = tilePlacementPosition;
        var tile = GlobalState.TerrainManager.GetTile(tileX: tilePlacementPosition.X, tileY: tilePlacementPosition.Y);
        AllTilesBuildable = BuildingExtensions.YieldTilesets(tile)
            .All(key => GlobalState.ConsolidatedZoningData[key].IsBuildable(Buildable));
    }

    public void PlaceBuildings()
    {
        GlobalState.TerrainManager.PlaceBuilding(
            BuildingType,
            BuildingKey,
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

    public BuildingType BuildingType { get; init; }
    public ICollection<Zoning> Buildable { get; init; }
    public string BuildingKey { get; init; }
    public bool AllTilesBuildable { get; private set; }
    public bool CommittedToBuild { get; set; }

    public RangeTilePlacement(
        BuildingType buildingType,
        string buildingKey,
        ICollection<Zoning> buildable,
        (int X, int Y) initialTilePlacementPosition)
    {
        BuildingType = buildingType;
        BuildingKey = buildingKey;
        Buildable = buildable;
        _initialTilePlacementPosition = initialTilePlacementPosition;
    }

    public bool TileInRange(int tileX, int tileY)
    {
        return _topLeftX <= tileX && tileX <= _bottomRightX
            && _topLeftY <= tileY && tileY <= _bottomRightY;
    }

    public bool TileTopOfRange(int tileX, int tileY)
    {
        return _topLeftX <= tileX && tileX <= _bottomRightX
            && tileY == _topLeftY;
    }

    public bool TileBottomOfRange(int tileX, int tileY)
    {
        return _topLeftX <= tileX && tileX <= _bottomRightX
            && tileY == _bottomRightY;
    }

    public void Update((int X, int Y) tilePlacementPosition)
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
        var tileRange = GlobalState.TerrainManager.GetRange(
            topLeftX: _topLeftX,
            topLeftY: _topLeftY,
            bottomRightX: _bottomRightX,
            bottomRightY: _bottomRightY);
        AllTilesBuildable = tileRange.AllTilesAreBuildable(Buildable);
    }

    public void PlaceBuildings()
    {
        GlobalState.TerrainManager.PlaceBuilding(
            BuildingType,
            BuildingKey,
            topLeftX: _topLeftX,
            topLeftY: _topLeftY,
            bottomRightX: _bottomRightX,
            bottomRightY: _bottomRightY);
    }
}
