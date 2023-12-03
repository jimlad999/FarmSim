using System.Collections.Generic;

namespace FarmSim.Terrain;

class TerrainManager
{
    internal readonly TerrainGenerator _terrainGenerator;

    private Dictionary<int, Dictionary<int, Chunk>> _chunks = new();

    public int ChunkSize { get; private set; } = 64;

    public TerrainManager(int seed)
    {
        _chunks = new();
        _terrainGenerator = new TerrainGenerator(chunkSize: ChunkSize, seed: seed);
    }

    public void Reseed(int seed, bool clearExisting = true)
    {
        _terrainGenerator.Reseed(seed);
        if (clearExisting)
        {
            _chunks = new();
        }
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

    public TileRange GetRange(
        int topLeftX,
        int topLeftY,
        int bottomRightX,
        int bottomRightY)
    {
        var tiles = new List<Tile>((bottomRightY - topLeftY + 1) * (bottomRightX - topLeftX + 1));
        for (int tileY = topLeftY; tileY <= bottomRightY; ++tileY)
        {
            for (int tileX = topLeftX; tileX <= bottomRightX; ++tileX)
            {
                tiles.Add(GetTile(tileX: tileX, tileY: tileY));
            }
        }
        return new TileRange(
            tiles,
            topLeftX: topLeftX,
            topLeftY: topLeftY,
            bottomRightX: bottomRightX,
            bottomRightY: bottomRightY);
    }

    public void PlaceBuilding(
        string building,
        int topLeftX,
        int topLeftY,
        int bottomRightX,
        int bottomRightY)
    {
        for (int tileY = topLeftY; tileY <= bottomRightY; ++tileY)
        {
            for (int tileX = topLeftX; tileX <= bottomRightX; ++tileX)
            {
                GetTile(tileX: tileX, tileY: tileY)
                    .Buildings
                    .Add(building);
            }
        }
    }
}
