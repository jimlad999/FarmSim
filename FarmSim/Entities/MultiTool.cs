﻿using Microsoft.Xna.Framework;

namespace FarmSim.Entities;

class MultiTool
{
    public int HarvestMultiplier = 1;
    public int BaseDamage = 10;
    public int Damage
    {
        get
        {
            return BaseDamage;
        }
    }
    private int ToolRadiusPow2 = 16 * 16;
    private int ToolReach = 32;
    private int WeaponReachPow2 = 70 * 70;
    private double ArcHalfRadians = 0.698132;//+/- 40 degrees of facing direction

    public PointRange ToolRange(Entity entity, int xOffset, int yOffset, Vector2 facingDirection)
    {
        return new PointRange(
            x: entity.XInt + xOffset,
            y: entity.YInt + yOffset,
            facingDirection: facingDirection,
            reach: ToolReach,
            radiusPow2: ToolRadiusPow2);
    }

    public ArcRange WeaponRange(Entity entity, int xOffset, int yOffset, Vector2 facingDirection)
    {
        return new ArcRange(
            x: entity.XInt + xOffset,
            y: entity.YInt + yOffset,
            facingDirection: facingDirection,
            arcHalfRadians: ArcHalfRadians,
            reachPow2: WeaponReachPow2);
    }
}