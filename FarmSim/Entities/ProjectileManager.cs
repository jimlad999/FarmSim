using FarmSim.Mobs;
using FarmSim.Utils;
using Microsoft.Xna.Framework;
using System;

namespace FarmSim.Entities;

class ProjectileManager : EntityManager<Projectile>
{
    private readonly Player.Player _player;
    private readonly MobManager _mobManager;
    private readonly EntityFactory<Projectile, ProjectileData> _projectileFactory;

    public ProjectileManager(
        Player.Player player,
        MobManager mobManager)
    {
        _player = player;
        _mobManager = mobManager;
        _projectileFactory = new EntityFactory<Projectile, ProjectileData>(new ProjectileData[]
        {
            Player.FireProjectileActions.Test
        });
    }

    public void Update(GameTime gameTime)
    {
        foreach (var projectile in Entities)
        {
            projectile.Update(gameTime);
            projectile.FlagForDespawning = Math.Abs(projectile.TileX - _player.TileX) > MobManager.DespawnRadius
                || Math.Abs(projectile.TileY - _player.TileY) > MobManager.DespawnRadius;
            if (!projectile.FlagForDespawning)
            {
                if (projectile.PlayerOwned)
                {
                    projectile.FlagForDespawning = _mobManager.DetectCollision(projectile);
                }
                else // mob owned
                {
                    projectile.FlagForDespawning = projectile.DetectCollision(_player);
                    // TODO: damage player
                }
            }
        }
        Entities.RemoveAll(mob => mob.FlagForDespawning);
    }

    public void CreateProjectile(
        ProjectileData metadata,
        bool playerOwned,
        int originX,
        int originY,
        Vector2 normalizedDirection)
    {
        var newProjectile = _projectileFactory.Create(metadata.Class);
        newProjectile.PlayerOwned = playerOwned;
        newProjectile.NormalizedDirection = normalizedDirection;
        newProjectile.Speed = metadata.Speed;
        newProjectile.EntitySpriteKey = metadata.EntitySpriteKey;
        newProjectile.HitRadiusPow2 = metadata.HitRadiusPow2;
        newProjectile.X = originX;
        newProjectile.XInt = originX;
        newProjectile.Y = originY;
        newProjectile.YInt = originY;
        newProjectile.UpdateTilePosition();
        Entities.Add(newProjectile);
    }
}
