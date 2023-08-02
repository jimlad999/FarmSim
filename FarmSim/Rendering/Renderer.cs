﻿using FarmSim.Terrain;
using FarmSim.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace FarmSim.Rendering;

class Renderer
{
    private const int TileSize = 64;//px
    private const float TileSizeFloat = TileSize;

    private readonly ViewportManager _viewportManager;
    private readonly TerrainManager _terrainManager;
    private readonly Dictionary<string, Texture2D> _tileset;

    public Renderer(
        ViewportManager viewportManager,
        TerrainManager terrainManager,
        Dictionary<string, Texture2D> tileset)
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

        System.Diagnostics.Debug.WriteLine((
            xOffset,
            yOffset,
            xDrawOffset,
            yDrawOffset,
            (xTileStart, xTileEnd),
            (yTileStart, yTileEnd),
            viewport,
            _viewportManager.Zoom
        ));
        var yDraw = yDrawOffset;
        for (var tileY = yTileStart; tileY < yTileEnd; ++tileY)
        {
            var xDraw = xDrawOffset;
            for (var tileX = xTileStart; tileX < xTileEnd; ++tileX)
            {
                var tile = _terrainManager.GetTile(tileX: tileX, tileY: tileY);
                spriteBatch.Draw(
                    texture: _tileset[tile.Tileset],
                    position: new Vector2(xDraw, yDraw),
                    sourceRectangle: null,
                    color: Color.White,
                    rotation: 0f,
                    origin: Vector2.Zero,
                    scale: _viewportManager.Zoom,
                    effects: SpriteEffects.None,
                    layerDepth: 0f);
                xDraw += zoomedTileSize;
            }
            yDraw += zoomedTileSize;
        }
    }
}
