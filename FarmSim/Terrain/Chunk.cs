using FarmSim.Utils;
using System.Collections.Generic;

namespace FarmSim.Terrain;

class Chunk
{
    public int ChunkSize { get; init; }
    public List<List<Tile>> Tiles { get; init; }

    public Chunk(int chunkSize, List<List<Tile>> tiles)
    {
        ChunkSize = chunkSize;
        Tiles = tiles;
    }

    public Tile GetTile(int tileX, int tileY)
    {
        var xi = tileX.Mod(ChunkSize);
        var yi = tileY.Mod(ChunkSize);
        return Tiles[yi][xi];
    }
}
