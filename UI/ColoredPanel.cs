using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;

namespace UI;

[DataContract]
public class ColoredPanel : Panel
{
    // should be set in Game.LoadContent()
#pragma warning disable CA2211 // Non-constant fields should not be visible
    public static Texture2D Pixel;
#pragma warning restore CA2211 // Non-constant fields should not be visible
    private static readonly Rectangle SourceRectangle = new Rectangle(x: 0, y: 0, width: 1, height: 1);

    [DataMember]
    public string Color;
    [DataMember]
    public int Alpha;

    [IgnoreDataMember]
    private Color? ParsedColor;

    public ColoredPanel()
    {
        Texture = "Pixel";
        SpriteSheetData = new UISpriteSheet.ProcessedData(Pixel, SourceRectangle, Vector2.Zero);
        TextureStale = false;
    }

    protected override void DrawTexture(SpriteBatch spriteBatch)
    {
        if (ParsedColor == null)
        {
            ParsedColor = new Color(ColorPalette.Parse(Color), Alpha);
        }
        spriteBatch.Draw(
            Pixel,
            destinationRectangle: DestinationCache,
            sourceRectangle: SourceRectangle,
            color: ParsedColor.Value,
            rotation: 0f,
            origin: Vector2.Zero,
            SpriteEffects.None,
            layerDepth: 0f);
    }
}
