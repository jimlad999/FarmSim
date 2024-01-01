using FarmSim.Entities;
using FarmSim.Mobs;
using FarmSim.Rendering;
using FarmSim.Terrain;
using FarmSim.Utils;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FarmSim.Player;

interface IAction
{
    bool CreatesProjectile { get; }
    void Invoke(Entity entity, Tile targetTile, int xOffset, int yOffset, Vector2 facingDirection);
    TelescopeResult Telescope(Entity entity, Tile targetTile, int xOffset, int yOffset, Vector2 facingDirection);
}

enum TelescopeResultType
{
    None,
    Projectile,
    Slash,
    Bucket,
    Chop,
    Farm,
    Harvest,
    Mine,
}

readonly struct TelescopeResult
{
    public static readonly TelescopeResult None = new(TelescopeResultType.None, null, null, () => { });
    public static TelescopeResult Projectile(Action invoke) => new(TelescopeResultType.Projectile, null, null, invoke);
    public static TelescopeResult Slash(IEnumerable<Entity> targetEntities, Action invoke) => new(TelescopeResultType.Slash, targetEntities, null, invoke);
    public static TelescopeResult Bucket(IEnumerable<Entity> targetEntities, Action invoke) => new(TelescopeResultType.Bucket, targetEntities, null, invoke);
    public static TelescopeResult Chop(IEnumerable<Entity> targetEntities, Action invoke) => new(TelescopeResultType.Chop, targetEntities, null, invoke);
    public static TelescopeResult Farm(Tile targetTile, Action invoke) => new(TelescopeResultType.Farm, null, targetTile, invoke);
    public static TelescopeResult Harvest(IEnumerable<Entity> targetEntities, Action invoke) => new(TelescopeResultType.Harvest, targetEntities, null, invoke);
    public static TelescopeResult Mine(IEnumerable<Entity> targetEntities, Action invoke) => new(TelescopeResultType.Mine, targetEntities, null, invoke);

    public readonly TelescopeResultType Type;
    private readonly IEnumerable<Entity> TargetEntities;
    private readonly Tile TargetTile;
    private readonly Action _invoke;

    public TelescopeResult(TelescopeResultType type, IEnumerable<Entity> targetEntities, Tile targetTile, Action invoke)
    {
        Type = type;
        TargetEntities = targetEntities;
        TargetTile = targetTile;
        _invoke = invoke;
    }

    public readonly bool IsTargeting(Entity entity)
    {
        return TargetEntities?.Contains(entity) == true;
    }

    public readonly bool IsTargeting(Tile tile)
    {
        return TargetTile == tile;
    }

    public readonly void Invoke()
    {
        _invoke();
    }
}

class FireProjectileAction : IAction
{
    public static ProjectileData Test = new() { Class = "FarmSim.Projectiles.LinearProjectile, FarmSim", Effect = new() { Class = "FarmSim.Effects.SmallKnockback, FarmSim" }, EntitySpriteKey = "magic-missile", Speed = 600, HitRadius = 10 };
    public ProjectileData Metadata = Test;

    public bool CreatesProjectile => true;

    public void Invoke(Entity entity, Tile targetTile, int xOffset, int yOffset, Vector2 facingDirection)
    {
        Telescope(entity, targetTile, xOffset, yOffset, facingDirection).Invoke();
    }

    public TelescopeResult Telescope(Entity entity, Tile targetTile, int xOffset, int yOffset, Vector2 facingDirection)
    {
        return TelescopeResult.Projectile(() =>
            GlobalState.ProjectileManager.CreateProjectile(
                Metadata,
                owner: entity,
                originX: entity.XInt + xOffset,
                originY: entity.YInt + yOffset,
                normalizedDirection: facingDirection));
    }
}

class MultiToolAction : IAction
{
    public bool CreatesProjectile => false;

    public void Invoke(Entity entity, Tile targetTile, int xOffset, int yOffset, Vector2 facingDirection)
    {
        Telescope(entity, targetTile, xOffset, yOffset, facingDirection).Invoke();
    }

