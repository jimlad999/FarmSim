﻿using Microsoft.Xna.Framework;
using System;

namespace FarmSim.Entities;

interface IRange
{
    bool InRange<TEntity>(TEntity entity, out int distancePow2)
        where TEntity : Entity;
}

readonly struct ArcRange : IRange
{
    private const double Radians180 = 3.14159;
    private const double Radians360 = Radians180 * 2;
    public readonly int X;
    public readonly int Y;
    public readonly int ReachPow2;
    public readonly bool ArcCrosses0 = false;
    public readonly Vector2 FacingDirection;
    public readonly double FacingDirectionRadians;
    public readonly double FacingDirectionRadiansMin;
    public readonly double FacingDirectionRadiansMax;

    public ArcRange(
        int x,
        int y,
        Vector2 facingDirection,
        double arcHalfRadians,
        int reachPow2)
    {
        X = x;
        Y = y;
        ReachPow2 = reachPow2;
        FacingDirection = facingDirection;
        FacingDirectionRadians = Math.Atan2(y: facingDirection.Y, x: facingDirection.X);
        FacingDirectionRadiansMin = FacingDirectionRadians - arcHalfRadians;
        FacingDirectionRadiansMax = FacingDirectionRadians + arcHalfRadians;
        if (FacingDirectionRadiansMin < -Radians180)
        {
            ArcCrosses0 = true;
            FacingDirectionRadiansMin += Radians360;
        }
        if (FacingDirectionRadiansMax > Radians180)
        {
            ArcCrosses0 = true;
            FacingDirectionRadiansMax -= Radians360;
        }
    }

    // SPEED HACK: return distancePow2 to reduce duplicate calculations in EntityManager.TryFindEntityWithinRangeOrCloseEnoughToBeEnagedInCombat
    public readonly bool InRange<TEntity>(TEntity entity, out int distancePow2)
        where TEntity : Entity
    {
        var xDiff = entity.XInt + entity.HitboxXOffset - X;
        var yDiff = entity.YInt + entity.HitboxYOffset - Y;
        distancePow2 = xDiff * xDiff + yDiff * yDiff - entity.HitRadiusPow2;
        if (distancePow2 > ReachPow2)
        {
            // outside circle altogether
            return false;
        }
        var entityDirectionRadians = Math.Atan2(y: yDiff, x: xDiff);
        return ArcCrosses0
            ? entityDirectionRadians > FacingDirectionRadiansMin
                || entityDirectionRadians < FacingDirectionRadiansMax
            : entityDirectionRadians > FacingDirectionRadiansMin
                && entityDirectionRadians < FacingDirectionRadiansMax;
    }
}
