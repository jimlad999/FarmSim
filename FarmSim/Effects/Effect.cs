using FarmSim.Entities;
using Microsoft.Xna.Framework;

namespace FarmSim.Effects;

abstract class Effect
{
    public string AnimationKey;
    public double DurationMilliseconds;

    public virtual void Apply(Entity entity, Vector2 normalizedDirection)
    {
        // default do nothing
    }
}
