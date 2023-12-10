using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;
using Utils;

namespace UI;

[DataContract]
public abstract class SpriteUIElement : UIElement
{
    // texture key lookup in sprite sheet
    [DataMember]
    public string Texture;
    [DataMember]
    public string Width;
    [DataMember]
    public string Height;

    [IgnoreDataMember]
    protected bool TextureStale = true;
    [IgnoreDataMember]
    protected UISpriteSheet.ProcessedData? SpriteSheetData;

    public override void Update(
        GameTime gameTime,
        UIState state,
        UISpriteSheet uiSpriteSheet,
        ControllerManager controllerManager)
    {
        if (Hidden)
        {
            return;
        }
        if (TextureStale && Texture != null)
        {
            SpriteSheetData = uiSpriteSheet[Texture];
            TextureStale = false;
        }
        base.Update(gameTime, state, uiSpriteSheet, controllerManager);
    }

    public override void Draw(SpriteBatch spriteBatch, Rectangle drawArea)
    {
        // Empty, transparent panels can be built without textures. Must specify Height and Width at least.
        // Empty destination is a no-op. Texture probably hasn't loaded or been set or not a transparent panel.
        if (!Hidden && !(SpriteSheetData == null && (Height == null || Width == null)))
        {
            if (DestinationCache == Rectangle.Empty)
            {
                DestinationCache = ComputeScreenDestination(drawArea);
            }
            if (SpriteSheetData != null)
            {
                var data = SpriteSheetData.Value;
                spriteBatch.Draw(
                    data.Texture,
                    destinationRectangle: DestinationCache,
                    sourceRectangle: data.SourceRectangle,
                    Color.White,
                    rotation: 0f,
                    data.Origin,
                    SpriteEffects.None,
                    layerDepth: 0f);
            }
            foreach (var child in Children)
            {
                child.Draw(spriteBatch, DestinationCache);
            }
        }
    }

    private Rectangle ComputeScreenDestination(Rectangle drawArea)
    {
        var height = Height != null ? Utils.ToPixels(Height, drawArea.Height) : SpriteSheetData.Value.SourceRectangle.Height;
        var width = Width != null ? Utils.ToPixels(Width, drawArea.Width) : SpriteSheetData.Value.SourceRectangle.Width;
        var y = Utils.ComputePosition(VerticalAlignment, startValue: Top, endValue: Bottom, thisDimensionSize: height, parentDimensionSize: drawArea.Height);
        var x = Utils.ComputePosition(HorizontalAlignment, startValue: Left, endValue: Right, thisDimensionSize: width, parentDimensionSize: drawArea.Width);

        return new Rectangle(x: drawArea.X + x, y: drawArea.Y + y, width: width, height: height);
    }
}