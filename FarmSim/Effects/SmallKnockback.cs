using FarmSim.Entities;
using Microsoft.Xna.Framework;

namespace FarmSim.Effects;

class SmallKnockback : Effect
{
    private const int Force = 200;

    public override void Apply(Entity entity, Vector2 normalizedDirection)
    {
        entity.ApplyForce(normalizedDirection * Force);
    }
}
