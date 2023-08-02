namespace FarmSim.Terrain;

class Tile
{
    public static Tile Null = new Tile("void");

    public string Tileset { get; init; }

    public Tile(string tileset)
    {
        Tileset = tileset;
    }
}
