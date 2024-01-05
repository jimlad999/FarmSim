using FarmSim.Effects;
using FarmSim.Entities;
using FarmSim.Utils;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FarmSim.Projectiles;

class ProjectileManager : EntityManager<Projectile>
{
    private readonly EntityFactory<Projectile, ProjectileData> _projectileFactory;
    private readonly EntityFactory<Effect, EffectData> _effectFactory;
    private readonly Dictionary<string, EntityData> _entityData;

    public ProjectileManager(
        Dictionary<string, EntityData> entityData)
    {
        var data = new ProjectileData[]
        {
            Player.FireProjectileAction.Test
        };
        _projectileFactory = new EntityFactory<Projectile, ProjectileData>(data);
        _effectFactory = new EntityFactory<Effect, EffectData>(data.Select(d => d.Effect).Where(e => e != null).ToArray());
        _entityData = entityData;
    }

    public void Update(GameTime gameTime)
    {
        foreach (var projectile in Entities)
        {
            projectile.Update(gameTime);
            if (!projectile.FlagForDespawning)
            {
                projectile.FlagForDespawning = GlobalState.PlayerManager.OutsideDespawnRadius(projectile)
                    || projectile.Owner is Player.Player
                        ? GlobalState.MobManager.DetectCollision(projectile)
                        : GlobalState.PlayerManager.DetectCollision(projectile);
            }
            if (projectile.FlagForDespawning && projectile.DespawnAnimationKey != null)
            {
                GlobalState.AnimationManager.Generate(x: projectile.XInt, y: projectile.YInt, animationKey: projectile.DespawnAnimationKey, scale: 1f);
            }
        }
        Entities.RemoveAll(mob => mob.FlagForDespawning);
    }

    public void CreateProjectile(
        ProjectileData metadata,
        Entity owner,
        int originX,
        int originY,
        Vector2 normalizedDirection)
    {
        var newProjectileEffect = _effectFactory.Create(metadata.Effect.Class);
        var newProjectile = _projectileFactory.Create(metadata.Class);
        newProjectile.Owner = owner;
        newProjectile.Effect = newProjectileEffect;
        newProjectile.NormalizedDirection = normalizedDirection;
        newProjectile.Speed = metadata.Speed;
        newProjectile.EntitySpriteKey = metadata.EntitySpriteKey;
        newProjectile.DefaultAnimationKey = _entityData[metadata.EntitySpriteKey].DefaultAnimationKey;
        newProjectile.HitRadius = metadata.HitRadius;
        newProjectile.HitRadiusPow2 = metadata.HitRadius * metadata.HitRadius;
        newProjectile.X = originX;
        newProjectile.XInt = originX;
        newProjectile.Y = originY;
        newProjectile.YInt = originY;
        newProjectile.UpdateTileIndex();
        newProjectile.InitDefaultAnimation();
        Entities.Add(newProjectile);
    }
}
