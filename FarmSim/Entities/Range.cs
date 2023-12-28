﻿using Microsoft.Xna.Framework;
using System;

namespace FarmSim.Entities;

interface IRange
{
    bool InRange<TEntity>(TEntity entity, out int distancePow2)
        where TEntity : Entity;
}

readonly struct PointRange : IRange
{
    public readonly int OriginX;
    public readonly int ReachX;
    public readonly int OriginY;
    public readonly int ReachY;
    public readonly Vector2 FacingDirection;
    public readonly int Reach;
    public readonly int RadiusPow2;

    public PointRange(
        int x,
        int y,
        Vector2 facingDirection,
        int reach,
        int radiusPow2)
    {
        OriginX = x;
        ReachX = (int)(x + facingDirection.X * reach);
        OriginY = y;
        ReachY = (int)(y + facingDirection.Y * reach);
        FacingDirection = facingDirection;
        Reach = reach;
        RadiusPow2 = radiusPow2;
    }

    public readonly bool InRange<TEntity>(TEntity entity, out int distancePow2) where TEntity : Entity
    {
        var xDiff = entity.XInt - ReachX;
        var yDiff = entity.YInt - ReachY;
        distancePow2 = xDiff * xDiff + yDiff * yDiff;
        return distancePow2 > RadiusPow2;
    }
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
        System.Diagnostics.Debug.WriteLine((
            "yDiff", yDiff,
            "xDiff", xDiff,
            "crosses0", ArcCrosses0,
            "rad", Math.Round(entityDirectionRadians, 5),
            "min", Math.Round(FacingDirectionRadiansMin, 5),
            "max", Math.Round(FacingDirectionRadiansMax, 5)));
        return ArcCrosses0
            ? entityDirectionRadians > FacingDirectionRadiansMin
                || entityDirectionRadians < FacingDirectionRadiansMax
            : entityDirectionRadians > FacingDirectionRadiansMin
                && entityDirectionRadians < FacingDirectionRadiansMax;
    }
}