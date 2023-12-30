using FarmSim.Entities;
using FarmSim.Mobs;
using FarmSim.Rendering;
using FarmSim.Terrain;
using FarmSim.Utils;
using Microsoft.Xna.Framework;

namespace FarmSim.Player;

interface IAction
{
    bool CreatesProjectile { get; }
    void Invoke(Entity entity, int xOffset, int yOffset, Vector2 facingDirection);
}

class FireProjectileActions : IAction
{
    public static ProjectileData Test = new() { Class = "FarmSim.Projectiles.LinearProjectile, FarmSim", Effect = new() { Class = "FarmSim.Projectiles.SmallKnockback, FarmSim" }, EntitySpriteKey = "magic-missile", Speed = 600, HitRadiusPow2 = 100 /* 10^2 */ };
    public ProjectileData Metadata = Test;

    public bool CreatesProjectile => true;

    public void Invoke(Entity entity, int xOffset, int yOffset, Vector2 facingDirection)
    {
        GlobalState.ProjectileManager.CreateProjectile(
            Metadata,
            owner: entity,
            originX: entity.XInt + xOffset,
            originY: entity.YInt + yOffset,
            normalizedDirection: facingDirection);
    }
}

class MultiToolActions : IAction
{
    public bool CreatesProjectile => false;

    public void Invoke(Entity entity, int xOffset, int yOffset, Vector2 facingDirection)
    {
        var entityWithMultiTool = entity as IHasMultiTool;
        if (entityWithMultiTool == null)
        {
            return;
        }
        var weaponRange = entityWithMultiTool.MultiTool.WeaponRange(entity, xOffset: xOffset, yOffset: yOffset, facingDirection);
        if (entity is Player && GlobalState.MobManager.TryFindEntityWithinRangeOrCloseEnoughToBeEnagedInCombat(weaponRange, out var hitMobs))
        {
            var animation = GlobalState.AnimationManager.PlayOnce(entity, "slash");
            animation.OnKeyFrame(() =>
            {
                // Allow for more mobs entering the attack animation after the attack has happened.
                // e.g. the playe is walking towards an enemy and attacks slightly too early.
                GlobalState.MobManager.TryFindEntityWithinRangeOrCloseEnoughToBeEnagedInCombat(weaponRange, out var extraMobs);
                hitMobs.AddRange(extraMobs);
                if (hitMobs.Count > 0)
                {
                    GlobalState.MobManager.Damage(hitMobs, entityWithMultiTool.MultiTool.Damage);
                }
            });
        }
        else if (entity is Mob && GlobalState.PlayerManager.TryFindEntityWithinRangeOrCloseEnoughToBeEnagedInCombat(weaponRange, out var hitPlayers))
        {
            var animation = GlobalState.AnimationManager.PlayOnce(entity, "slash");
            animation.OnKeyFrame(() =>
            {
                // Player walks into an attack
                GlobalState.PlayerManager.TryFindEntityWithinRangeOrCloseEnoughToBeEnagedInCombat(weaponRange, out var extraPlayers);
                hitPlayers.AddRange(extraPlayers);
                if (hitPlayers.Count > 0)
                {
                    GlobalState.PlayerManager.Damage(hitPlayers, entityWithMultiTool.MultiTool.Damage);
                }
            });
        }
        else
        {
            var tile = GlobalState.TerrainManager.GetTileWithinRange(entityWithMultiTool.MultiTool.ToolRange(entity, xOffset: xOffset, yOffset: yOffset, facingDirection));
            var noResourcesFound = true;
            Animation harvestResourceAnimation = null;
            foreach (var resource in tile.GetResources())
            {
                noResourcesFound = false;
                switch (resource.PrimaryTag)
                {
                    case Tags.Wood:
                        harvestResourceAnimation = ChopWood(harvestResourceAnimation, resource, entity, entityWithMultiTool.MultiTool);
                        break;
                    case Tags.Plant:
                        harvestResourceAnimation = HavestPlant(harvestResourceAnimation, resource, entity, entityWithMultiTool.MultiTool);
                        break;
                    case Tags.Liquid:
                        harvestResourceAnimation = CollectLiquid(harvestResourceAnimation, resource, entity, entityWithMultiTool.MultiTool);
                        break;
                    case Tags.Drink:
                        harvestResourceAnimation = CollectLiquid(harvestResourceAnimation, resource, entity, entityWithMultiTool.MultiTool);
                        break;
                    case Tags.Rock:
                        harvestResourceAnimation = MineRock(harvestResourceAnimation, resource, entity, entityWithMultiTool.MultiTool);
                        break;
                    case Tags.Ore:
                        harvestResourceAnimation = MineRock(harvestResourceAnimation, resource, entity, entityWithMultiTool.MultiTool);
                        break;
                    case Tags.Gem:
                        harvestResourceAnimation = MineRock(harvestResourceAnimation, resource, entity, entityWithMultiTool.MultiTool);
                        break;
                };
            }
            if (noResourcesFound)
            {
                // TODO: work out better way of what interacts with multi tool
                if (tile.Terrain == "grass")
                {
                    var animation = GlobalState.AnimationManager.PlayOnce(entity, "till-land");
                    ChangeTile(animation, tile, "farm-land");
                }
                else if (tile.Terrain == "farm-land")
                {
                    var animation = GlobalState.AnimationManager.PlayOnce(entity, "till-land");
                    ChangeTile(animation, tile, "grass");
                }
                else if (tile.Terrain == "water")
                {
                    var animation = GlobalState.AnimationManager.PlayOnce(entity, "bucket");
                    ChangeTile(animation, tile, "rock");
                }
                else
                {
                    // hit nothing but play default animation anyway
                    GlobalState.AnimationManager.PlayOnce(entity, "slash");
                }
            }
        }
    }

    private static Animation ChopWood(Animation animation, Resource resource, Entity entity, MultiTool multiTool)
    {
        animation ??= GlobalState.AnimationManager.PlayOnce(entity, "chop");
        HarvestResource(animation, resource, multiTool);
        return animation;
    }

    private static Animation HavestPlant(Animation animation, Resource resource, Entity entity, MultiTool multiTool)
    {
        animation ??= GlobalState.AnimationManager.PlayOnce(entity, "harvest");
        HarvestResource(animation, resource, multiTool);
        return animation;
    }

    private static Animation CollectLiquid(Animation animation, Resource resource, Entity entity, MultiTool multiTool)
    {
        animation ??= GlobalState.AnimationManager.PlayOnce(entity, "bucket");
        HarvestResource(animation, resource, multiTool);
        return animation;
    }

    private static Animation MineRock(Animation animation, Resource resource, Entity entity, MultiTool multiTool)
    {
        animation ??= GlobalState.AnimationManager.PlayOnce(entity, "mine");
        HarvestResource(animation, resource, multiTool);
        return animation;
    }

    private static void HarvestResource(Animation animation,Resource resource, MultiTool multiTool)
    {
        animation.OnKeyFrame(() =>
        {
            System.Diagnostics.Debug.WriteLine("generating resources");
            GlobalState.TerrainManager.HavestResource(resource, multiTool.HarvestMultiplier);
        });
    }

    private static void ChangeTile(Animation animation, Tile tile, string newTerrain)
    {
        animation.OnKeyFrame(() => GlobalState.TerrainManager.ChangeTile(tile, newTerrain));
    }
}
