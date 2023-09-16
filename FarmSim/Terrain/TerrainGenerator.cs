using FarmSim.External;
using System;
using System.Collections.Generic;

namespace FarmSim.Terrain;

class TerrainGenerator
{
    private Random _rand;
    private OpenSimplexNoise _regionGenerator;
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
        _regionGenerator = new OpenSimplexNoise(_rand.NextInt64());
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
                var tileNoiseVal = _tileGenerator.Evaluate(xTile / 64.0, yTile / 64.0);
                var regionNoiseVal = _regionGenerator.Evaluate(xTile / 128.0, yTile / 128.0);
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

    private static GeneratorFuncs GetGeneratorFuncs(double noiseVal, int xTile, int yTile)
    {
        // ensure center area is plains
        var d = xTile * xTile + yTile * yTile;
        const double dd = 128 * 128;
        var boundMod = 1 - d / dd;
        if (Math.Abs(noiseVal) < boundMod)
        {
            return new GeneratorFuncs(
                terrainFunc: GetPlainsTerrain,
                treeFunc: GetTreePlains,
                oreFunc: GetOrePlains,
                animalFunc: GetNothing);
        }
        return noiseVal switch
        {
            > 0.7 => new GeneratorFuncs(
                terrainFunc: GetRockyTerrain,
                treeFunc: GetTreeRocky,
                oreFunc: GetOreRocky,
                animalFunc: GetNothing),
            > 0.5 => new GeneratorFuncs(
                terrainFunc: GetPlainsTerrain,
                treeFunc: GetTreeForrest,
                oreFunc: GetNothing,
                animalFunc: GetNothing),
            < 0.2 => new GeneratorFuncs(
                terrainFunc: GetSeaTerrain,
                treeFunc: GetNothing,
                oreFunc: GetOreSea,
                animalFunc: GetNothing),
            _ => new GeneratorFuncs(
                terrainFunc: GetPlainsTerrain,
                treeFunc: GetTreePlains,
                oreFunc: GetOrePlains,
                animalFunc: GetNothing),
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
            > 0.1 => "tree-pine",
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
            > 0.95 => "tree-pine",
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
