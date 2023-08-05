using FarmSim.External;
using System;
using System.Collections.Generic;

namespace FarmSim.Terrain;

class TerrainGenerator
{
    private Random _rand;
    private OpenSimplexNoise _tileGenerator;
    private OpenSimplexNoise _regionGenerator;
    private int _chunkSize;

    public TerrainGenerator(int chunkSize, int seed)
    {
        _chunkSize = chunkSize;
        Reseed(seed);
    }

    public void Reseed(int seed)
    {
        _rand = new Random(seed);
        _tileGenerator = new OpenSimplexNoise(_rand.NextInt64());
        _regionGenerator = new OpenSimplexNoise(_rand.NextInt64());
    }

    public Chunk GenerateChunk(int chunkX, int chunkY)
    {
        var xstart = chunkX * _chunkSize;
        var xend = xstart + _chunkSize;
        var ystart = chunkY * _chunkSize;
        var yend = ystart + _chunkSize;
        var tiles = new List<List<Tile>>(_chunkSize);
        for (var yTile = ystart; yTile < yend; ++yTile)
        {
            var tileSlice = new List<Tile>(_chunkSize);
            tiles.Add(tileSlice);
            for (var xTile = xstart; xTile < xend; ++xTile)
            {
                var tileNoiseVal = _tileGenerator.Evaluate(xTile / 64.0, yTile / 64.0);
                var regionNoiseVal = _regionGenerator.Evaluate(xTile / 128.0, yTile / 128.0);
                var getTile = GetTilesetFunc(noiseVal: regionNoiseVal, xTile: xTile, yTile: yTile);
                var tileset = getTile(tileNoiseVal);
                tileSlice.Add(new Tile(tileset));
            }
        }
        return new Chunk(_chunkSize, tiles);
    }

    private static Func<double, string> GetTilesetFunc(double noiseVal, int xTile, int yTile)
    {
        var d = xTile * xTile + yTile * yTile;
        const double dd = 128 * 128;
        var boundMod = 1 - d / dd;
        if (Math.Abs(noiseVal) < boundMod) return GetPlainsTileset;
        return noiseVal switch
        {
            > 0.7 => GetRockyTileset,
            < 0.2 => GetSeaTileset,
            _ => GetPlainsTileset,
        };
    }

    private static string GetRockyTileset(double noiseVal)
    {
        return noiseVal switch
        {
            < -0.9 => "grass",
            < -0.5 => "sand",
            _ => "rock",
        };
    }

    private static string GetSeaTileset(double noiseVal)
    {
        return noiseVal switch
        {
            > 0.95 => "rock",
            > 0.85 => "sand",
            _ => "water",
        };
    }

    private static string GetPlainsTileset(double noiseVal)
    {
        return noiseVal switch
        {
            > 0.8 => "rock",
            < -0.8 => "water",
            _ => "grass",
        };
    }
}
