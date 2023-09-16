namespace FarmSim.Terrain;

class Tile
{
    public string Terrain { get; init; }
    public string Trees { get; init; }
    public string Ores { get; init; }

    public Tile(
        string terrain,
        string trees,
        string ores)
    {
        Terrain = terrain;
        Trees = trees;
        Ores = ores;
    }
}
