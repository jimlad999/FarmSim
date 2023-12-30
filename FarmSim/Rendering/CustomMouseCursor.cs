using FarmSim.Player;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using UI;

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
    private IResult Current;

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

    public void Update(UIOverlay uiOverlay)
    {
        var result = GetCursor(uiOverlay);
        if (!result.Equals(Current))
        {
            Current = result;
            if (result is SystemResult systemResult)
            {
                Mouse.SetCursor(systemResult.MouseCursor);
            }
            else if (result is CustomResult customResult)
            {
                Mouse.SetCursor(MouseCursor.FromTexture2D(customResult.Texture, originx: customResult.OriginX, originy: customResult.OriginY));
            }
        }
    }

    private IResult GetCursor(UIOverlay uiOverlay)
    {
        if (uiOverlay.State.IsMouseOverInteractiveElement)
        {
            return new SystemResult(MouseCursor.Hand);
        }
        if (uiOverlay.State.IsMouseOverElement)
        {
            return new SystemResult(MouseCursor.Arrow);
        }
        var player = GlobalState.PlayerManager.ActivePlayer;
        if (player.TilePlacement != null || player.PrimaryAction == null)
        {
            return new SystemResult(MouseCursor.Arrow);
        }
        if (player.PrimaryAction is FireProjectileAction)
        {
            return new CustomResult(Projectile, 8, 8);
        }
        if (player.PrimaryAction is MultiToolAction)
        {
            switch (player.TelescopeAction.Type)
            {
                case TelescopeResultType.Projectile:
                    return new CustomResult(Projectile, 8, 8);
                case TelescopeResultType.Slash:
                    return new CustomResult(Slash, 0, 0);
                case TelescopeResultType.Bucket:
                    return new CustomResult(Bucket, 0, 0);
                case TelescopeResultType.Chop:
                    return new CustomResult(Chop, 0, 0);
                case TelescopeResultType.Farm:
                    return new CustomResult(Farm, 0, 0);
                case TelescopeResultType.Harvest:
                    return new CustomResult(Harvest, 0, 0);
                case TelescopeResultType.Mine:
                    return new CustomResult(Mine, 0, 0);
            }
        }
        return new SystemResult(MouseCursor.Arrow);
    }

    interface IResult
    {
    }
    struct SystemResult : IResult
    {
        public MouseCursor MouseCursor;

        public SystemResult(MouseCursor mouseCursor)
        {
            MouseCursor = mouseCursor;
        }

        public override bool Equals(object obj)
        {
            return obj is SystemResult result &&
                MouseCursor == result.MouseCursor;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(MouseCursor);
        }
    }
    struct CustomResult : IResult
    {
        public Texture2D Texture;
        public int OriginX;
        public int OriginY;

        public CustomResult(Texture2D texture, int originX, int originY)
        {
            Texture = texture;
            OriginX = originX;
            OriginY = originY;
        }

        public override bool Equals(object obj)
        {
            return obj is CustomResult result &&
                Texture == result.Texture;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Texture);
        }
    }
}
