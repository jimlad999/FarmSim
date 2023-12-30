using FarmSim.Player;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FarmSim.Rendering;

class CustomMouseCursor
{
    private readonly Texture2D Bucket;
    private readonly Texture2D Chop;
    private readonly Texture2D Farm;
    private readonly Texture2D Harvest;
    private readonly Texture2D Mine;
    private readonly Texture2D Projectile;
    private readonly Texture2D Slash;
    private Texture2D CurrentCursor;

    public CustomMouseCursor(
        Texture2D bucket,
        Texture2D chop,
        Texture2D farm,
        Texture2D harvest,
        Texture2D mine,
        Texture2D projectile,
        Texture2D slash)
    {
        Bucket = bucket;
        Chop = chop;
        Farm = farm;
        Harvest = harvest;
        Mine = mine;
        Projectile = projectile;
        Slash = slash;
    }

    public void Update()
    {
        var (mouseTexture, originX, originY) = GetPlayerCursor();
        if (CurrentCursor != mouseTexture)
        {
            CurrentCursor = mouseTexture;
            Mouse.SetCursor(MouseCursor.FromTexture2D(CurrentCursor, originX, originY));
        }
    }

    private (Texture2D, int originX, int originY) GetPlayerCursor()
    {
        var player = GlobalState.PlayerManager.ActivePlayer;
        if (player.PrimaryAction == null)
        {
            // points don't matter since cursor won't change
            return (CurrentCursor, 0, 0);
        }
        if (player.PrimaryAction is FireProjectileAction)
        {
            return (Projectile, 8, 8);
        }
        if (player.PrimaryAction is MultiToolAction)
        {
            switch (player.TelescopeAction())
            {
                case TelescopeResult.Projectile:
                    return (Projectile, 8, 8);
                case TelescopeResult.Slash:
                    return (Slash, 0, 0);
                case TelescopeResult.Bucket:
                    return (Bucket, 0, 0);
                case TelescopeResult.Chop:
                    return (Chop, 0, 0);
                case TelescopeResult.Farm:
                    return (Farm, 0, 0);
                case TelescopeResult.Harvest:
                    return (Harvest, 0, 0);
                case TelescopeResult.Mine:
                    return (Mine, 0, 0);
            }
        }
        // points don't matter since cursor won't change
        return (CurrentCursor, 0, 0);
    }
}
