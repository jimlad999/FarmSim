using FarmSim.Entities;
using FarmSim.Mobs;
using FarmSim.Player;
using FarmSim.Terrain;
using FarmSim.UI;
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
    public const int TileSizeHalf = TileSize / 2;
    private const float TileSizeFloat = TileSize;
    public const int WallHeight = 96;//px
    public const int WallHeightHalf = WallHeight / 2;
    public const float WallHeightFloat = WallHeight;
    public const float WallPlusTileSizeFloat = WallHeightFloat + TileSizeFloat;
    private const int ChunkLOD = 32;
    private const float ChunkLODFloat = ChunkLOD;
    private const float ChunkTerrainLODZoomLevel = 1f / 8f;
    private const float ChunkObjectLODZoomLevel = 1f / 16f;
    private static readonly Color ExteriorWallTransparency = new Color(15, 15, 15, 127);
    private static readonly Color IndoorWhilePlayerIsOutsideColor = new Color(127, 127, 127, 255);
    private static readonly Color PartialBuildingColor = new Color(127, 127, 127, 255);
    private static readonly Color PartialBuildingInvalidColor = new Color(255, 0, 0, 255);
    private static readonly Color PartialBuildingExteriorWallTransparencyColor = new Color(127, 127, 127, 127);
    private static readonly Color PartialBuildingInvalidExteriorWallTransparencyColor = new Color(255, 0, 0, 127);
    private static readonly Color FogOfWarColor = new Color(0, 0, 0, 200);

    private readonly ViewportManager _viewportManager;
    private readonly TerrainManager _terrainManager;
    private readonly Tileset _tileset;
    private readonly EntitySpriteSheet _entitySpriteSheet;
    private readonly Player.Player _player;
    private readonly MobManager _mobManager;
    private readonly Effect _fogOfWarEffect;
    private readonly Texture2D _fogOfWarOverlay;
    private readonly Texture2D _pixel;
    private Dictionary<Chunk, RenderTarget2D> _chunkTilePrerender = new();

    public Renderer(
        ViewportManager viewportManager,
        TerrainManager terrainManager,
        Tileset tileset,
        EntitySpriteSheet entitySpriteSheet,
        Player.Player player,
        MobManager mobManager,
        Effect fogOfWarEffect,
        Texture2D fogOfWarOverlay,
        Texture2D pixel)
    {
        _viewportManager = viewportManager;
        _terrainManager = terrainManager;
        _tileset = tileset;
        _entitySpriteSheet = entitySpriteSheet;
        _player = player;
        _mobManager = mobManager;
        _fogOfWarEffect = fogOfWarEffect;
        _fogOfWarOverlay = fogOfWarOverlay;
        _pixel = pixel;
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

        var playerIsInsideBuilding = _terrainManager.GetTile(tileX: _player.TileX, tileY: _player.TileY).Buildings.Any(BuildingData.BuildingIsEnclosed);
        var mobLookupByTile = _mobManager.GetEntitiesInRange(xTileStart: xTileStart, xTileEnd: xTileEnd, yTileStart: yTileStart, yTileEnd: yTileEnd)
            .ToLookup(mob => mob.TileY);

        // can set once globally?
        _fogOfWarEffect.Parameters["HalfScreenWidth"].SetValue(spriteBatch.GraphicsDevice.Viewport.Width / 2f);
        _fogOfWarEffect.Parameters["HalfScreenHeight"].SetValue(spriteBatch.GraphicsDevice.Viewport.Height / 2f);
        var fogOfWarRadius = Player.Player.SightRadius * TileSize * _viewportManager.Zoom;
        var fogOfWarRadiusPlusBuffer = fogOfWarRadius + 30 * _viewportManager.Zoom;
        var fogOfWarRadiusPlusBufferPow2 = fogOfWarRadiusPlusBuffer * fogOfWarRadiusPlusBuffer;
        _fogOfWarEffect.Parameters["FogOfWarRadiusPow2"].SetValue(fogOfWarRadiusPlusBufferPow2);
        var fogOfWarStartClipRadiusPlusBuffer = fogOfWarRadius - 30 * _viewportManager.Zoom;
        var fogOfWarStartClipRadiusPlusBufferPow2 = fogOfWarStartClipRadiusPlusBuffer * fogOfWarStartClipRadiusPlusBuffer;
        _fogOfWarEffect.Parameters["FogOfWarStartClipRadiusPow2"].SetValue(fogOfWarStartClipRadiusPlusBufferPow2);
        _fogOfWarEffect.Parameters["FogOfWarRadiusPow2Diff"].SetValue(fogOfWarRadiusPlusBufferPow2 - fogOfWarStartClipRadiusPlusBufferPow2);
        var canSeeFogOfWarEdges = _viewportManager.Zoom < 1;

        spriteBatch.GraphicsDevice.Clear(Color.CornflowerBlue);
        float yDraw = (int)yDrawOffset;
        for (var tileY = yTileStart; tileY < yTileEnd; ++tileY)
        {
            spriteBatch.Begin();
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
                    DrawTileTerrain(spriteBatch, tile, xDraw: xDraw, yDraw: yDraw, scale: _viewportManager.Zoom, _player.TilePlacement, playerIsInsideBuilding);
                    xDraw += zoomedTileSize;
                }
            }
            spriteBatch.End();
            // render entities after the terrain has finished so we don't clip the sprite when rendering the next tile over
            if (shouldRenderEntities)
            {
                spriteBatch.Begin(blendState: BlendState.NonPremultiplied, effect: canSeeFogOfWarEdges ? _fogOfWarEffect : null);
                xDraw = (int)xDrawOffset;
                var entities = mobLookupByTile[tileY];
                if (_player.TileY == tileY)
                {
                    entities = entities.Append(_player);
                }
                entities = entities.Concat(
                    Enumerable.Range(xTileStart, xTileEnd - xTileStart)
                        .SelectMany(tileX => _terrainManager.GetTile(tileX: tileX, tileY: tileY).GetEntities())
                    );
                var leftXPoint = xTileStart * TileSize;
                var entitiesToRenderThisTile = entities.OrderBy(e => e.Y).ToArray();
                foreach (var entity in entitiesToRenderThisTile)
                {
                    var drawXDiff = (leftXPoint - entity.XInt) * _viewportManager.Zoom;
                    var drawYDiff = (topYPoint - entity.YInt) * _viewportManager.Zoom;
                    DrawEntity(spriteBatch, entity, xDraw: xDraw - drawXDiff, yDraw: yDraw - drawYDiff, zoomScale: _viewportManager.Zoom);
                }
                if (_player.TilePlacement != null)
                {
                    if (canSeeFogOfWarEdges)
                    {
                        spriteBatch.End();
                        spriteBatch.Begin();
                    }
                    for (var tileX = xTileStart; tileX < xTileEnd; ++tileX)
                    {
                        if (_player.TilePlacement.TileInRange(tileX: tileX, tileY: tileY))
                        {
                            var tile = _terrainManager.GetTile(tileX: tileX, tileY: tileY);
                            DrawPartialBuilding(spriteBatch, tile, xDraw: xDraw, yDraw: yDraw, _player.TilePlacement, playerIsInsideBuilding);
                        }
                        xDraw += zoomedTileSize;
                    }
                    if (canSeeFogOfWarEdges)
                    {
                        spriteBatch.End();
                        spriteBatch.Begin(blendState: BlendState.NonPremultiplied, effect: canSeeFogOfWarEdges ? _fogOfWarEffect : null);
                    }
                }
                spriteBatch.End();
            }
            yDraw += zoomedTileSize;
            if (yTilesSkipped != 0)
            {
                tileY += yTilesSkipped;
                yDraw += zoomedTileSize * yTilesSkipped;
            }
        }
        if (canSeeFogOfWarEdges)
        {
            var screenWidth = spriteBatch.GraphicsDevice.Viewport.Width;
            var screenHeight = spriteBatch.GraphicsDevice.Viewport.Height;
            spriteBatch.Begin(blendState: BlendState.NonPremultiplied);
            var fogOfWarRadiusInt = (int)fogOfWarRadius;
            var fogOfWarRadiusInt2 = fogOfWarRadiusInt * 2;
            var fogOfWarOverlayDestination = new Rectangle(
                x: screenWidth / 2 - fogOfWarRadiusInt,
                y: screenHeight / 2 - fogOfWarRadiusInt,
                width: fogOfWarRadiusInt2,
                height: fogOfWarRadiusInt2);
            spriteBatch.Draw(
                _fogOfWarOverlay,
                fogOfWarOverlayDestination,
                color: FogOfWarColor);
            var fogOfWarLeft = fogOfWarOverlayDestination.Left;
            if (fogOfWarLeft > 0)
            {
                spriteBatch.Draw(
                    _pixel,
                    new Rectangle(x: 0, y: 0, width: fogOfWarLeft, height: screenHeight),
                    color: FogOfWarColor);
            }
            var fogOfWarRight = fogOfWarOverlayDestination.Right;
            if (fogOfWarRight < screenWidth)
            {
                spriteBatch.Draw(
                    _pixel,
                    new Rectangle(x: fogOfWarRight, y: 0, width: screenWidth - fogOfWarRight, height: screenHeight),
                    color: FogOfWarColor);
            }
            var fogOfWarTop = fogOfWarOverlayDestination.Top;
            if (fogOfWarTop > 0)
            {
                spriteBatch.Draw(
                    _pixel,
                    new Rectangle(x: fogOfWarOverlayDestination.X, y: 0, width: fogOfWarOverlayDestination.Width, height: fogOfWarTop),
                    color: FogOfWarColor);
            }
            var fogOfWarBottom = fogOfWarOverlayDestination.Bottom;
            if (fogOfWarBottom < screenHeight)
            {
                spriteBatch.Draw(
                    _pixel,
                    new Rectangle(x: fogOfWarOverlayDestination.X, y: fogOfWarBottom, width: fogOfWarOverlayDestination.Width, height: screenHeight - fogOfWarBottom),
                    color: FogOfWarColor);
            }
            spriteBatch.End();
        }
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
            float zoomedTileSize = TileSizeFloat / ChunkLODFloat;
            float yDraw = 0;
            foreach (var row in chunk.Tiles)
            {
                float xDraw = 0;
                foreach (var tile in row)
                {
                    DrawTileTerrain(spriteBatch, tile, xDraw: xDraw, yDraw: yDraw, scale: ChunkTerrainLODZoomLevel, tilePlacement: null, playerIsInsideBuilding: false);
                    xDraw += zoomedTileSize;
                }
                yDraw += zoomedTileSize;
            }
        }
        return chunkPrerender;
    }

    private void DrawTileTerrain(SpriteBatch spriteBatch, Tile tile, float xDraw, float yDraw, float scale, ITilePlacement tilePlacement, bool playerIsInsideBuilding)
    {
        var tilePlacementHasFloor = tilePlacement != null && BuildingData.BuildingHasFloor(tilePlacement.BuildingKey);
        string tileAboveFloor;
        var tileAboveInRangeOfPlacement = tilePlacementHasFloor && tilePlacement.TileInRange(tileX: tile.X, tileY: tile.Y - 1);
        // prefer rendering tile placement preview over placed tiles
        if (tileAboveInRangeOfPlacement)
        {
            tileAboveFloor = tilePlacement.BuildingKey;
        }
        else
        {
            var tileAbove = _terrainManager.GetTile(tileX: tile.X, tileY: tile.Y - 1);
            tileAboveFloor = tileAbove.Buildings.FirstOrDefault(BuildingData.BuildingHasFloor);
        }
        string thisTileFloor;
        var thisTileInRangeOfPlacement = tilePlacementHasFloor && tilePlacement.TileInRange(tileX: tile.X, tileY: tile.Y);
        var defaultColor = Color.White;
        if (thisTileInRangeOfPlacement)
        {
            thisTileFloor = tilePlacement.BuildingKey;
        }
        else
        {
            thisTileFloor = tile.Buildings.FirstOrDefault(BuildingData.BuildingHasFloor);
        }
        if (thisTileFloor == null)
        {
            DrawTileset(
                spriteBatch,
                _tileset[tile.Terrain],
                xDraw: xDraw,
                yDraw: yDraw,
                scale: scale,
                color: defaultColor);
        }
        if (tile.Buildings.Any())
        {
            foreach (var buildingKey in tile.Buildings)
            {
                var building = GlobalState.BuildingData.Buildings[buildingKey];
                string tilesetKey = null;
                var color = defaultColor;
                if (building.Floor != null && !thisTileInRangeOfPlacement)
                {
                    tilesetKey = building.Floor;
                    if (!playerIsInsideBuilding)
                    {
                        color = IndoorWhilePlayerIsOutsideColor;
                    }
                }
                // TODO: build stations
                //else
                //{
                //    get station tileset key
                //}
                if (tilesetKey != null)
                {
                    DrawTileset(
                        spriteBatch,
                        _tileset[tilesetKey],
                        xDraw: xDraw,
                        yDraw: yDraw,
                        scale: scale,
                        color: color);
                }
            }
            if (thisTileFloor != null)
            {
                string tileBelowFloor;
                var tileBelowInRangeOfPlacement = tilePlacementHasFloor && tilePlacement.TileInRange(tileX: tile.X, tileY: tile.Y + 1);
                if (tileBelowInRangeOfPlacement)
                {
                    tileBelowFloor = tilePlacement.BuildingKey;
                }
                else
                {
                    var tileBelow = _terrainManager.GetTile(tileX: tile.X, tileY: tile.Y + 1);
                    tileBelowFloor = tileBelow.Buildings.FirstOrDefault(BuildingData.BuildingHasFloor);
                }
                var building = GlobalState.BuildingData.Buildings[thisTileFloor];
                if ((playerIsInsideBuilding || (building.HasTransparency && tileBelowFloor == null)) && tileAboveFloor == null)
                {
                    var interiorWallTilesetKey = building.InteriorWall;
                    if (interiorWallTilesetKey != null)
                    {
                        DrawTileset(
                            spriteBatch,
                            _tileset[interiorWallTilesetKey],
                            xDraw: xDraw,
                            yDraw: yDraw - TileSizeFloat * scale,
                            scale: scale,
                            color: playerIsInsideBuilding
                                ? defaultColor
                                : IndoorWhilePlayerIsOutsideColor);
                    }
                }
                else if (!playerIsInsideBuilding && tileBelowFloor != null)
                {
                    // defer the roof tile for exterior wall until the exterior wall is drawn
                    var roofTilesetKey = GlobalState.BuildingData.Buildings[tileBelowFloor].Roof;
                    if (roofTilesetKey != null)
                    {
                        DrawTileset(
                            spriteBatch,
                            _tileset[roofTilesetKey],
                            xDraw: xDraw,
                            yDraw: yDraw - WallHeightFloat * scale,
                            scale: scale,
                            color: defaultColor);
                    }
                }
            }
        }
        // defer drawing exterior walls so that they are always rendered over any entities inside
        if (thisTileFloor == null && tileAboveFloor != null && !tileAboveInRangeOfPlacement)
        {
            var buildingAbove = GlobalState.BuildingData.Buildings[tileAboveFloor];
            var exteriorWallTilesetKey = buildingAbove.ExteriorWall;
            if (exteriorWallTilesetKey != null)
            {
                DrawTileset(
                    spriteBatch,
                    _tileset[exteriorWallTilesetKey],
                    xDraw: xDraw,
                    yDraw: yDraw - TileSizeFloat * scale,
                    scale: scale,
                    color: playerIsInsideBuilding
                        ? ExteriorWallTransparency
                        : defaultColor);
            }
            if (!playerIsInsideBuilding)
            {
                var roofTilesetKey = buildingAbove.Roof;
                if (roofTilesetKey != null)
                {
                    DrawTileset(
                        spriteBatch,
                        _tileset[roofTilesetKey],
                        xDraw: xDraw,
                        yDraw: yDraw - (WallPlusTileSizeFloat) * scale,
                        scale: scale,
                        color: defaultColor);
                }
            }
        }
    }

    private void DrawEntity(SpriteBatch spriteBatch, Entity entity, float xDraw, float yDraw, float zoomScale)
    {
        var entitySpriteSheet = _entitySpriteSheet[entity.EntitySpriteKey, entity.FacingDirection];
        DrawEntity(
            spriteBatch,
            entitySpriteSheet,
            xDraw: xDraw,
            yDraw: yDraw,
            scale: zoomScale * entity.Scale,
            color: entity.Color);
    }

    private void DrawPartialBuilding(SpriteBatch spriteBatch, Tile tile, float xDraw, float yDraw, ITilePlacement tilePlacement, bool playerIsInsideBuilding)
    {
        var scale = _viewportManager.Zoom;
        var tileIsBuildable = tilePlacement.AllTilesBuildable
            || BuildingExtensions.YieldTilesets(tile)
                .All(key => _tileset[key].IsBuildable(tilePlacement.Buildable));
        var color = tileIsBuildable
            ? PartialBuildingColor
            : PartialBuildingInvalidColor;
        var building = GlobalState.BuildingData.Buildings[tilePlacement.BuildingKey];
        if (playerIsInsideBuilding && building.InteriorWall != null)
        {
            var tileAbove = _terrainManager.GetTile(tileX: tile.X, tileY: tile.Y - 1);
            var tileAboveHasFloor = tileAbove.Buildings.Any(BuildingData.BuildingHasFloor);
            if (!tileAboveHasFloor && tilePlacement.TileTopOfRange(tileX: tile.X, tileY: tile.Y))
            {
                DrawTileset(
                    spriteBatch,
                    _tileset[building.InteriorWall],
                    xDraw: xDraw,
                    yDraw: yDraw - (TileSize) * scale,
                    scale: scale,
                    color);
            }
            if (building.Floor != null)
            {
                DrawTileset(
                    spriteBatch,
                    _tileset[building.Floor],
                    xDraw: xDraw,
                    yDraw: yDraw,
                    scale: scale,
                    color);
            }
            if (tileAboveHasFloor && building.ExteriorWall != null)
            {
                var tileBelow = _terrainManager.GetTile(tileX: tile.X, tileY: tile.Y + 1);
                var tileBelowHasFloor = tileBelow.Buildings.Any(BuildingData.BuildingHasFloor);
                if (!tileBelowHasFloor && tilePlacement.TileBottomOfRange(tileX: tile.X, tileY: tile.Y))
                {
                    DrawTileset(
                        spriteBatch,
                        _tileset[building.ExteriorWall],
                        xDraw: xDraw,
                        yDraw: yDraw,
                        scale: scale,
                        color: tileIsBuildable
                            ? PartialBuildingExteriorWallTransparencyColor
                            : PartialBuildingInvalidExteriorWallTransparencyColor);
                }
            }
        }
        else if (building.ExteriorWall != null)
        {
            if (building.HasTransparency)
            {
                if (building.InteriorWall != null)
                {
                    DrawTileset(
                        spriteBatch,
                        _tileset[building.InteriorWall],
                        xDraw: xDraw,
                        yDraw: yDraw - (TileSize) * scale,
                        scale: scale,
                        color);
                }
                if (building.Floor != null)
                {
                    DrawTileset(
                        spriteBatch,
                        _tileset[building.Floor],
                        xDraw: xDraw,
                        yDraw: yDraw,
                        scale: scale,
                        color);
                }
            }
            if (tilePlacement.TileBottomOfRange(tileX: tile.X, tileY: tile.Y))
            {
                DrawTileset(
                    spriteBatch,
                    _tileset[building.ExteriorWall],
                    xDraw: xDraw,
                    yDraw: yDraw,
                    scale: scale,
                    color);
            }
            if (building.Roof != null)
            {
                DrawTileset(
                    spriteBatch,
                    _tileset[building.Roof],
                    xDraw: xDraw,
                    yDraw: yDraw - (WallHeightFloat) * scale,
                    scale: scale,
                    color);
            }
        }
        else if (building.Floor != null)
        {
            DrawTileset(
                spriteBatch,
                _tileset[building.Floor],
                xDraw: xDraw,
                yDraw: yDraw,
                scale: scale,
                color);
        }
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