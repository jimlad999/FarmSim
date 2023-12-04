using System.Collections.Generic;

namespace FarmSim.Terrain;

class Tile
{
    public Chunk Chunk { get; init; }
    public int X { get; init; }
    public int Y { get; init; }
    public string Terrain { get; init; }
    public string Trees { get; init; }
    public string Ores { get; init; }
    public Buildings Buildings { get; init; }

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
        Trees = trees;
        Ores = ores;
        Buildings = buildings;
    }
}
