﻿using FarmSim.Entities;
using Microsoft.Xna.Framework;

namespace FarmSim.Rendering;

class EntityDefaultAnimation : Animation, IEntityAnimation
{
    public Entity Entity { get; set; }
    public override FacingDirection FacingDirection { get => Entity.FacingDirection; }
    public override double X { get => Entity.X; set { } }
    public override double Y { get => Entity.Y; set { } }
    public override int XInt { get => Entity.XInt; set { } }
    public override int YInt { get => Entity.YInt; set { } }
    public override int TileX { get => Entity.TileX; set { } }
    public override int TileY { get => Entity.TileY; set { } }

    public EntityDefaultAnimation(Entity entity, double animationOffset)
    {
        Entity = entity;
        SpriteSheetKey = entity.EntitySpriteKey;
        AnimationKey = entity.DefaultAnimationKey;
        Scale = new Vector2(entity.Scale, entity.Scale);
        Color = entity.Color;
        InitAnimationOffset(animationOffset);
    }

    private void InitAnimationOffset(double animationOffset)
    {
        var frames = GetFrames();
        var currentFrame = frames[ActiveFrameIndex];
        if (currentFrame.Duration == 0)
        {
            return;
        }
        while (animationOffset >= currentFrame.Duration)
        {
            animationOffset -= currentFrame.Duration;
            if (++ActiveFrameIndex >= frames.Length)
            {
                ActiveFrameIndex = 0;
            }
            currentFrame = frames[ActiveFrameIndex];
        }
        ActiveFrameDurationMilliseconds = animationOffset;
    }

    public override DelayedAction Update(GameTime gameTime)
    {
        FlagForDespawning = Entity is IDespawnble despawnable && despawnable.FlagForDespawning;
        UpdateAnimation(gameTime, GetFrames(), out var _);
        return DelayedAction.None;
    }
}
