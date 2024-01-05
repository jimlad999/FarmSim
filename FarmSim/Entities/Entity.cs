using Microsoft.Xna.Framework;
using System;

namespace FarmSim.Entities;

abstract class Entity : IPositionable
{
    private const int MinForcePow2 = 10 * 10;

    public FacingDirection FacingDirection = FacingDirection.Down;

    public int Height;
    public int HitRadius;
    public int HitRadiusPow2;

    // world position
    public double X { get; set; }
    public double Y { get; set; }
    public int XInt { get; set; }
    public int YInt { get; set; }
    // tile index
    public int TileX { get; set; }
    public int TileY { get; set; }

    public string EntitySpriteKey;
    public string DefaultAnimationKey;
    public float Scale = 1f;
    public Color Color = Color.White;

    public int HitboxXOffset = 0;
    public int HitboxYOffset = 0;

    private Vector2 ExternalForce;

    protected void UpdateFacingDirection(double directionX, double directionY)
    {
        FacingDirection = Math.Abs(directionX) > Math.Abs(directionY)
            ? directionX < 0 ? FacingDirection.Left : FacingDirection.Right
            : directionY < 0 ? FacingDirection.Up : FacingDirection.Down;
    }

    // Animation offset is used to make it so entities spawned at the same time don't have the exact same animation timing.
    // This is to ensure as much as possible there is no uniform animations close by to make it appear more natural.
    public virtual void InitDefaultAnimation(double animationOffsetMilliseconds = 0)
    {
        var defaultAnimation = GlobalState.AnimationManager.InitDefault(this, animationOffsetMilliseconds);
        Height = defaultAnimation.GetFrameHeight();
    }

    public void ApplyForce(Vector2 externalForce)
    {
        ExternalForce = externalForce;
    }

    protected bool UpdateForces(GameTime gameTime, ref double x, ref double y)
    {
        if (ExternalForce == Vector2.Zero)
        {
            return false;
        }
        x += ExternalForce.X * gameTime.ElapsedGameTime.TotalSeconds;
        y += ExternalForce.Y * gameTime.ElapsedGameTime.TotalSeconds;
        ExternalForce *= 0.8f;
        if (ExternalForce.LengthSquared() < MinForcePow2)
        {
            ExternalForce = Vector2.Zero;
        }
        return true;
    }
}
