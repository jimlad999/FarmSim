using FarmSim.External;
using FarmSim.Utils;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FarmSim.Terrain;

class TerrainGenerator
{
    private const int ContinentRadius = 256;
    private const int GapBetweenContinents = 128;
    private const int ContinentRegion = ContinentRadius + GapBetweenContinents;
    private Random _rand;
    private OpenSimplexNoise _regionGenerator1;
    private OpenSimplexNoise _regionGenerator2;
    private OpenSimplexNoise _regionGenerator3;
    private OpenSimplexNoise _tileGenerator;
    private OpenSimplexNoise _treeGenerator;
    private OpenSimplexNoise _oreGenerator;
    private int _chunkSize;

    public TerrainGenerator(int chunkSize, int seed)
    {
        _chunkSize = chunkSize;
        Reseed(seed);
    }

    public void Reseed(int seed)
    {
        _rand = new Random(seed);
        _regionGenerator1 = new OpenSimplexNoise(_rand.NextInt64());
        _regionGenerator2 = new OpenSimplexNoise(_rand.NextInt64());
        _regionGenerator3 = new OpenSimplexNoise(_rand.NextInt64());
        _tileGenerator = new OpenSimplexNoise(_rand.NextInt64());
        _treeGenerator = new OpenSimplexNoise(_rand.NextInt64());
        _oreGenerator = new OpenSimplexNoise(_rand.NextInt64());
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
                var tileNoiseVal = GetTileNoiseVal(yTile, xTile);
                var regionNoiseVal = GetRegionNoiseVal(yTile, xTile);
                var treeNoiseVal = _treeGenerator.Evaluate(xTile, yTile);
                var oreNoiseVal = _oreGenerator.Evaluate(xTile, yTile);
                var generatorFuncs = GetGeneratorFuncs(noiseVal: regionNoiseVal, xTile: xTile, yTile: yTile);
                var terrain = generatorFuncs.TerrainFunc(tileNoiseVal);
                var trees = generatorFuncs.TreeFunc(treeNoiseVal, terrain);
                var ores = generatorFuncs.OreFunc(oreNoiseVal, terrain);
                tileSlice.Add(new Tile(terrain, trees, ores));
            }
        }
        return new Chunk(_chunkSize, tiles);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal double GetTileNoiseVal(int yTile, int xTile)
    {
        return _tileGenerator.Evaluate(xTile / 7.0, yTile / 7.0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal double GetRegionNoiseVal(int yTile, int xTile)
    {
        return _regionGenerator1.Evaluate(xTile / 9.0, yTile / 9.0)
            + _regionGenerator2.Evaluate(xTile / 67.0, yTile / 78.0)
            - _regionGenerator3.Evaluate(xTile / 94.0, yTile / 56.0);
    }

    private static GeneratorFuncs GetGeneratorFuncs(double noiseVal, int xTile, int yTile)
    {
        // ensure center area is plains
        var d = DistanceSquaredFromCenterOfContinent(xTile, yTile);
        const double dd = ContinentRadius * ContinentRadius;
        //if (aaa > 0) d = d.Mod((int)dd);
        var dRatio = d / dd;
        var boundMod = 1 - dRatio;
        if (Math.Abs(noiseVal) < boundMod)
        {
            return new GeneratorFuncs(
                terrainFunc: GetPlainsTerrain,
                treeFunc: GetTreePlains,
                oreFunc: GetOrePlains,
                animalFunc: GetNothing);
        }
        return GetGeneratorFuncs(GetRegionType(noiseVal));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int DistanceSquaredFromCenterOfContinent(int xTile, int yTile)
    {
        const int regionSizeX2 = ContinentRegion * 2 - 1;
        xTile = Math.Abs(xTile);
        yTile = Math.Abs(yTile);
        while (xTile >= ContinentRegion) xTile -= regionSizeX2;
        while (yTile >= ContinentRegion) yTile -= regionSizeX2;
        xTile = xTile % ContinentRegion;
        yTile = yTile % ContinentRegion;
        var d = xTile * xTile + yTile * yTile;
        return d;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string GetRegionType(double noiseVal)
    {
        return noiseVal switch
        {
            > 0.7 => "rocky",
            < 0.2 => "sea",
            < 0.4 => "beach",
            _ => "plains",
        };
    }

    private static GeneratorFuncs GetGeneratorFuncs(string regionType)
    {
        return regionType switch
        {
            "rocky" => new GeneratorFuncs(
                terrainFunc: GetRockyTerrain,
                treeFunc: GetTreeRocky,
                oreFunc: GetOreRocky,
                animalFunc: GetNothing),
            "plains" => new GeneratorFuncs(
                terrainFunc: GetPlainsTerrain,
                treeFunc: GetTreeForrest,
                oreFunc: GetOrePlains,
                animalFunc: GetNothing),
            "sea" => new GeneratorFuncs(
                terrainFunc: GetSeaTerrain,
                treeFunc: GetNothing,
                oreFunc: GetOreSea,
                animalFunc: GetNothing),
            "beach" => new GeneratorFuncs(
                terrainFunc: GetBeachTerrain,
                treeFunc: GetNothing,
                oreFunc: GetNothing,
                animalFunc: GetNothing),
            _ => throw new NotImplementedException(),
        };
    }

    private static string GetRockyTerrain(double noiseVal)
    {
        return noiseVal switch
        {
            < -0.9 => "grass",
            < -0.5 => "sand",
            _ => "rock",
        };
    }

    private static string GetBeachTerrain(double noiseVal)
    {
        return noiseVal switch
        {
            > 0.95 => "rock",
            > 0.0 => "sand",
            _ => "water",
        };
    }

    private static string GetSeaTerrain(double noiseVal)
    {
        return noiseVal switch
        {
            > 0.95 => "rock",
            > 0.85 => "sand",
            _ => "water",
        };
    }

    private static string GetPlainsTerrain(double noiseVal)
    {
        return noiseVal switch
        {
            > 0.8 => "rock",
            < -0.8 => "water",
            _ => "grass",
        };
    }

    private static string GetTreePlains(double noiseVal, string terrain)
    {
        if (terrain == "water")
        {
            return null;
        }
        return noiseVal switch
        {
            > 0.7 => "tree-pine",
            _ => null,
        };
    }

    private static string GetTreeForrest(double noiseVal, string terrain)
    {
        if (terrain == "water")
        {
            return null;
        }
        return noiseVal switch
        {
            > 0.4 => "tree-pine",
            _ => null,
        };
    }

    private static string GetTreeRocky(double noiseVal, string terrain)
    {
        if (terrain == "water")
        {
            return null;
        }
        return noiseVal switch
        {
            > 0.8 => "tree-pine",
            _ => null,
        };
    }

    private static string GetOrePlains(double noiseVal, string terrain)
    {
        return noiseVal switch
        {
            > 0.9 => "ore-coal",
            _ => null,
        };
    }

    private static string GetOreRocky(double noiseVal, string terrain)
    {
        return noiseVal switch
        {
            > 0.4 => "ore-coal",
            _ => null,
        };
    }

    private static string GetOreSea(double noiseVal, string terrain)
    {
        return noiseVal switch
        {
            > 0.7 => "ore-coal",
            _ => null,
        };
    }

    private static string GetNothing(double noiseVal, string terrain)
    {
        return null;
    }

    private struct GeneratorFuncs
    {
        public Func<double, string> TerrainFunc;
        public Func<double, string, string> TreeFunc;
        public Func<double, string, string> OreFunc;
        public Func<double, string, string> AnimalFunc;

        public GeneratorFuncs(
            Func<double, string> terrainFunc,
            Func<double, string, string> treeFunc,
            Func<double, string, string> oreFunc,
            Func<double, string, string> animalFunc)
        {
            TerrainFunc = terrainFunc;
            TreeFunc = treeFunc;
            OreFunc = oreFunc;
            AnimalFunc = animalFunc;
        }
    }
}
