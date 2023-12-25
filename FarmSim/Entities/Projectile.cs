using Microsoft.Xna.Framework;

namespace FarmSim.Entities;

abstract class Projectile : Entity
{
    public bool PlayerOwned;
    public bool FlagForDespawning;
    public Vector2 NormalizedDirection;
    public double Speed;

    public abstract void Update(GameTime gameTime);

    public bool DetectCollision(Entity entity)
    {
        var xDiff = X - entity.X - entity.HitboxXOffset;
        var yDiff = Y - entity.Y - entity.HitboxYOffset;
        var sumHitRadii = HitRadiusPow2 + entity.HitRadiusPow2;
        var collision = xDiff * xDiff + yDiff * yDiff < sumHitRadii;
        return collision;
    }
}