    public TelescopeResult Telescope(Entity entity, Tile targetTile, int xOffset, int yOffset, Vector2 facingDirection)
    {
        if (entity is not IHasMultiTool entityWithMultiTool)
        {
            return TelescopeResult.None;
        }
        var weaponRange = entityWithMultiTool.MultiTool.WeaponRange(entity, xOffset: xOffset, yOffset: yOffset, facingDirection);
        if (entity is Player && GlobalState.MobManager.TryFindEntityWithinRangeOrCloseEnoughToBeEnagedInCombat(weaponRange, out var hitMobs))
        {
            return TelescopeResult.Slash(hitMobs, () =>
            {
                var alreadyProcessed = new List<Mob>();
                GlobalState.AnimationManager.PlayOnce(entity, "slash");
                var animation = GlobalState.AnimationManager.Generate(entity, "slash", 0, playOnceOnly: true, facingDirection, xOffset: xOffset, yOffset: yOffset);
                animation.OnKeyFrame(() =>
                {
                    // Allow for more mobs entering the attack animation after the attack has happened.
                    // e.g. the playe is walking towards an enemy and attacks slightly too early.
                    GlobalState.MobManager.TryFindEntityWithinRangeOrCloseEnoughToBeEnagedInCombat(weaponRange, out var extraMobs);
                    extraMobs.RemoveAll(alreadyProcessed.Contains);
                    hitMobs.AddRange(extraMobs);
                    if (hitMobs.Count > 0)
                    {
                        alreadyProcessed.AddRange(hitMobs);
                        GlobalState.MobManager.Damage(hitMobs.Distinct(), entityWithMultiTool.MultiTool, entity.XInt, entity.YInt);
                        hitMobs.Clear();
                    }
                });
            });
        }
        else if (entity is Mob && GlobalState.PlayerManager.TryFindEntityWithinRangeOrCloseEnoughToBeEnagedInCombat(weaponRange, out var hitPlayers))
        {
            return TelescopeResult.Slash(hitPlayers, () =>
            {
                var alreadyProcessed = new List<Player>();
                GlobalState.AnimationManager.PlayOnce(entity, "slash");
                var animation = GlobalState.AnimationManager.Generate(entity, "slash", 0, playOnceOnly: true, facingDirection, xOffset: xOffset, yOffset: yOffset);
                animation.OnKeyFrame(() =>
                {
                    // Player walks into an attack
                    GlobalState.PlayerManager.TryFindEntityWithinRangeOrCloseEnoughToBeEnagedInCombat(weaponRange, out var extraPlayers);
                    extraPlayers.RemoveAll(alreadyProcessed.Contains);
                    hitPlayers.AddRange(extraPlayers);
                    if (hitPlayers.Count > 0)
                    {
                        alreadyProcessed.AddRange(hitPlayers);
                        GlobalState.PlayerManager.Damage(hitPlayers.Distinct(), entityWithMultiTool.MultiTool.Damage);
                        hitPlayers.Clear();
                    }
                });
            });
        }
        else if (entityWithMultiTool.MultiTool.IsTileWithinRange(entity, targetTile))
        {
            var noResourcesFound = true;
            foreach (var resource in targetTile.GetResources())
            {
                noResourcesFound = false;
                switch (resource.PrimaryTag)
                {
                    case Tags.Wood:
                        return TelescopeResult.Chop(new[] { resource }, () =>
                        {
                            ChopWood(resource, entity, entityWithMultiTool.MultiTool);
                        });
                    case Tags.Plant:
                        return TelescopeResult.Harvest(new[] { resource }, () =>
                        {
                            HavestPlant(resource, entity, entityWithMultiTool.MultiTool);
                        });
                    case Tags.Liquid:
                    case Tags.Drink:
                        return TelescopeResult.Bucket(new[] { resource }, () =>
                        {
                            CollectLiquid(resource, entity, entityWithMultiTool.MultiTool);
                        });
                    case Tags.Rock:
                    case Tags.Ore:
                    case Tags.Gem:
                        return TelescopeResult.Mine(new[] { resource }, () =>
                        {
                            MineRock(resource, entity, entityWithMultiTool.MultiTool);
                        });
                };
            }
            if (noResourcesFound)
            {
                // TODO: work out better way of what interacts with multi tool
                if (targetTile.Terrain == "grass"
                    || targetTile.Terrain == "farm-land")
                {
                    return TelescopeResult.Farm(targetTile, () =>
                    {
                        if (targetTile.Terrain == "grass")
                        {
                            var animation = GlobalState.AnimationManager.PlayOnce(entity, "till-land");
                            ChangeTile(animation, targetTile, "farm-land");
                        }
                        else if (targetTile.Terrain == "farm-land")
                        {
                            var animation = GlobalState.AnimationManager.PlayOnce(entity, "till-land");
                            ChangeTile(animation, targetTile, "grass");
                        }
                    });
                }
                else if (targetTile.Terrain == "water")
                {
                    return TelescopeResult.Bucket(Array.Empty<Entity>(), () =>
                    {
                        var animation = GlobalState.AnimationManager.PlayOnce(entity, "bucket");
                        ChangeTile(animation, targetTile, "rock");
                    });
                }
            }
        }
        return TelescopeResult.Slash(Array.Empty<Entity>(), () =>
        {
            // hit nothing but play default animation anyway
            GlobalState.AnimationManager.PlayOnce(entity, "slash");
            GlobalState.AnimationManager.Generate(entity, "slash", 0, playOnceOnly: true, facingDirection, xOffset: -xOffset, yOffset: yOffset);
        });
    }

    private static Animation ChopWood(Resource resource, Entity entity, MultiTool multiTool)
    {
        var animation = GlobalState.AnimationManager.PlayOnce(entity, "chop");
        HarvestResource(animation, resource, multiTool);
        return animation;
    }

    private static Animation HavestPlant(Resource resource, Entity entity, MultiTool multiTool)
    {
        var animation = GlobalState.AnimationManager.PlayOnce(entity, "harvest");
        HarvestResource(animation, resource, multiTool);
        return animation;
    }

    private static Animation CollectLiquid(Resource resource, Entity entity, MultiTool multiTool)
    {
        var animation = GlobalState.AnimationManager.PlayOnce(entity, "bucket");
        HarvestResource(animation, resource, multiTool);
        return animation;
    }

    private static Animation MineRock(Resource resource, Entity entity, MultiTool multiTool)
    {
        var animation = GlobalState.AnimationManager.PlayOnce(entity, "mine");
        HarvestResource(animation, resource, multiTool);
        return animation;
    }

    private static void HarvestResource(Animation animation, Resource resource, MultiTool multiTool)
    {
        animation.OnKeyFrame(() => GlobalState.TerrainManager.HavestResource(resource, multiTool.HarvestMultiplier));
    }

    private static void ChangeTile(Animation animation, Tile tile, string newTerrain)
    {
        animation.OnKeyFrame(() => GlobalState.TerrainManager.ChangeTile(tile, newTerrain));
    }
}
