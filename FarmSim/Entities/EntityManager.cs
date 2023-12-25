using FarmSim.Mobs;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace FarmSim.Entities;

class EntityManager
{
    public readonly Player.Player Player;
    public readonly MobManager MobManager;
    public readonly ProjectileManager ProjectileManager;

    public EntityManager(
        Player.Player player,
        MobManager mobManager,
        ProjectileManager projectileManager)
    {
        Player = player;
        MobManager = mobManager;
        ProjectileManager = projectileManager;
    }

    public void Update(GameTime gameTime)
    {
        // player should always win initiative
        Player.Update(gameTime);
        // projectiles next so that player projectiles can hit mobs
        ProjectileManager.Update(gameTime);
        // mobs last
        MobManager.Update(gameTime);
    }

    public void Clear()
    {
        MobManager.Clear();
        ProjectileManager.Clear();
    }

    public IEnumerable<Entity> GetEntitiesInRange(int xTileStart, int xTileEnd, int yTileStart, int yTileEnd)
    {
        return MobManager.GetEntitiesInRange(xTileStart: xTileStart, xTileEnd: xTileEnd, yTileStart: yTileStart, yTileEnd: yTileEnd)
            .Concat(ProjectileManager.GetEntitiesInRange(xTileStart: xTileStart, xTileEnd: xTileEnd, yTileStart: yTileStart, yTileEnd: yTileEnd))
            .Append(Player);
    }
}

class EntityManager<TEntity> where TEntity : Entity
{
    public List<TEntity> Entities = new();

    public IEnumerable<Entity> GetEntitiesInRange(int xTileStart, int xTileEnd, int yTileStart, int yTileEnd)
    {
        return Entities.Where(mob =>
            xTileStart <= mob.TileX && mob.TileX <= xTileEnd
            && yTileStart <= mob.TileY && mob.TileY <= yTileEnd);
    }

    public virtual void Clear()
    {
        Entities = new();
    }
}
