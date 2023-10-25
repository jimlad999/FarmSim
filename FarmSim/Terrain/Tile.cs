namespace FarmSim.Terrain;

class Tile
{
    public Chunk Chunk { get; init; }
    public string Terrain { get; init; }
    public string Trees { get; init; }
    public string Ores { get; init; }

    public Tile(
        Chunk chunk,
        string terrain,
        string trees,
        string ores)
    {
        Chunk = chunk;
        Terrain = terrain;
        Trees = trees;
        Ores = ores;
    }
}
