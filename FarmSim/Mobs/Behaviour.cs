using Microsoft.Xna.Framework;

namespace FarmSim.Mobs;

abstract class Behaviour
{
    public abstract bool TryExecute(Mob mob, GameTime gameTime);
    public abstract void Reset();
}
