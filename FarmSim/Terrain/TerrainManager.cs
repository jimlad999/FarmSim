﻿using FarmSim.Entities;
using FarmSim.UI;
using FarmSim.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FarmSim.Terrain;

class TerrainManager
{
    public const int ChunkSize = 64;

    private readonly TerrainGenerator _terrainGenerator;
    private readonly Dictionary<string, ResourceData> _resourceData;

    private Dictionary<int, Dictionary<int, Chunk>> _chunks = new();

    public TerrainManager(
        int seed,
        Dictionary<string, ResourceData> resourceData)
    {
        _chunks = new();
        _resourceData = resourceData;
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
        BuildingType buildingType,
        string buildingKey,
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
                    .Add(buildingType, buildingKey);
            }
        }
    }

    public Resource CreateResource(string tilesetKey, int tileX, int tileY)
    {
        var resourceData = _resourceData[tilesetKey];
        return new Resource(itemId: resourceData.ItemId, primaryTag: resourceData.PrimaryTag, tilesetKey: tilesetKey, tileX: tileX, tileY: tileY);
    }

    public void HavestResource(Resource resource, int harvestMultipler)
    {
        var tile = GetTile(tileX: resource.TileX, tileY: resource.TileY);
        resource.FlagForDespawning = true;
        tile.RemoveResource(resource);
        GlobalState.AnimationManager.Generate(x: resource.XInt, y: resource.YInt, animationKey: "generic-despawn", scale: 1f);
        // TODO: Added variance (i.e. generate arbitrary number of items based on config)
        var numberToGenerate = harvestMultipler;
        for (int i = 0; i < numberToGenerate; ++i)
        {
            GlobalState.ItemManager.CreateNewItem(resource.ItemId, originX: resource.XInt, originY: resource.YInt, normalizedDirection: RandomUtil.RandomNormalizedDirection());
        }
    }

    public void ChangeTile(Tile tile, string newTerrain)
    {
        tile.Terrain = newTerrain;
    }

    public bool ValidateMovement(int oldX, int oldY, int newX, int newY)
    {
        var oldTileX = oldX.ToTileIndex();
        var oldTileY = oldY.ToTileIndex();
        var newTileX = newX.ToTileIndex();
        var newTileY = newY.ToTileIndex();
        var oldTile = GetTile(tileX: oldTileX, tileY: oldTileY);
        var newTile = GetTile(tileX: newTileX, tileY: newTileY);
        var oldBuilding = oldTile.Buildings.FirstOrDefault(BuildingData.BuildingHasFloor);
        var newBuilding = newTile.Buildings.FirstOrDefault(BuildingData.BuildingHasFloor);
        var oldHasBuilding = oldBuilding != null;
        var newHasBuilding = newBuilding != null;
        if (oldHasBuilding == newHasBuilding)
        {
            return true;
        }
        else if (oldHasBuilding)
        {
            return GlobalState.BuildingData.Buildings[oldBuilding].CanWalkThrough;
        }
        else if (newHasBuilding)
        {
            return GlobalState.BuildingData.Buildings[newBuilding].CanWalkThrough;
        }
        else
        {
            return false;
        }
    }
}
