using FarmSim.Entities;
using FarmSim.Utils;
using Microsoft.Xna.Framework;

namespace FarmSim.Player;

interface IAction
{
    void Invoke(Entity entity, int xOffset, int yOffset, Vector2 facingDirection);
}

class FireProjectileActions : IAction
{
    public static ProjectileData Test = new() { Class = "FarmSim.Entities.LinearProjectile, FarmSim", EntitySpriteKey = "magic-missile", Speed = 600, HitRadiusPow2 = 100 /* 10^2 */ };
    public ProjectileData Metadata = Test;

    public void Invoke(Entity entity, int xOffset, int yOffset, Vector2 facingDirection)
    {
        GlobalState.ProjectileManager.CreateProjectile(
            Metadata,
            playerOwned: entity is Player,
            originX: entity.XInt + xOffset,
            originY: entity.YInt + yOffset,
            normalizedDirection: facingDirection);
    }
}
