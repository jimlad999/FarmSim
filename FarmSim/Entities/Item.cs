using FarmSim.Utils;
using Microsoft.Xna.Framework;
using System;
using System.Runtime.CompilerServices;

namespace FarmSim.Entities;

class Item : Entity, IHasHeight, IDespawnble
{
    // Consider making this variable based on item?
    private const double InitialHorizontalSpeed = 100;
    [ThreadStatic]
    private static int InitialVerticalSpeedsIndex = 0;
    private static readonly double[] InitialVerticalSpeeds = new double[] { 120, 140, 130, 150 };
    private const double InitialGavity = 250;
    // Once speed drops below this, consider the item "stationary"
    private const double LowestHorizontalSpeed = 10;

    public double HeightOffGround { get; set; } = 0;
    public int HeightOffGroundInt { get; set; } = 0;

    public Vector2 NormalizedDirection;
    public double Speed;
    public double VerticalSpeed;
    public double VerticalSpeedOnBounce;
    public double Gavity;

    // Data about how to construct/build this kind of item (e.g. type, available tags, quality range).
    public ItemData Metadata;
    // Data about this instance of item (e.g. type, chosen tags, chosen quality).
    // This is what is added to the player inventory.
    public ItemInfo InstanceInfo;
    // Player who threw the item into the world.
    public Player.Player Owner;
    public double SecondsInWorld;
    internal double PickUpDelayTimeMilliseconds;

    public bool FlagForDespawning { get; set; } = false;

    public Item()
    {
        Speed = InitialHorizontalSpeed;
        VerticalSpeed = InitialVerticalSpeeds[InitialVerticalSpeedsIndex++];
        if (InitialVerticalSpeedsIndex >= InitialVerticalSpeeds.Length) InitialVerticalSpeedsIndex = 0;
        VerticalSpeedOnBounce = CalculateVerticalSpeedOnBounce(VerticalSpeed);
        Gavity = InitialGavity;
    }

    public void Update(GameTime gameTime)
    {
        SecondsInWorld += gameTime.ElapsedGameTime.TotalSeconds;
        if (PickUpDelayTimeMilliseconds > 0)
        {
            PickUpDelayTimeMilliseconds -= gameTime.ElapsedGameTime.TotalMilliseconds;
        }
        UpdateMovement(gameTime);
    }

    private void UpdateMovement(GameTime gameTime)
    {
        if (Speed == 0)
        {
            return;
        }
        var distancePerFrame = Speed * gameTime.ElapsedGameTime.TotalSeconds;
        X += NormalizedDirection.X * distancePerFrame;
        Y += NormalizedDirection.Y * distancePerFrame;
        XInt = (int)X;
        YInt = (int)Y;
        this.UpdateTileIndex();
        HeightOffGround += VerticalSpeed * gameTime.ElapsedGameTime.TotalSeconds;
        HeightOffGroundInt = (int)HeightOffGround;
        VerticalSpeed -= Gavity * gameTime.ElapsedGameTime.TotalSeconds;
        if (HeightOffGroundInt <= 0)
        {
            HeightOffGround = 0;
            HeightOffGroundInt = 0;
            Gavity /= 1.5;
            // ground friction (ignore air friction - may adjust later to "look right")
            Speed /= 2;
            if (Speed < LowestHorizontalSpeed)
            {
                Speed = 0;
                VerticalSpeed = 0;
                VerticalSpeedOnBounce = 0;
            }
            else
            {
                VerticalSpeed = VerticalSpeedOnBounce;
                VerticalSpeedOnBounce = CalculateVerticalSpeedOnBounce(VerticalSpeed);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double CalculateVerticalSpeedOnBounce(double verticalSpeed)
    {
        return verticalSpeed / 1.5;
    }
}
