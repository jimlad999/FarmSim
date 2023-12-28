using FarmSim.Entities;
using FarmSim.Projectiles;
using FarmSim.Rendering;
using FarmSim.Utils;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FarmSim.Mobs;

class MobManager : EntityManager<Mob>
{
    private static readonly Matrix Rotate120 = Matrix.CreateRotationZ(2.0944f);
    private static readonly Color MobLightBlue = Color.FromNonPremultiplied(0, 120, 210, 255);
    private const int MinWaitTimeMilliseconds = 10_000;
    private const int MaxWaitTimeMilliseconds = 60_000;
    private const int RandomWaitTimeMilliseconds = MaxWaitTimeMilliseconds - MinWaitTimeMilliseconds;
    // Public because despawn radius of projectiles same as mobs
    public const int SpawnRadius = Player.Player.SightRadius + 4;//tiles
    private const int SpawnRadius2Plus1 = SpawnRadius + SpawnRadius + 1;//tiles
    private const int SpawnRadiusPow2 = SpawnRadius * SpawnRadius;

    private readonly MobData[] _mobData;
    private readonly Dictionary<string, EntityData> _entityData;
    private readonly EntityFactory<Mob, MobData> _mobFactor;

    private double _waitTimeMilliseconds;

    public MobManager(
        MobData[] mobData,
        Dictionary<string, EntityData> entityData)
    {
        _mobData = mobData;
        _entityData = entityData;
        _mobFactor = new EntityFactory<Mob, MobData>(mobData);
#if !DEBUG
        ResetSpawnWaitTime();
#endif
    }

#if DEBUG
    public override void Clear()
    {
        base.Clear();
        _waitTimeMilliseconds = 0;
    }
#endif

    public bool DetectCollision(Projectile projectile)
    {
        foreach (var mob in Entities.Where(projectile.DetectCollision))
        {
            // TODO: damage
            Damage(mob, mob.HP);
            if (projectile.Effect != null)
            {
                projectile.Effect.Apply(mob, projectile);
                GlobalState.AnimationManager.Generate(entity: mob, animationKey: projectile.Effect.AnimationKey, direction: new Vector2(x: 0, y: 1));
            }
            return true;
        }
        return false;
    }

    public void Damage(List<Mob> mobs, int damage)
    {
        foreach (var mob in mobs)
        {
            Damage(mob, damage);
        }
    }

    private static void Damage(Mob mob, int damage)
    {
        var hitAnimation = GlobalState.AnimationManager.PlayOnce(mob, "hit");
        mob.Hit = true;
        mob.HP -= damage;
        hitAnimation.After(() =>
        {
            mob.Hit = false;
            if (mob.HP <= 0)
            {
                mob.FlagForDespawning = true;
                GlobalState.AnimationManager.Generate(x: mob.XInt, y: mob.YInt, animationKey: "generic-despawn", scale: mob.Scale);
                foreach (var drop in mob.GetDrops())
                {
                    GlobalState.ItemManager.CreateNewItem(
                        itemId: drop,
                        originX: mob.XInt,
                        originY: mob.YInt,
                        normalizedDirection: RandomUtil.RandomNormalizedDirection());
                }
            }
        });
    }

    public void Update(GameTime gameTime)
    {
        foreach (var mob in Entities)
        {
            mob.Update(gameTime);
            if (GlobalState.PlayerManager.OutsideDespawnRadius(mob))
            {
                mob.FlagForDespawning = true;
                // do we need to generate animations for mobs we shouldn't be able see?
                GlobalState.AnimationManager.Generate(x: mob.XInt, y: mob.YInt, animationKey: "generic-despawn", scale: mob.Scale);
            }
        }
        Entities.RemoveAll(mob => mob.FlagForDespawning);
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
        foreach (var player in GlobalState.PlayerManager.Entities)
        {
            for (int mobGroups = 0; mobGroups < 3; ++mobGroups, offset = Vector2.Transform(offset, Rotate120))
            {
                var spawnTileX = player.TileX + (int)offset.X;
                var spawnTileY = player.TileY + (int)offset.Y;
                var spawnX = spawnTileX * Renderer.TileSize + Renderer.TileSizeHalf;
                var spawnY = spawnTileY * Renderer.TileSize + Renderer.TileSizeHalf;
                var spawnTile = GlobalState.TerrainManager.GetTile(tileX: spawnTileX, tileY: spawnTileY);
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
                    newMob.Tags = mobToSpawn.Tags.PickTags();
                    newMob.Scale = newMob.Tags.Match(new()
                    {
                        { Tags.Gigantic, () => 2f },
                        { Tags.Large, () => 1.5f },
                        { Tags.Medium, () => 1f },
                        { Tags.Small, () => 0.9f },
                        { Tags.Tiny, () => 0.8f },
                    }, defaultValue: 1f);
                    newMob.Color = newMob.Tags.Match(new()
                    {
                        { Tags.White, () => Color.White },
                        { Tags.Black, () => Color.Black },
                        { Tags.Red, () => Color.Red },
                        { Tags.Green, () => Color.Green },
                        { Tags.Blue, () => newMob.Scale < 1f ? MobLightBlue : Color.Blue },
                        { Tags.Yellow, () => Color.Yellow },
                    }, defaultValue: Color.White);
                    // TODO: modify based on metadata (currently 400 = 20*20 (i.e. 20^2))
                    newMob.HitRadiusPow2 = (int)(400 * newMob.Scale);
                    newMob.EntitySpriteKey = mobToSpawn.EntitySpriteKey;
                    newMob.DefaultAnimationKey = _entityData[mobToSpawn.EntitySpriteKey].DefaultAnimationKey;
                    newMob.HP = mobToSpawn.hp;
                    newMob.TileX = spawnTileX;
                    newMob.X = spawnX;
                    newMob.XInt = spawnX;
                    newMob.TileY = spawnTileY;
                    newMob.Y = spawnY;
                    newMob.YInt = spawnY;
                    newMob.HitboxYOffset = -16;
                    newMob.InitBehaviours();
                    newMob.InitDefaultAnimation();
                    Entities.Add(newMob);
                }
            }
        }
    }
}
