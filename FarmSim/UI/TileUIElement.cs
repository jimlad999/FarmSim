using FarmSim.Rendering;
using FarmSim.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UI;
using UIUtils = UI.Utils;

namespace FarmSim.UI;

class TileUIElement : UIElement
{
    private const float DefaultScale = 0.5f;
    private string Roof;
    private string ExteriorWall;
    private string InteriorWall;
    private string Floor;
    private bool HasTransparency;
    private bool FloorOnly;

    public TileUIElement(
        string roof,
        string exteriorWall,
        string interiorWall,
        string floor,
        bool hasTransparency)
    {
        Roof = roof;
        ExteriorWall = exteriorWall;
        InteriorWall = interiorWall;
        Floor = floor;
        HasTransparency = hasTransparency;
        FloorOnly = Floor != null && ExteriorWall == null && InteriorWall == null && Roof == null;
    }

    public override Rectangle PreComputeDestinationCache(Rectangle drawArea, Point offset)
    {
        var height = FloorOnly
            ? Renderer.TileSize
            : (ExteriorWall != null || InteriorWall != null ? Renderer.WallHeightHalf : 0)
                + (Roof != null || Floor != null ? Renderer.TileSizeHalf : 0);
        var width = FloorOnly ? Renderer.TileSize : Renderer.TileSizeHalf;
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
        if (Hidden)
        {
            return;
        }
        if (DestinationCache == Rectangle.Empty || CachedDrawArea != drawArea || CachedOffset != offset)
        {
            CachedDrawArea = drawArea;
            CachedOffset = offset;
            DestinationCache = PreComputeDestinationCache(drawArea, offset);
        }
        if (!drawArea.Intersects(DestinationCache))
        {
            return;
        }
        if (FloorOnly)
        {
            var position = DestinationCache.Location.ToVector2();
            DrawTile(spriteBatch, position, scale: 1.0f, GlobalState.Tileset[Floor]);
        }
        else
        {
            if (InteriorWall != null && (ExteriorWall == null || HasTransparency))
            {
                const int InteriorWallOffsetFromBottom = Renderer.TileSizeHalf + Renderer.WallHeightHalf;
                var position = new Vector2(x: DestinationCache.Left, y: DestinationCache.Bottom - InteriorWallOffsetFromBottom);
                DrawTile(spriteBatch, position, scale: DefaultScale, GlobalState.Tileset[InteriorWall]);
            }
            if (Floor != null && (ExteriorWall == null || HasTransparency))
            {
                const int FloorOffsetFromBottom = Renderer.WallHeightHalf;
                var position = new Vector2(x: DestinationCache.Left, y: DestinationCache.Bottom - FloorOffsetFromBottom);
                DrawTile(spriteBatch, position, scale: DefaultScale, GlobalState.Tileset[Floor]);
            }
            if (ExteriorWall != null)
            {
                const int ExteriorWallOffsetFromBottom = Renderer.WallHeightHalf;
                var position = new Vector2(x: DestinationCache.Left, y: DestinationCache.Bottom - ExteriorWallOffsetFromBottom);
                DrawTile(spriteBatch, position, scale: DefaultScale, GlobalState.Tileset[ExteriorWall]);
            }
            if (Roof != null)
            {
                const int RoofOffsetFromBottom = Renderer.WallHeightHalf + Renderer.WallHeightHalf;
                var position = new Vector2(x: DestinationCache.Left, y: DestinationCache.Bottom - RoofOffsetFromBottom);
                DrawTile(spriteBatch, position, scale: DefaultScale, GlobalState.Tileset[Roof]);
            }
        }
    }

    private void DrawTile(SpriteBatch spriteBatch, Vector2 position, float scale, Tileset.ProcessedTileData roof)
    {
        spriteBatch.Draw(
            texture: roof.Texture,
            position: position,
            sourceRectangle: roof.SourceRectangle,
            color: Color.White,
            rotation: 0f,
            origin: roof.Origin,
            scale: scale,
            effects: SpriteEffects.None,
            layerDepth: 0f);
    }
}
