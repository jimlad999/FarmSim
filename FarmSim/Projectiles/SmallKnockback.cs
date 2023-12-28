using FarmSim.Entities;

namespace FarmSim.Projectiles;

class SmallKnockback : ProjectileEffect
{
    private const int Force = 200;

    public override void Apply(Entity entity, Projectile projectile)
    {
        entity.ApplyForce(projectile.NormalizedDirection * Force);
    }
}
