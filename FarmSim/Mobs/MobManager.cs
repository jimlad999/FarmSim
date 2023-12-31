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
    private const int UnconsciousTimeMilliseconds = 15_000;
    private const int HappyTimeMilliseconds = 1_000;
    private const int MinWaitTimeMilliseconds = 10_000;
    private const int MaxWaitTimeMilliseconds = 60_000;
    private const int RandomWaitTimeMilliseconds = MaxWaitTimeMilliseconds - MinWaitTimeMilliseconds;
    // Public because despawn radius of projectiles same as mobs
    public const int SpawnRadius = Player.Player.SightRadiusTiles + 4;//tiles
    private const int SpawnRadius2Plus1 = SpawnRadius + SpawnRadius + 1;//tiles
    private const int SpawnRadiusPow2 = SpawnRadius * SpawnRadius;

    private readonly MobData[] _mobData;
    private readonly Dictionary<string, EntityData> _entityData;
    private readonly EntityFactory<Mob, MobData> _mobFactory;

    private double _waitTimeMilliseconds;

    public MobManager(
        MobData[] mobData,
        Dictionary<string, EntityData> entityData)
    {
        _mobData = mobData;
        _entityData = entityData;
        _mobFactory = new EntityFactory<Mob, MobData>(mobData);
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

    public IEnumerable<Mob> GetEntitiesInRange(int xTileStart, int xTileEnd, int yTileStart, int yTileEnd)
    {
        return Entities.Where(mob =>
            xTileStart <= mob.TileX && mob.TileX <= xTileEnd
            && yTileStart <= mob.TileY && mob.TileY <= yTileEnd);
    }

    public bool TryPickUpItem(Item item)
    {
        // Should unconscious mobs be able to pick up items?
        foreach (var mob in Entities)
        {
            var itemXDiff = mob.XInt - item.XInt;
            var itemYDiff = mob.YInt - item.YInt;
            var itemDistancePow2 = itemXDiff * itemXDiff + itemYDiff * itemYDiff;
            if (itemDistancePow2 <= mob.PickUpDistancePow2)
            {
                mob.PickUpItem(item);
                return true;
            }
        }
        return false;
    }

    public bool DetectCollision(Projectile projectile)
    {
        foreach (var mob in Entities.Where(projectile.DetectCollision))
        {
            // TODO: damage
            Damage(mob, mob.HP);
            if (projectile.Effect != null)
            {
                projectile.Effect.Apply(mob, projectile);
                if (projectile.Effect.AnimationKey != null)
                {
                    GlobalState.AnimationManager.Generate(entity: mob, animationKey: projectile.Effect.AnimationKey, durationMilliseconds: projectile.Effect.DurationMilliseconds, direction: Vector2.UnitY);
                }
            }
            return true;
        }
        return false;
    }

    public void Damage(IEnumerable<Mob> mobs, int damage)
    {
        foreach (var mob in mobs)
        {
            Damage(mob, damage);
        }
    }

    private static void Damage(Mob mob, int damage)
    {
        // Mob will fall unconscious when hp drops to/below 0. Player can then kill the mob for drops or tame it with food.
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
            // Mobs will drop anything they haven't already eaten
            foreach (var instanceInfo in mob.Inventory)
            {
                GlobalState.ItemManager.CreateItemFromExisting(
                    instanceInfo: instanceInfo,
                    originX: mob.XInt,
                    originY: mob.YInt,
                    normalizedDirection: RandomUtil.RandomNormalizedDirection());
            }
        }
        else
        {
            var hitAnimation = GlobalState.AnimationManager.PlayOnce(mob, "hit");
            mob.Hit = true;
            mob.HP -= damage;
            hitAnimation.After(() =>
            {
                mob.Hit = false;
                if (mob.HP <= 0)
                {
                    var hitAnimation = GlobalState.AnimationManager.PlayForDuration(mob, "unconscious", durationMilliseconds: UnconsciousTimeMilliseconds);
                    var unconsciousAnimation = GlobalState.AnimationManager.Generate(entity: mob, animationKey: "unconscious", durationMilliseconds: UnconsciousTimeMilliseconds, direction: Vector2.UnitY);
                    unconsciousAnimation.After(() =>
                    {
                        if (!mob.FlagForDespawning)
                        {
                            mob.HP = 1;
                        }
                    });
                }
            });
        }
    }

    public void Tame(Mob mob, Player.Player player)
    {
        if (mob.Owner != null)
        {
            return;
        }
        mob.HP = mob.Metadata.hp;
        mob.Owner = player;
        GlobalState.AnimationManager.PlayForDuration(mob, "happy", durationMilliseconds: HappyTimeMilliseconds);
        // TODO: limited time behaviour to make mob look at player
        // TODO: play special effects;
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

    public void ResetSpawnWaitTime()
    {
        _waitTimeMilliseconds = MinWaitTimeMilliseconds + RandomUtil.Rand.Next(RandomWaitTimeMilliseconds);
    }

    public void SpawnMobs()
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
                if (spawnTile.Buildings.Any())
                {
                    continue;
                }
                var spawnableMobs = _mobData.Where(m => m.Spawnable.Contains(spawnTile.Terrain)).ToList();
                if (spawnableMobs.Count == 0)
                {
                    continue;
                }
                var mobToSpawn = spawnableMobs[RandomUtil.Rand.Next(spawnableMobs.Count)];
                var numberToSpawn = RandomUtil.Rand.Next(minValue: mobToSpawn.MinSpawned, maxValue: mobToSpawn.MaxSpawned);
                for (int mob = 0; mob < numberToSpawn; ++mob)
                {
                    var newMob = _mobFactory.Create(mobToSpawn.Class);
                    newMob.Inventory = new Inventory();
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
                    // TODO: modify based on metadata
                    const int HitRadius = 25;
                    const int HitRadiusPow2 = HitRadius * HitRadius;
                    newMob.HitRadius = (int)(HitRadius * newMob.Scale);
                    newMob.HitRadiusPow2 = (int)(HitRadiusPow2 * newMob.Scale);
                    newMob.EntitySpriteKey = mobToSpawn.EntitySpriteKey;
                    newMob.DefaultAnimationKey = _entityData[mobToSpawn.EntitySpriteKey].DefaultAnimationKey;
                    newMob.HP = mobToSpawn.hp;
                    newMob.Hunger = mobToSpawn.Hunger.GetValue();
                    newMob.TileX = spawnTileX;
                    newMob.X = spawnX;
                    newMob.XInt = spawnX;
                    newMob.TileY = spawnTileY;
                    newMob.Y = spawnY;
                    newMob.YInt = spawnY;
                    newMob.HitboxYOffset = -16;
                    newMob.InitDefaultBehaviours();
                    newMob.InitDefaultAnimation(animationOffsetMilliseconds: RandomUtil.Rand.Next(500));
                    Entities.Add(newMob);
                }
            }
        }
    }
}
