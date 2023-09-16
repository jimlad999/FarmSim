namespace FarmSim.Terrain;

class Tile
{
    public string Tileset { get; init; }
    public string Trees { get; init; }
    public string Ores { get; init; }

    public Tile(
        string tileset,
        string trees,
        string ores)
    {
        Tileset = tileset;
        Trees = trees;
        Ores = ores;
    }
}
