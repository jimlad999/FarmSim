﻿using FarmSim.Player;
using FarmSim.Terrain;
using FarmSim.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace FarmSim.Rendering;

class Renderer
{
    public const int TileSize = 64;//px
    private const float TileSizeFloat = TileSize;
    private const int ChunkLOD = 32;
    private const float ChunkLODFloat = ChunkLOD;
    private const float ChunkTerrainLODZoomLevel = 1f / 8f;
    private const float ChunkObjectLODZoomLevel = 1f / 16f;
    private static readonly Color PartialBuildingColor = new Color(127, 127, 127, 127);
    private static readonly Color PartialBuildingInvalidColor = new Color(255, 0, 0, 127);

    private readonly ViewportManager _viewportManager;
    private readonly TerrainManager _terrainManager;
    private readonly Tileset _tileset;
    private readonly Player.Player _player;
    private Dictionary<Chunk, RenderTarget2D> _chunkTilePrerender = new();

    public Renderer(
        ViewportManager viewportManager,
        TerrainManager terrainManager,
        Tileset tileset,
        Player.Player player)
    {
        _viewportManager = viewportManager;
        _terrainManager = terrainManager;
        _tileset = tileset;
        _player = player;
    }

    public void ClearLODCache()
    {
        foreach (var texture in _chunkTilePrerender.Values)
        {
            texture.Dispose();
        }
        _chunkTilePrerender = new();
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        var viewport = _viewportManager.Viewport;
        var zoomedTileSize = TileSizeFloat * _viewportManager.Zoom;

        var xOffset = viewport.X.Mod(TileSize);
        var yOffset = viewport.Y.Mod(TileSize);
        int xTileStart = (viewport.X - xOffset) / TileSize;
        int yTileStart = (viewport.Y - yOffset) / TileSize;
        // +4 to allow for rendering objects taller than a few tile (e.g. trees and buildings)
        int xTileEnd = (viewport.X + viewport.Width) / TileSize + 4;
        int yTileEnd = (viewport.Y + viewport.Height) / TileSize + 4;
        var xDrawOffset = -xOffset * _viewportManager.Zoom;
        var yDrawOffset = -yOffset * _viewportManager.Zoom;
        // TODO: consider cycling unused chunk pre renders to reduce memory footprint. Currently sitting comfortably under 1GB memory (in testing)
        var renderedChunks = new HashSet<Chunk>();

        // pre render any chunks that need to be rendered
        for (var tileY = yTileStart; tileY < yTileEnd;)
        {
            var processYSkip = true;
            int yTilesSkipped = 0;
            for (var tileX = xTileStart; tileX < xTileEnd;)
            {
                var chunk = _terrainManager.GetChunk(tileX: tileX, tileY: tileY);
                if (!_chunkTilePrerender.TryGetValue(chunk, out var _))
                {
                    _chunkTilePrerender[chunk] = GenerateChunkPrerender(spriteBatch, chunk);
                }
                var (xChunkIndex, yChunkIndex) = chunk.GetIndices(tileX: tileX, tileY: tileY);
                tileX += chunk.ChunkSize - xChunkIndex;
                if (processYSkip)
                {
                    processYSkip = false;
                    yTilesSkipped = chunk.ChunkSize - yChunkIndex;
                }
            }
            if (yTilesSkipped != 0)
            {
                tileY += yTilesSkipped;
            }
        }

        spriteBatch.GraphicsDevice.Clear(Color.CornflowerBlue);
        spriteBatch.Begin();
        float yDraw = (int)yDrawOffset;
        for (var tileY = yTileStart; tileY < yTileEnd; ++tileY)
        {
            float xDraw = (int)xDrawOffset;
            var processYSkip = true;
            var yTilesSkipped = 0;
            for (var tileX = xTileStart; tileX < xTileEnd; ++tileX)
            {
                var tile = _terrainManager.GetTile(tileX: tileX, tileY: tileY);
                var (xChunkIndex, yChunkIndex) = tile.Chunk.GetIndices(tileX: tileX, tileY: tileY);
                var chunkSize = tile.Chunk.ChunkSize;
                if (_viewportManager.Zoom < ChunkTerrainLODZoomLevel)
                {
                    if (renderedChunks.Add(tile.Chunk))
                    {
                        DrawChunk(spriteBatch, tile.Chunk, xDraw: xDraw - xChunkIndex * zoomedTileSize, yDraw: yDraw - yChunkIndex * zoomedTileSize);
                    }
                }
                else
                {
                    DrawTileTerrain(spriteBatch, tile, xDraw: xDraw, yDraw: yDraw, scale: _viewportManager.Zoom);
                }
                if (_viewportManager.Zoom >= ChunkObjectLODZoomLevel)
                {
                    DrawTileObjects(spriteBatch, tile, xDraw: xDraw, yDraw: yDraw);
                    if (_player.TilePlacement != null
                        && _player.TilePlacement.TileInRange(tileX: tileX, tileY: tileY))
                    {
                        DrawPartialBuilding(spriteBatch, tile, _player.TilePlacement, xDraw: xDraw, yDraw: yDraw);
                    }
                }
                else
                {
                    var xTilesSkipped = chunkSize - xChunkIndex - 1;
                    tileX += xTilesSkipped;
                    xDraw += zoomedTileSize * xTilesSkipped;
                    if (processYSkip)
                    {
                        processYSkip = false;
                        yTilesSkipped = chunkSize - yChunkIndex - 1;
                    }
                }
                xDraw += zoomedTileSize;
            }
            yDraw += zoomedTileSize;
            if (yTilesSkipped != 0)
            {
                tileY += yTilesSkipped;
                yDraw += zoomedTileSize * yTilesSkipped;
            }
        }
        spriteBatch.End();

    }

