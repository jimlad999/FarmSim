using FarmSim.Entities;
using FarmSim.Terrain;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FarmSim.Utils;

// currently only dealing with static positions. will need work to deal with things that move
class ChunkPartitionedCollection<T> : IEnumerable<T>
    where T : IPositionable
{
    public Dictionary<(int ChunkX, int ChunkY), List<T>> ChunkPartitionedLists = new();

    public IEnumerable<T> GetInRange(int xTileStart, int xTileEnd, int yTileStart, int yTileEnd)
    {
        var xChunkStart = GetChunk(xTileStart);
        var xChunkEnd = GetChunk(xTileEnd) + 1;
        var yChunkStart = GetChunk(yTileStart);
        var yChunkEnd = GetChunk(yTileEnd) + 1;
        for (int chunkY = yChunkStart; chunkY < yChunkEnd; ++chunkY)
        {
            for (int chunkX = xChunkStart; chunkX < xChunkEnd; ++chunkX)
            {
                foreach (var value in LazyGet(chunkX: chunkX, chunkY: chunkY))
                {
                    yield return value;
                }
            }
        }
    }

    public void Add(T value)
    {
        var chunkX = GetChunk(value.TileX);
        var chunkY = GetChunk(value.TileY);
        LazyGet(chunkX: chunkX, chunkY: chunkY).Add(value);
    }

    public Func<Predicate<T>, int> RemoveAllInRange(
        int xTileStart,
        int xTileEnd,
        int yTileStart,
        int yTileEnd)
    {
        return predicate =>
        {
            var xChunkStart = GetChunk(xTileStart);
            var xChunkEnd = GetChunk(xTileEnd) + 1;
            var yChunkStart = GetChunk(yTileStart);
            var yChunkEnd = GetChunk(yTileEnd) + 1;
            var removed = 0;
            for (int chunkY = yChunkStart; chunkY < yChunkEnd; ++chunkY)
            {
                for (int chunkX = xChunkStart; chunkX < xChunkEnd; ++chunkX)
                {
                    removed += LazyGet(chunkX: chunkX, chunkY: chunkY).RemoveAll(predicate);
                }
            }
            return removed;
        };
    }

    public void Clear()
    {
        ChunkPartitionedLists.Clear();
    }

    private List<T> LazyGet(int chunkX, int chunkY)
    {
        var key = (chunkX, chunkY);
        if (!ChunkPartitionedLists.TryGetValue(key, out var value))
        {
            ChunkPartitionedLists[key] = value = new();
        }
        return value;
    }

    private static int GetChunk(int tile)
    {
        var chunk = tile / TerrainManager.ChunkSize;
        if (tile < 0) --chunk;
        return chunk;
    }

    public IEnumerator<T> GetEnumerator()
    {
        return ChunkPartitionedLists.SelectMany(a => a.Value).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
