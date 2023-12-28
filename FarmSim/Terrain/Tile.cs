using FarmSim.Entities;
using System.Collections.Generic;

namespace FarmSim.Terrain;

class Tile
{
    public Chunk Chunk;
    public int X;
    public int Y;
    public string Terrain;
    public Resource Trees;
    public Resource Ores;
    public Buildings Buildings;
    public bool InSight;


    public Tile(
        Chunk chunk,
        int x,
        int y,
        string terrain,
        string trees,
        string ores,
        Buildings buildings)
    {
        Chunk = chunk;
        X = x;
        Y = y;
        Terrain = terrain;
        Trees = trees == null ? null : GlobalState.TerrainManager.CreateResource(trees, tileX: x, tileY: y);
        Ores = ores == null ? null : GlobalState.TerrainManager.CreateResource(ores, tileX: x, tileY: y);
        Buildings = buildings;
    }

    public void RemoveResource(Resource resource)
    {
        if (resource == Trees)
        {
            Trees = null;
        }
        else if (resource == Ores)
        {
            Ores = null;
        }
    }

    public IEnumerable<Resource> GetResources()
    {
        if (Trees != null) yield return Trees;
        if (Ores != null) yield return Ores;
    }

    public IEnumerable<Entity> GetEntities()
    {
        return GetResources();
    }
}
