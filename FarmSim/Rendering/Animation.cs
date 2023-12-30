using FarmSim.Entities;
using FarmSim.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace FarmSim.Rendering;

abstract class Animation : IPositionable, IDespawnble
{
    public enum DelayedAction
    {
        None,
        KeyFrame,
        After,
    }
    public string SpriteSheetKey;
    public string AnimationKey;
    public virtual FacingDirection FacingDirection { get => FacingDirection.Down; }
    public int ActiveFrameIndex = 0;
    public List<Action> AfterActions = new();
    public List<Action> KeyFrameActions = new();

    public virtual double X { get; set; }
    public virtual double Y { get; set; }
    public virtual int XInt { get; set; }
    public virtual int YInt { get; set; }
    public virtual int TileX { get; set; }
    public virtual int TileY { get; set; }
    public Vector2 Scale = Vector2.One;
    public float Rotation = 0f;
    public Color Color = Color.White;
    public SpriteEffects SpriteEffect = SpriteEffects.None;

    public bool FlagForDespawning { get; set; }

    protected double ActiveFrameDurationMilliseconds = 0;

    public void OnKeyFrame(Action action)
    {
        // Will throw null ref here if Clear() has been called.
        // Don't accept actions after it has been executed.
        // Caller should know better.
        KeyFrameActions.Add(action);
    }

    public void After(Action action)
    {
        // Will throw null ref here if Clear() has been called.
        // Don't accept actions after it has been executed.
        // Caller should know better.
        AfterActions.Add(action);
    }

    public void ClearKeyFrame()
    {
        KeyFrameActions.Clear();
        KeyFrameActions = null;
    }

    public void ClearAfter()
    {
        AfterActions.Clear();
        AfterActions = null;
    }

    public void Clear()
    {
        if (KeyFrameActions != null)
        {
            ClearKeyFrame();
        }
        if (AfterActions != null)
        {
            ClearAfter();
        }
    }

    public virtual DelayedAction Update(GameTime gameTime)
    {
        var frames = GetFrames();
        var animationFinished = UpdateAnimation(gameTime, frames, out var activeFrame);
        if (activeFrame.KeyFrame && KeyFrameActions?.Count > 0)
        {
            return DelayedAction.KeyFrame;
        }
        else if (animationFinished)
        {
            FlagForDespawning = true;
            return DelayedAction.After;
        }
        return DelayedAction.None;
    }

    protected bool UpdateAnimation(GameTime gameTime, FrameData[] frames, out FrameData activeFrame)
    {
        activeFrame = frames[ActiveFrameIndex];
        if (frames.Length == 1)
        {
            // single still sprite
            if (activeFrame.Duration == 0)
            {
                // no animation configured
                return false;
            }
        }
        var cycledThroughAnimations = false;
        ActiveFrameDurationMilliseconds += gameTime.ElapsedGameTime.TotalMilliseconds;
        if (ActiveFrameDurationMilliseconds >= activeFrame.Duration)
        {
            ActiveFrameDurationMilliseconds = 0;
            if (++ActiveFrameIndex >= frames.Length)
            {
                ActiveFrameIndex = 0;
                cycledThroughAnimations = true;
            }
            activeFrame = frames[ActiveFrameIndex];
        }
        return cycledThroughAnimations;
    }

    protected FrameData[] GetFrames()
    {
        if (GlobalState.Tileset.Data.TryGetValue(SpriteSheetKey, out var tileData))
        {
            return tileData.Animations[AnimationKey].DirectionFrames[FacingDirection];
        }
        return GlobalState.EntitiesData.Data[SpriteSheetKey].Animations[AnimationKey].DirectionFrames[FacingDirection];
    }

    protected void InitAnimationOffset(double animationOffset)
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
}