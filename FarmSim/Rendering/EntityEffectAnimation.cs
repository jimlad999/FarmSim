using FarmSim.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace FarmSim.Rendering;

class EntityEffectAnimation : EntityDurableAnimation
{
    public EntityEffectAnimation(Entity entity, string animationKey, double durationMilliseconds, bool playOnceOnly, Vector2 direction)
        : base(entity, animationKey, durationMilliseconds, playOnceOnly)
    {
        // TODO: decide if simple rotation or whether to scale based on entity.FacingDirection and direction.
        // e.g. a radial slash can simply be rotated, but a downward pick axe animation will need to scale based on the above.
        // -90 degrees because we expect the default animation to be drawn facing downwards
        Rotation = (float)Math.Atan2(y: direction.Y, x: direction.X) - 1.5708f;
        // TODO: flip sprites based on direction.X < 0 OR > 0
        SpriteEffect = SpriteEffects.None;
        SpriteSheetKey = EffectAnimation.DefaultSpriteSheetKey;
        // Reset color back to white regardless of Entity.Color. Effects are already the correct color in the sprite sheet.
        Color = Color.White;
    }
}
