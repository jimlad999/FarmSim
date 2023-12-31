using FarmSim.Entities;
using FarmSim.Utils;
using Microsoft.Xna.Framework;

namespace FarmSim.Rendering;

class EntityDurableAnimation : EntityAnimation
{
    public double DurationMilliseconds;

    public EntityDurableAnimation(Entity entity, string animationKey, double durationMilliseconds)
        : base(entity, animationKey)
    {
        DurationMilliseconds = durationMilliseconds;
    }

    public override DelayedAction Update(GameTime gameTime)
    {
        DurationMilliseconds -= gameTime.ElapsedGameTime.TotalMilliseconds;
        if (DurationMilliseconds <= 0)
        {
            FlagForDespawning = true;
            return DelayedAction.After;
        }
        return base.Update(gameTime);
    }

    protected override bool UpdateAnimation(GameTime gameTime, FrameData[] frames, out FrameData activeFrame)
    {
        base.UpdateAnimation(gameTime, frames, out activeFrame);
        // animation will cycle till the DurationMilliseconds is exhausted or Entity.FlaggedForDespawn
        return false;
    }
}
