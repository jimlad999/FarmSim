using FarmSim.External;
using System.Collections.Generic;

namespace FarmSim.Terrain;

class TerrainGenerator
{
    private OpenSimplexNoise _noiseGen = new OpenSimplexNoise();
    private int _chunkSize;

    public TerrainGenerator(int chunkSize)
    {
        _chunkSize = chunkSize;
    }

    public void Reseed(long seed)
    {
        _noiseGen = new OpenSimplexNoise(seed);
    }

    public Chunk GenerateChunk(int chunkX, int chunkY)
    {
        var xstart = chunkX * _chunkSize;
        var xend = xstart + _chunkSize;
        var ystart = chunkY * _chunkSize;
        var yend = ystart + _chunkSize;
        var tiles = new List<List<Tile>>(_chunkSize);
        for (var yi = ystart; yi < yend; ++yi)
        {
            var tileSlice = new List<Tile>(_chunkSize);
            tiles.Add(tileSlice);
            for (var xi = xstart; xi < xend; ++xi)
            {
                var noiseVal = _noiseGen.Evaluate(xi / 64.0, yi / 64.0);
                var tileset = "grass";
                if (noiseVal < -0.2)
                {
                    tileset = "water";
                }
                else if (noiseVal > 0.5)
                {
                    tileset = "rock";
                }
                tileSlice.Add(new Tile(tileset));
            }
        }
        return new Chunk(_chunkSize, tiles);
    }
}
