using Microsoft.Xna.Framework;

namespace FarmSim.Rendering;

// positional data is ignored. active frame is used for all 
class GlobalRepeatingAnimation : Animation
{
    public GlobalRepeatingAnimation(string spriteSheetKey, string animationKey)
    {
        SpriteSheetKey = spriteSheetKey;
        AnimationKey = animationKey;
    }

    public override DelayedAction Update(GameTime gameTime)
    {
        UpdateAnimation(gameTime, GetFrames(), out var _);
        return DelayedAction.None;
    }
}
