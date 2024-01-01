using FarmSim.Entities;
using FarmSim.Utils;
using Microsoft.Xna.Framework;

namespace FarmSim.Rendering;

class EntityDurableAnimation : EntityAnimation
{
    public double DurationMilliseconds;
    private readonly bool PlayOnceOnly;

    public EntityDurableAnimation(Entity entity, string animationKey, double durationMilliseconds, bool playOnceOnly)
        : base(entity, animationKey)
    {
        DurationMilliseconds = durationMilliseconds;
        PlayOnceOnly = playOnceOnly;
    }

    public override DelayedAction Update(GameTime gameTime)
    {
        if (PlayOnceOnly)
        {
            return base.Update(gameTime);
        }
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
        var animationFinished = base.UpdateAnimation(gameTime, frames, out activeFrame);
        // If PlayOnce then will finish after first play through. Otherwise animation will cycle till the DurationMilliseconds is exhausted or Entity.FlaggedForDespawn
        return PlayOnceOnly ? animationFinished : false;
    }
}
