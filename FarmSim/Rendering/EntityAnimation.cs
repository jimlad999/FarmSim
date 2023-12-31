using FarmSim.Entities;
using Microsoft.Xna.Framework;

namespace FarmSim.Rendering;

class EntityAnimation : Animation, IEntityAnimation
{
    public EntityAnimation(Entity entity, string animationKey)
    {
        Entity = entity;
        FacingDirection = entity.FacingDirection;
        SpriteSheetKey = entity.EntitySpriteKey;
        AnimationKey = animationKey;
        Scale = new Vector2(entity.Scale, entity.Scale);
        Color = entity.Color;
    }

    public Entity Entity { get; set; }
    public override FacingDirection FacingDirection { get; }

    public override double X { get => Entity.X; set { } }
    public override double Y { get => Entity.Y; set { } }
    public override int XInt { get => Entity.XInt; set { } }
    public override int YInt { get => Entity.YInt; set { } }
    public override int TileX { get => Entity.TileX; set { } }
    public override int TileY { get => Entity.TileY; set { } }

    public override DelayedAction Update(GameTime gameTime)
    {
        if (Entity is IDespawnble despawnable && despawnable.FlagForDespawning)
        {
            FlagForDespawning = true;
            return DelayedAction.After;
        }
        return base.Update(gameTime);
    }
}
