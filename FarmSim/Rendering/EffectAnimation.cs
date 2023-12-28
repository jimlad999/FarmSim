namespace FarmSim.Rendering;

class EffectAnimation : Animation
{
    public const string DefaultSpriteSheetKey = "effects";

    public EffectAnimation(string animationKey)
    {
        SpriteSheetKey = DefaultSpriteSheetKey;
        AnimationKey = animationKey;
    }
}
