using FarmSim.Utils;
using System;
using System.Collections.Generic;

namespace FarmSim.Terrain;

class Chunk
{
    public int ChunkSize { get; }
    public List<List<Tile>> Tiles { get; }
    public int ChunkX { get; }
    public int ChunkY { get; }

    public Chunk(
        int chunkSize,
        List<List<Tile>> tiles,
        int chunkX,
        int chunkY)
    {
        ChunkSize = chunkSize;
        Tiles = tiles;
        ChunkX = chunkX;
        ChunkY = chunkY;
    }

    public Tile GetTile(int tileX, int tileY)
    {
        var (xi, yi) = GetIndices(tileX: tileX, tileY: tileY);
        return Tiles[yi][xi];
    }

    public (int xIndex, int yIndex) GetIndices(int tileX, int tileY)
    {
        return (tileX.Mod(ChunkSize), tileY.Mod(ChunkSize));
    }

    public override bool Equals(object obj)
    {
        return obj is Chunk chunk &&
               ChunkX == chunk.ChunkX &&
               ChunkY == chunk.ChunkY;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ChunkX, ChunkY);
    }
}
