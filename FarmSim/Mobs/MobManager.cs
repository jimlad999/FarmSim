using FarmSim.Entities;
using FarmSim.Rendering;
using FarmSim.Terrain;
using FarmSim.Utils;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FarmSim.Mobs;

class MobManager
{
    private static readonly Matrix Rotate120 = Matrix.CreateRotationZ(2.0944f);
    private static readonly Color MobLightBlue = Color.FromNonPremultiplied(0, 120, 210, 255);
    private const int MinWaitTimeMilliseconds = 10_000;
    private const int MaxWaitTimeMilliseconds = 60_000;
    private const int RandomWaitTimeMilliseconds = MaxWaitTimeMilliseconds - MinWaitTimeMilliseconds;
    private const int SpawnRadius = Player.Player.SightRadius + 4;//tiles
    private const int SpawnRadius2Plus1 = SpawnRadius + SpawnRadius + 1;//tiles
    private const int SpawnRadiusPow2 = SpawnRadius * SpawnRadius;
    private const int DespawnRadius = SpawnRadius + 10;//tiles

    private readonly MobData[] _mobData;
    private readonly Player.Player _player;
    private readonly TerrainManager _terrainManager;
    private readonly MobFactory _mobFactor;

    private List<Mob> _mobs = new();

    private double _waitTimeMilliseconds;

    public MobManager(
        MobData[] mobData,
        Player.Player player,
        TerrainManager terrainManager)
    {
        _mobData = mobData;
        _player = player;
        _terrainManager = terrainManager;
        _mobFactor = new MobFactory(mobData);
#if !DEBUG
        ResetSpawnWaitTime();
#endif
    }

    internal void Clear()
    {
        _mobs = new();
#if DEBUG
        _waitTimeMilliseconds = 0;
#endif
    }

    public IEnumerable<Entity> GetEntitiesInRange(int xTileStart, int xTileEnd, int yTileStart, int yTileEnd)
    {
        return _mobs.Where(mob =>
            xTileStart <= mob.TileX && mob.TileX <= xTileEnd
            && yTileStart <= mob.TileY && mob.TileY <= yTileEnd);
    }

    public void Update(GameTime gameTime)
    {
        foreach (var mob in _mobs)
        {
            mob.Update(gameTime);
            mob.FlagForDespawning = Math.Abs(mob.TileX - _player.TileX) > DespawnRadius
                || Math.Abs(mob.TileY - _player.TileY) > DespawnRadius;
        }
        _mobs.RemoveAll(mob => mob.FlagForDespawning);
        if (_waitTimeMilliseconds > 0)
        {
            _waitTimeMilliseconds -= gameTime.ElapsedGameTime.TotalMilliseconds;
        }
        else
        {
            SpawnMobs();
            ResetSpawnWaitTime();
        }
    }

    private void ResetSpawnWaitTime()
    {
        _waitTimeMilliseconds = MinWaitTimeMilliseconds + RandomUtil.Rand.Next(RandomWaitTimeMilliseconds);
    }

    private void SpawnMobs()
    {
        // range: -SpawnRadius to SpawnRadius
        var xOffset = RandomUtil.Rand.Next(SpawnRadius2Plus1) - SpawnRadius;
        var yOffset = (int)Math.Sqrt(SpawnRadiusPow2 - xOffset * xOffset);
        if (RandomUtil.Rand.Next(2) == 1)
        {
            yOffset = -yOffset;
        };
        var offset = new Vector2(x: xOffset, y: yOffset);
        for (int mobGroups = 0; mobGroups < 3; ++mobGroups, offset = Vector2.Transform(offset, Rotate120))
        {
            var spawnTileX = _player.TileX + (int)offset.X;
            var spawnTileY = _player.TileY + (int)offset.Y;
            var spawnX = spawnTileX * Renderer.TileSize + Renderer.TileSizeHalf;
            var spawnY = spawnTileY * Renderer.TileSize + Renderer.TileSizeHalf;
            var spawnTile = _terrainManager.GetTile(tileX: spawnTileX, tileY: spawnTileY);
            var spawnableMobs = _mobData.Where(m => m.Spawnable.Contains(spawnTile.Terrain)).ToList();
            if (spawnableMobs.Count == 0)
            {
                return;
            }
            var mobToSpawn = spawnableMobs[RandomUtil.Rand.Next(spawnableMobs.Count)];
            var numberToSpawn = RandomUtil.Rand.Next(minValue: mobToSpawn.MinSpawned, maxValue: mobToSpawn.MaxSpawned);
            for (int mob = 0; mob < numberToSpawn; ++mob)
            {
                var newMob = _mobFactor.Create(mobToSpawn.Class);
                newMob.Metadata = mobToSpawn;
                newMob.Tags = mobToSpawn.GetTags();
                newMob.Tags.Match(new()
                {
                    { Tags.Gigantic, () => newMob.Scale = 2f },
                    { Tags.Large, () => newMob.Scale = 1.5f },
                    { Tags.Medium, () => newMob.Scale = 1f },
                    { Tags.Small, () => newMob.Scale = 0.9f },
                    { Tags.Tiny, () => newMob.Scale = 0.8f },
                });
                newMob.Tags.Match(new()
                {
                    { Tags.White, () => newMob.Color = Color.White },
                    { Tags.Black, () => newMob.Color = Color.Black },
                    { Tags.Red, () => newMob.Color = Color.Red },
                    { Tags.Green, () => newMob.Color = Color.Green },
                    { Tags.Blue, () => newMob.Color = newMob.Scale < 1f ? MobLightBlue : Color.Blue },
                    { Tags.Yellow, () => newMob.Color = Color.Yellow },
                });
                newMob.EntitySpriteKey = mobToSpawn.EntitySpriteKey;
                newMob.HP = mobToSpawn.hp;
                newMob.TileX = spawnTileX;
                newMob.X = spawnX;
                newMob.XInt = spawnX;
                newMob.TileY = spawnTileY;
                newMob.Y = spawnY;
                newMob.YInt = spawnY;
                newMob.Init();
                _mobs.Add(newMob);
            }
        }
    }
}