    private void DrawChunk(SpriteBatch spriteBatch, Chunk chunk, float xDraw, float yDraw)
    {
        // some off screen chunks (rendering beyond screen boarder to try reduce chance of blank sections being shown when scrolling)
        // won't be generated but this is fine as they are off screen and they will eventually be populated
        if (_chunkTilePrerender.TryGetValue(chunk, out var chunkPrerender))
        {
            spriteBatch.Draw(
                texture: chunkPrerender,
                position: new Vector2(xDraw, yDraw),
                sourceRectangle: null,
                color: Color.White,
                rotation: 0f,
                origin: Vector2.Zero,
                scale: _viewportManager.Zoom * ChunkLODFloat,
                effects: SpriteEffects.None,
                layerDepth: 0f);
        }
    }

    private RenderTarget2D GenerateChunkPrerender(SpriteBatch spriteBatch, Chunk chunk)
    {
        var chunkPrerender = new RenderTarget2D(
            spriteBatch.GraphicsDevice,
            width: chunk.ChunkSize * TileSize / ChunkLOD,
            height: chunk.ChunkSize * TileSize / ChunkLOD);
        using (RenderTargetScope.Create(spriteBatch, chunkPrerender))
        {
            spriteBatch.GraphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin(blendState: BlendState.AlphaBlend);

            float zoomedTileSize = TileSizeFloat / ChunkLODFloat;
            float yDraw = 0;
            foreach (var row in chunk.Tiles)
            {
                float xDraw = 0;
                foreach (var tile in row)
                {
                    DrawTileTerrain(spriteBatch, tile, xDraw: xDraw, yDraw: yDraw, scale: ChunkTerrainLODZoomLevel);
                    xDraw += zoomedTileSize;
                }
                yDraw += zoomedTileSize;
            }

            spriteBatch.End();
        }
        return chunkPrerender;
    }

    private void DrawTileTerrain(SpriteBatch spriteBatch, Tile tile, float xDraw, float yDraw, float scale)
    {
        if (!tile.Buildings.HasFloor)
        {
            var tileset = _tileset[tile.Terrain];
            DrawTileset(
                spriteBatch,
                tileset,
                xDraw: xDraw,
                yDraw: yDraw,
                scale: scale,
                Color.White);
        }
        if (tile.Buildings.Any())
        {
            foreach (var building in tile.Buildings)
            {
                var tileset = _tileset[building];
                DrawTileset(
                    spriteBatch,
                    tileset,
                    xDraw: xDraw,
                    yDraw: yDraw,
                    scale: scale,
                    Color.White);
            }
        }
    }

    private void DrawTileObjects(SpriteBatch spriteBatch, Tile tile, float xDraw, float yDraw)
    {
        if (tile.Trees != null)
        {
            var tree = _tileset[tile.Trees];
            DrawTileset(
                spriteBatch,
                tree,
                xDraw: xDraw,
                yDraw: yDraw,
                scale: _viewportManager.Zoom,
                Color.White);
        }
        if (tile.Ores != null)
        {
            var ore = _tileset[tile.Ores];
            DrawTileset(
                spriteBatch,
                ore,
                xDraw: xDraw,
                yDraw: yDraw,
                scale: _viewportManager.Zoom,
                Color.White);
        }
    }

    private void DrawPartialBuilding(SpriteBatch spriteBatch, Tile tile, ITilePlacement tilePlacement, float xDraw, float yDraw)
    {
        var color = !(tilePlacement.AllTilesBuildable
            || BuildingTypeExtensions.YieldTilesets(tile)
                .All(key => _tileset[key].IsBuildable(tilePlacement.Buildable))
            )
            ? PartialBuildingInvalidColor
            : PartialBuildingColor;
        var building = _tileset[tilePlacement.BuildingTileset];
        DrawTileset(
            spriteBatch,
            building,
            xDraw: xDraw,
            yDraw: yDraw,
            scale: _viewportManager.Zoom,
            color);
    }

    private static void DrawTileset(
        SpriteBatch spriteBatch,
        Tileset.ProcessedTileData tileset,
        float xDraw,
        float yDraw,
        float scale,
        Color color)
    {
        spriteBatch.Draw(
            texture: tileset.Texture,
            position: new Vector2(xDraw, yDraw),
            sourceRectangle: tileset.SourceRectangle,
            color: color,
            rotation: 0f,
            origin: tileset.Origin,
            scale: scale,
            effects: SpriteEffects.None,
            layerDepth: 0f);
    }
}