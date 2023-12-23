using FarmSim.Entities;
using System.Collections.Generic;

namespace FarmSim.Terrain;

class Tile
{
    public Chunk Chunk { get; init; }
    public int X { get; init; }
    public int Y { get; init; }
    public string Terrain { get; init; }
    public Resource Trees { get; init; }
    public Resource Ores { get; init; }
    public Buildings Buildings { get; init; }
    public bool InSight { get; set; }


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
        Trees = trees == null ? null : new Resource(trees, tileX: x, tileY: y);
        Ores = ores == null ? null : new Resource(ores, tileX: x, tileY: y);
        Buildings = buildings;
    }

    public IEnumerable<Entity> GetEntities()
    {
        if (Trees != null) yield return Trees;
        if (Ores != null) yield return Ores;
    }
}
