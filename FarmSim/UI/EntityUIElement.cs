using FarmSim.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UI;
using UIUtils = UI.Utils;

namespace FarmSim.UI;

class EntityUIElement : UIElement
{
    private readonly string SpriteSheetKey;
    private readonly EntityData EntityData;
    private readonly Color Color;

    public EntityUIElement(string spriteSheetKey, EntityData entityData, Color color)
    {
        SpriteSheetKey = spriteSheetKey;
        EntityData = entityData;
        Color = color;
    }

    public override Rectangle PreComputeDestinationCache(Rectangle drawArea, Point offset)
    {
        var animationData = EntityData.Animations[EntityData.DefaultAnimationKey];
        var width = animationData.FrameWidth;
        var height = animationData.FrameHeight;
        var y = UIUtils.ComputePosition(VerticalAlignment, startValue: Top, endValue: Bottom, thisDimensionSize: height, parentDimensionSize: drawArea.Height);
        var x = UIUtils.ComputePosition(HorizontalAlignment, startValue: Left, endValue: Right, thisDimensionSize: width, parentDimensionSize: drawArea.Width);
        return new Rectangle(
            x: drawArea.X + offset.X + x,
            y: drawArea.Y + offset.Y + y,
            width: width,
            height: height);
    }

    public override void Draw(SpriteBatch spriteBatch, Rectangle drawArea, Point offset)
    {
        base.Draw(spriteBatch, drawArea, offset);
        var animation = GlobalState.AnimationManager.EntityAnimations[SpriteSheetKey];
        DrawSprite(spriteBatch, DestinationCache.Location.ToVector2(), scale: 1f, GlobalState.EntitySpriteSheet[animation]);
    }

    private void DrawSprite(SpriteBatch spriteBatch, Vector2 position, float scale, ProcessedSpriteData spriteData)
    {
        spriteBatch.Draw(
            texture: spriteData.Texture,
            position: position,
            sourceRectangle: spriteData.SourceRectangle,
            color: Color,
            rotation: 0f,
            // PreComputeDestinationCache removes the need to use the origin value
            origin: Vector2.Zero,
            scale: scale,
            effects: SpriteEffects.None,
            layerDepth: 0f);
    }
}
