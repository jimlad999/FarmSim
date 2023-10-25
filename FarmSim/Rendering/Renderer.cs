using FarmSim.Terrain;
using FarmSim.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarmSim.Rendering;

class Renderer
{
    public const int TileSize = 64;//px
    private const float TileSizeFloat = TileSize;
    private readonly ViewportManager _viewportManager;
    private readonly TerrainManager _terrainManager;
    private readonly Tileset _tileset;

    public Renderer(
        ViewportManager viewportManager,
        TerrainManager terrainManager,
        Tileset tileset)
    {
        _viewportManager = viewportManager;
        _terrainManager = terrainManager;
        _tileset = tileset;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        var viewport = _viewportManager.Viewport;
        var zoomedTileSize = TileSizeFloat * _viewportManager.Zoom;

        var xOffset = viewport.X.Mod(TileSize);
        var yOffset = viewport.Y.Mod(TileSize);
        int xTileStart = (viewport.X - xOffset) / TileSize;
        int yTileStart = (viewport.Y - yOffset) / TileSize;
        int xTileEnd = (viewport.X + viewport.Width) / TileSize + 1;
        int yTileEnd = (viewport.Y + viewport.Height) / TileSize + 1;
        var xDrawOffset = -xOffset * _viewportManager.Zoom;
        var yDrawOffset = -yOffset * _viewportManager.Zoom;

        float yDraw = (int)yDrawOffset;
        for (var tileY = yTileStart; tileY < yTileEnd; ++tileY)
        {
            float xDraw = (int)xDrawOffset;
            for (var tileX = xTileStart; tileX < xTileEnd; ++tileX)
            {
                var tile = _terrainManager.GetTile(tileX: tileX, tileY: tileY);
                var tileset = _tileset[tile.Terrain];
                spriteBatch.Draw(
                    texture: tileset.Texture,
                    position: new Vector2(xDraw, yDraw),
                    sourceRectangle: tileset.SourceRectangle,
                    color: Color.White,
                    rotation: 0f,
                    origin: tileset.Origin,
                    scale: _viewportManager.Zoom,
                    effects: SpriteEffects.None,
                    layerDepth: 0f);
                if (tile.Trees != null)
                {
                    var tree = _tileset[tile.Trees];
                    spriteBatch.Draw(
                        texture: tree.Texture,
                        position: new Vector2(xDraw, yDraw),
                        sourceRectangle: tree.SourceRectangle,
                        color: Color.White,
                        rotation: 0f,
                        origin: tree.Origin,
                        scale: _viewportManager.Zoom,
                        effects: SpriteEffects.None,
                        layerDepth: 0f);
                }
                if (tile.Ores != null)
                {
                    var ore = _tileset[tile.Ores];
                    spriteBatch.Draw(
                        texture: ore.Texture,
                        position: new Vector2(xDraw, yDraw),
                        sourceRectangle: ore.SourceRectangle,
                        color: Color.White,
                        rotation: 0f,
                        origin: ore.Origin,
                        scale: _viewportManager.Zoom,
                        effects: SpriteEffects.None,
                        layerDepth: 0f);
                }
                xDraw += zoomedTileSize;
            }
            yDraw += zoomedTileSize;
        }
    }
}