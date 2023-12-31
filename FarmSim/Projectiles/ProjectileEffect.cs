using FarmSim.Entities;

namespace FarmSim.Projectiles;

abstract class ProjectileEffect
{
    public string AnimationKey;
    public double DurationMilliseconds;

    public virtual void Apply(Entity entity, Projectile projectile)
    {
        // default do nothing
    }
}
