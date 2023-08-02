using System.Collections.Generic;

namespace FarmSim.Terrain;

class TerrainManager
{
    private readonly Dictionary<int, Dictionary<int, Chunk>> _chunks = new();
    private readonly TerrainGenerator _terrainGenerator;

    public int ChunkRadius { get; private set; } = 16;
    public int ChunkSize { get; private set; } = 64;

    public TerrainManager()
    {
        _chunks = new();
        _terrainGenerator = new TerrainGenerator(ChunkSize);
    }

    public Tile GetTile(int tileX, int tileY)
    {
        var chunk = GetChunk(tileX, tileY);
        return chunk.GetTile(tileX, tileY);
    }

    public Chunk GetChunk(int tileX, int tileY)
    {
        var chunkX = (tileX < 0 ? tileX + 1 : tileX) / ChunkSize;
        if (tileX < 0) --chunkX;
        var chunkY = (tileY < 0 ? tileY + 1 : tileY) / ChunkSize;
        if (tileY < 0) --chunkY;

        //locks require?

        if (!_chunks.TryGetValue(chunkY, out var chunkSlice))
        {
            _chunks[chunkY] = chunkSlice = new();
        }

        if (!chunkSlice.TryGetValue(chunkX, out var chunk))
        {
            chunkSlice[chunkX] = chunk = _terrainGenerator.GenerateChunk(chunkX, chunkY);
        }

        return chunk;
    }


}
