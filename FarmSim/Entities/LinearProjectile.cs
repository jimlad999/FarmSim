using Microsoft.Xna.Framework;

namespace FarmSim.Entities;

class LinearProjectile : Projectile
{
    public override void Update(GameTime gameTime)
    {
        var distancePerFrame = gameTime.ElapsedGameTime.TotalSeconds * Speed;
        X += NormalizedDirection.X * distancePerFrame;
        Y += NormalizedDirection.Y * distancePerFrame;
        XInt = (int)X;
        YInt = (int)Y;
        UpdateTilePosition();
        UpdateFacingDirection(directionX: NormalizedDirection.X, directionY: NormalizedDirection.Y);
    }
}
