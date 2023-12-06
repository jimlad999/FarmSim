using FarmSim.Player;
using FarmSim.Terrain;
using FarmSim.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils.Rendering;

namespace FarmSim.Rendering;

class Renderer
{
    // Pull these from the tilesets? Should these be configurable?
    public const int TileSize = 64;//px
    public const int TileSizeHalf = TileSize / 2;//px
    private const float TileSizeFloat = TileSize;
    public const int WallHeight = 96;//px
    public const float WallHeightFloat = WallHeight;
    public const float WallPlusTileSizeFloat = WallHeightFloat + TileSizeFloat;
    private const int ChunkLOD = 32;
    private const float ChunkLODFloat = ChunkLOD;
    private const float ChunkTerrainLODZoomLevel = 1f / 8f;
    private const float ChunkObjectLODZoomLevel = 1f / 16f;
    private static readonly Color ExteriorWallTransparency = new Color(15, 15, 15, 127);
    private static readonly Color PartialBuildingColor = new Color(127, 127, 127, 127);
    private static readonly Color PartialBuildingInvalidColor = new Color(255, 0, 0, 127);

    private readonly ViewportManager _viewportManager;
    private readonly TerrainManager _terrainManager;
    private readonly Tileset _tileset;
    private readonly EntitySpriteSheet _entitySpriteSheet;
    private readonly Player.Player _player;
    private Dictionary<Chunk, RenderTarget2D> _chunkTilePrerender = new();

