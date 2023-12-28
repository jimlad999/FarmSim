using FarmSim.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace FarmSim.Rendering;

class EntityEffectAnimation : EntityAnimation
{
    public EntityEffectAnimation(Entity entity, string animationKey, Vector2 direction)
        : base(entity, animationKey)
    {
        // TODO: decide if simple rotation or whether to scale based on entity.FacingDirection and direction.
        // e.g. a radial slash can simply be rotated, but a downward pick axe animation will need to scale based on the above.
        Rotation = (float)Math.Atan2(y: direction.Y, x: direction.X);
        // TODO: flip sprites based on direction.X < 0 OR > 0
        SpriteEffect = SpriteEffects.None;
        SpriteSheetKey = EffectAnimation.DefaultSpriteSheetKey;
    }
}
