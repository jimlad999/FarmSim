using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace FarmSim.Entities;

static class EntityManager
{
    public static void Update(GameTime gameTime)
    {
        // player should always win initiative
        GlobalState.PlayerManager.Update(gameTime);
        GlobalState.ItemManager.Update(gameTime);
        // projectiles before mobs so that player projectiles can hit mobs
        GlobalState.ProjectileManager.Update(gameTime);
        // mobs lose inititive
        GlobalState.MobManager.Update(gameTime);
    }

    public static void Reset()
    {
        GlobalState.PlayerManager.Reset();
        GlobalState.ItemManager.Clear();
        GlobalState.ProjectileManager.Clear();
        GlobalState.MobManager.Clear();
    }
}

class EntityManager<TEntity> where TEntity : Entity
{
    private const int CloseEnoughToBeEngagedInCombat = Rendering.Renderer.TileSize * 3;
    private const int CloseEnoughToBeEngagedInCombatPow2 = CloseEnoughToBeEngagedInCombat * CloseEnoughToBeEngagedInCombat;
    public List<TEntity> Entities = new();

    public bool TryFindEntityWithinRangeOrCloseEnoughToBeEnagedInCombat(ArcRange range, out List<TEntity> targets)
    {
        var foundCloseEnoughToBeEngagedInCombat = false;
        targets = Entities.Where(entity =>
        {
            if (range.InRange(entity, out var distancePow2))
            {
                return true;
            }
            if (distancePow2 <= CloseEnoughToBeEngagedInCombatPow2)
            {
                foundCloseEnoughToBeEngagedInCombat = true;
            }
            return false;
        }).ToList();
        return targets.Count > 0 || foundCloseEnoughToBeEngagedInCombat;
    }

    public virtual void Clear()
    {
        Entities = new();
    }
}