    public Renderer(
        ViewportManager viewportManager,
        TerrainManager terrainManager,
        Tileset tileset,
        EntitySpriteSheet entitySpriteSheet,
        Player.Player player)
    {
        _viewportManager = viewportManager;
        _terrainManager = terrainManager;
        _tileset = tileset;
        _entitySpriteSheet = entitySpriteSheet;
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
        var shouldRenderChunks = _viewportManager.Zoom < ChunkTerrainLODZoomLevel;
        var shouldRenderEntities = _viewportManager.Zoom >= ChunkObjectLODZoomLevel;
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

        var playerXTile = _player.XInt / TileSize;
        if (_player.XInt < 0) --playerXTile;
        var playerYTile = _player.YInt / TileSize;
        if (_player.YInt < 0) --playerYTile;
        // TODO: correctly identify "inside" building". probably same as "has floor"
        var playerIsInsideBuilding = _terrainManager.GetTile(tileX: playerXTile, tileY: playerYTile).Buildings.Any();

        spriteBatch.GraphicsDevice.Clear(Color.CornflowerBlue);
        spriteBatch.Begin();
        float yDraw = (int)yDrawOffset;
        for (var tileY = yTileStart; tileY < yTileEnd; ++tileY)
        {
            float xDraw = (int)xDrawOffset;
            var processedYSkip = true;
            var yTilesSkipped = 0;
            var topYPoint = tileY * TileSize;
            for (var tileX = xTileStart; tileX < xTileEnd; ++tileX)
            {
                var tile = _terrainManager.GetTile(tileX: tileX, tileY: tileY);
                var (xChunkIndex, yChunkIndex) = tile.Chunk.GetIndices(tileX: tileX, tileY: tileY);
                var chunkSize = tile.Chunk.ChunkSize;
                if (shouldRenderChunks)
                {
                    if (renderedChunks.Add(tile.Chunk))
                    {
                        DrawChunk(spriteBatch, tile.Chunk, xDraw: xDraw - xChunkIndex * zoomedTileSize, yDraw: yDraw - yChunkIndex * zoomedTileSize);
                    }
                    var xTilesSkipped = chunkSize - xChunkIndex;
                    tileX += xTilesSkipped;
                    xDraw += zoomedTileSize * xTilesSkipped;
                    if (!shouldRenderEntities && processedYSkip)
                    {
                        processedYSkip = false;
                        yTilesSkipped = chunkSize - yChunkIndex - 1;
                    }
                }
                else
                {
                    DrawTileTerrain(spriteBatch, tile, xDraw: xDraw, yDraw: yDraw, scale: _viewportManager.Zoom, playerIsInsideBuilding);
                    xDraw += zoomedTileSize;
                }
            }
            // render entities after the terrain has finished so we don't clip the sprite when rendering the next tile over
            if (shouldRenderEntities)
            {
                xDraw = (int)xDrawOffset;
                for (var tileX = xTileStart; tileX < xTileEnd; ++tileX)
                {
                    var tile = _terrainManager.GetTile(tileX: tileX, tileY: tileY);
                    var leftXPoint = tileX * TileSize;
                    var entitiesToRenderThisTile = playerYTile == tileY && playerXTile == tileX
                        ? new[] { _player }.OrderBy(e => e.Y).ToArray()
                        : Array.Empty<Player.Player>();
                    var entitiesAlreadyRendered = 0;
                    foreach (var entity in entitiesToRenderThisTile)
                    {
                        var entityDiffFromTopOfRow = topYPoint - entity.YInt;
                        if (entityDiffFromTopOfRow > 0.5)
                            break;
                        var drawXDiff = (leftXPoint - entity.XInt) * _viewportManager.Zoom;
                        var drawYDiff = entityDiffFromTopOfRow * _viewportManager.Zoom;
                        DrawEntity(spriteBatch, entity, xDraw: xDraw - drawXDiff, yDraw: yDraw - drawYDiff, scale: _viewportManager.Zoom);
                        ++entitiesAlreadyRendered;
                    }
                    DrawTileObjects(spriteBatch, tile, xDraw: xDraw, yDraw: yDraw);
                    foreach (var entity in entitiesToRenderThisTile.Skip(entitiesAlreadyRendered))
                    {
                        var drawXDiff = (leftXPoint - entity.XInt) * _viewportManager.Zoom;
                        var drawYDiff = (topYPoint - entity.YInt) * _viewportManager.Zoom;
                        DrawEntity(spriteBatch, entity, xDraw: xDraw - drawXDiff, yDraw: yDraw - drawYDiff, scale: _viewportManager.Zoom);
                    }
                    if (_player.TilePlacement != null
                        && _player.TilePlacement.TileInRange(tileX: tileX, tileY: tileY))
                    {
                        DrawPartialBuilding(spriteBatch, tile, _player.TilePlacement, xDraw: xDraw, yDraw: yDraw);
                    }
                    xDraw += zoomedTileSize;
                }
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
                    DrawTileTerrain(spriteBatch, tile, xDraw: xDraw, yDraw: yDraw, scale: ChunkTerrainLODZoomLevel, playerIsInsideBuilding: false);
                    xDraw += zoomedTileSize;
                }
                yDraw += zoomedTileSize;
            }

            spriteBatch.End();
        }
        return chunkPrerender;
    }

    private void DrawTileTerrain(SpriteBatch spriteBatch, Tile tile, float xDraw, float yDraw, float scale, bool playerIsInsideBuilding)
    {
        if (!tile.Buildings.HasFloor)
        {
            DrawTileset(
                spriteBatch,
                _tileset[tile.Terrain],
                xDraw: xDraw,
                yDraw: yDraw,
                scale: scale,
                Color.White);
        }
        var tileAbove = _terrainManager.GetTile(tile.X, tile.Y - 1);
        // TODO: work out this correctly when multiple kinds of buildings
        var tileAboveHasFloor = tileAbove.Buildings.Any();
        var thisTileHasFloor = tile.Buildings.Any();
        if (tile.Buildings.Any())
        {
            foreach (var building in tile.Buildings)
            {
                DrawTileset(
                    spriteBatch,
                    _tileset[building],
                    xDraw: xDraw,
                    yDraw: yDraw,
                    scale: scale,
                    Color.White);
            }
            if (thisTileHasFloor)
            {
                var floor = tile.Buildings.First();
                if (playerIsInsideBuilding && !tileAboveHasFloor)
                {
                    var wall = $"{floor.Split('-')[0]}-interior-wall";
                    DrawTileset(
                        spriteBatch,
                        _tileset[wall],
                        xDraw: xDraw,
                        yDraw: yDraw - TileSizeFloat * scale,
                        scale: scale,
                        Color.White);
                }
                else if (!playerIsInsideBuilding)
                {
                    var tileBelow = _terrainManager.GetTile(tile.X, tile.Y + 1);
                    var tileBelowHasFloor = tileBelow.Buildings.Any();
                    // defer the roof tile for exterior wall until the exterior wall is drawn
                    if (tileBelowHasFloor)
                    {
                        var roof = $"{floor.Split('-')[0]}-roof";
                        DrawTileset(
                            spriteBatch,
                            _tileset[roof],
                            xDraw: xDraw,
                            yDraw: yDraw - WallHeightFloat * scale,
                            scale: scale,
                            Color.White);
                    }
                }
            }
        }
        // defer drawing exterior walls so that they are always rendered over any entities inside
        if (!thisTileHasFloor && tileAboveHasFloor)
        {
            var floor = tileAbove.Buildings.First();
            var wall = $"{floor.Split('-')[0]}-exterior-wall";
            DrawTileset(
                spriteBatch,
                _tileset[wall],
                xDraw: xDraw,
                yDraw: yDraw - TileSizeFloat * scale,
                scale: scale,
                playerIsInsideBuilding ? ExteriorWallTransparency : Color.White);
            if (!playerIsInsideBuilding)
            {
                var roof = $"{floor.Split('-')[0]}-roof";
                DrawTileset(
                    spriteBatch,
                    _tileset[roof],
                    xDraw: xDraw,
                    yDraw: yDraw - (WallPlusTileSizeFloat) * scale,
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

    private void DrawEntity(SpriteBatch spriteBatch, Player.Player entity, float xDraw, float yDraw, float scale)
    {
        var entitySpriteSheet = _entitySpriteSheet[entity.EntitySpriteKey, entity.FacingDirection];
        DrawEntity(
            spriteBatch,
            entitySpriteSheet,
            xDraw: xDraw,
            yDraw: yDraw,
            scale: scale,
            Color.White);
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

    private static void DrawEntity(
        SpriteBatch spriteBatch,
        EntitySpriteSheet.ProcessedEntityData entity,
        float xDraw,
        float yDraw,
        float scale,
        Color color)
    {
        spriteBatch.Draw(
            texture: entity.Texture,
            position: new Vector2(xDraw, yDraw),
            sourceRectangle: entity.SourceRectangle,
            color: color,
            rotation: 0f,
            origin: entity.Origin,
            scale: scale,
            effects: SpriteEffects.None,
            layerDepth: 0f);
    }
}