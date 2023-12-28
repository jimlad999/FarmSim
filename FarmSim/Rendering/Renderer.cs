using FarmSim.Entities;
using FarmSim.Player;
using FarmSim.Terrain;
using FarmSim.UI;
using FarmSim.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Utils.Rendering;
using Effect = Microsoft.Xna.Framework.Graphics.Effect;

namespace FarmSim.Rendering;

class Renderer
{
    // Pull these from the tilesets? Should these be configurable?
    public const int TileSize = 64;//px
    public const int TileSizeHalf = TileSize / 2;
    public const float TileSizeFloat = TileSize;
    public const int WallHeight = 96;//px
    public const int WallHeightHalf = WallHeight / 2;
    public const float WallHeightFloat = WallHeight;
    public const float WallPlusTileSizeFloat = WallHeightFloat + TileSizeFloat;
    private const int ChunkLOD = 32;
    private const float ChunkLODFloat = ChunkLOD;
    private const float ChunkTerrainLODZoomLevel = 1f / 8f;
    private const float ChunkObjectLODZoomLevel = 1f / 16f;
    private static readonly Color ItemShadow = new Color(0, 0, 0, 127);
    private static readonly Color ExteriorWallTransparency = new Color(15, 15, 15, 127);
    private static readonly Color IndoorWhilePlayerIsOutsideColor = new Color(127, 127, 127, 255);
    private static readonly Color PartialBuildingColor = new Color(127, 127, 127, 255);
    private static readonly Color PartialBuildingInvalidColor = new Color(255, 0, 0, 255);
    private static readonly Color PartialBuildingExteriorWallTransparencyColor = new Color(127, 127, 127, 127);
    private static readonly Color PartialBuildingInvalidExteriorWallTransparencyColor = new Color(255, 0, 0, 127);
    private static readonly Color FogOfWarColor = new Color(0, 0, 0, 200);

#pragma warning disable CA2211 // Non-constant fields should not be visible
    public static bool RenderFogOfWar = true;
#pragma warning restore CA2211 // Non-constant fields should not be visible

    private readonly ViewportManager _viewportManager;
    private readonly Tileset _tileset;
    private readonly EntitySpriteSheet _entitySpriteSheet;
    private readonly Effect _fogOfWarEffect;
    private readonly Effect _fogOfWarInverseEffect;
    private readonly Texture2D _pixel;
    private Dictionary<Chunk, RenderTarget2D> _chunkTilePrerender = new();

    public Renderer(
        ViewportManager viewportManager,
        Tileset tileset,
        EntitySpriteSheet entitySpriteSheet,
        Effect fogOfWarEffect,
        Effect fogOfWarInverseEffect,
        Texture2D pixel)
    {
        _viewportManager = viewportManager;
        _tileset = tileset;
        _entitySpriteSheet = entitySpriteSheet;
        _fogOfWarEffect = fogOfWarEffect;
        _fogOfWarInverseEffect = fogOfWarInverseEffect;
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
                var chunk = GlobalState.TerrainManager.GetChunk(tileX: tileX, tileY: tileY);
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

        var activePlayer = GlobalState.PlayerManager.ActivePlayer;
        var playerIsInsideBuilding = GlobalState.TerrainManager.GetTile(tileX: activePlayer.TileX, tileY: activePlayer.TileY).Buildings.Any(BuildingData.BuildingIsEnclosed);
        var spriteAnimationLookupByTile = GlobalState.AnimationManager.GetAnimationsInRange(xTileStart: xTileStart, xTileEnd: xTileEnd, yTileStart: yTileStart, yTileEnd: yTileEnd)
            .ToLookup(mob => mob.TileY);

        // can set once globally?
        var halfScreenWidth = spriteBatch.GraphicsDevice.Viewport.Width / 2f;
        var halfScreenHeight = spriteBatch.GraphicsDevice.Viewport.Height / 2f;
        var fogOfWarRadius = Player.Player.SightRadius * TileSize * _viewportManager.Zoom;
        var fogOfWarRadiusWithBuffer = fogOfWarRadius + 30 * _viewportManager.Zoom;
        var fogOfWarRadiusWithBufferPow2 = fogOfWarRadiusWithBuffer * fogOfWarRadiusWithBuffer;
        var fogOfWarStartClipRadiusWithBuffer = fogOfWarRadius - 15 * _viewportManager.Zoom;
        var fogOfWarStartClipRadiusWithBufferPow2 = fogOfWarStartClipRadiusWithBuffer * fogOfWarStartClipRadiusWithBuffer;
        _fogOfWarEffect.Parameters["HalfScreenWidth"].SetValue(halfScreenWidth);
        _fogOfWarEffect.Parameters["HalfScreenHeight"].SetValue(halfScreenHeight);
        _fogOfWarEffect.Parameters["FogOfWarRadiusPow2"].SetValue(fogOfWarRadiusWithBufferPow2);
        _fogOfWarEffect.Parameters["FogOfWarStartClipRadiusPow2"].SetValue(fogOfWarStartClipRadiusWithBufferPow2);
        _fogOfWarEffect.Parameters["FogOfWarRadiusPow2Diff"].SetValue(fogOfWarRadiusWithBufferPow2 - fogOfWarStartClipRadiusWithBufferPow2);
        var fogOfWarInverseRadiusPow2 = fogOfWarRadius * fogOfWarRadius;
        var fogOfWarInverseStartClipRadiusWithBuffer = fogOfWarRadius - 30 * _viewportManager.Zoom;
        var fogOfWarInverseStartClipRadiusWithBufferPow2 = fogOfWarInverseStartClipRadiusWithBuffer * fogOfWarInverseStartClipRadiusWithBuffer;
        _fogOfWarInverseEffect.Parameters["HalfScreenWidth"].SetValue(halfScreenWidth);
        _fogOfWarInverseEffect.Parameters["HalfScreenHeight"].SetValue(halfScreenHeight);
        _fogOfWarInverseEffect.Parameters["FogOfWarRadiusPow2"].SetValue(fogOfWarInverseRadiusPow2);
        _fogOfWarInverseEffect.Parameters["FogOfWarStartClipRadiusPow2"].SetValue(fogOfWarInverseStartClipRadiusWithBufferPow2);
        _fogOfWarInverseEffect.Parameters["FogOfWarRadiusPow2Diff"].SetValue(fogOfWarInverseRadiusPow2 - fogOfWarInverseStartClipRadiusWithBufferPow2);
        //specific because the player sight radius is larger than the number of tiles you can see at zoom == 1
        var renderFogOfWar = RenderFogOfWar && _viewportManager.Zoom < 1;

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
                var tile = GlobalState.TerrainManager.GetTile(tileX: tileX, tileY: tileY);
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
                    DrawTileTerrain(spriteBatch, tile, xDraw: xDraw, yDraw: yDraw, scale: _viewportManager.Zoom, activePlayer.TilePlacement, playerIsInsideBuilding);
                    xDraw += zoomedTileSize;
                }
            }
            spriteBatch.End();
            // render entities after the terrain has finished so we don't clip the sprite when rendering the next tile over
            if (shouldRenderEntities)
            {
                spriteBatch.Begin(blendState: BlendState.NonPremultiplied, effect: renderFogOfWar ? _fogOfWarEffect : null);
                xDraw = (int)xDrawOffset;
                var animations = spriteAnimationLookupByTile[tileY];
                var leftXPoint = xTileStart * TileSize;
                var animationsToRenderThisTile = animations.OrderBy(e => e.Y).ToArray();
                foreach (var animation in animationsToRenderThisTile)
                {
                    var drawXDiff = (leftXPoint - animation.XInt) * _viewportManager.Zoom;
                    var drawYDiff = (topYPoint - animation.YInt) * _viewportManager.Zoom;
                    DrawSpriteAnimation(spriteBatch, animation, xDraw: xDraw - drawXDiff, yDraw: yDraw - drawYDiff, zoomScale: _viewportManager.Zoom);
                }
                if (activePlayer.TilePlacement != null)
                {
                    if (renderFogOfWar)
                    {
                        spriteBatch.End();
                        spriteBatch.Begin();
                    }
                    for (var tileX = xTileStart; tileX < xTileEnd; ++tileX)
                    {
                        if (activePlayer.TilePlacement.TileInRange(tileX: tileX, tileY: tileY))
                        {
                            var tile = GlobalState.TerrainManager.GetTile(tileX: tileX, tileY: tileY);
                            DrawPartialBuilding(spriteBatch, tile, xDraw: xDraw, yDraw: yDraw, activePlayer.TilePlacement, playerIsInsideBuilding);
                        }
                        xDraw += zoomedTileSize;
                    }
                    if (renderFogOfWar)
                    {
                        spriteBatch.End();
                        spriteBatch.Begin(blendState: BlendState.NonPremultiplied, effect: renderFogOfWar ? _fogOfWarEffect : null);
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
        if (renderFogOfWar)
        {
            spriteBatch.Begin(effect: shouldRenderEntities ? _fogOfWarInverseEffect : null);
            var fogOfWarOverlayDestination = new Rectangle(x: 0, y: 0, width: spriteBatch.GraphicsDevice.Viewport.Width, height: spriteBatch.GraphicsDevice.Viewport.Height);
            spriteBatch.Draw(_pixel, fogOfWarOverlayDestination, color: FogOfWarColor);
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
        string tileAboveFloorBuildingKey;
        var tileAboveInRangeOfPlacement = tilePlacementHasFloor && tilePlacement.TileInRange(tileX: tile.X, tileY: tile.Y - 1);
        // prefer rendering tile placement preview over placed tiles
        if (tileAboveInRangeOfPlacement)
        {
            tileAboveFloorBuildingKey = tilePlacement.BuildingKey;
        }
        else
        {
            var tileAbove = GlobalState.TerrainManager.GetTile(tileX: tile.X, tileY: tile.Y - 1);
            tileAboveFloorBuildingKey = tileAbove.Buildings.FirstOrDefault(BuildingData.BuildingHasFloor);
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
            DrawSprite(
                spriteBatch,
                _tileset[GlobalState.AnimationManager.TilesetAnimations[tile.Terrain]],
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
                Animation buildingAnimation = null;
                var color = defaultColor;
                if (building.Floor != null && !thisTileInRangeOfPlacement)
                {
                    buildingAnimation = building.FloorAnimation;
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
                if (buildingAnimation != null)
                {
                    DrawSprite(
                        spriteBatch,
                        _tileset[buildingAnimation],
                        xDraw: xDraw,
                        yDraw: yDraw,
                        scale: scale,
                        color: color);
                }
            }
            if (thisTileFloor != null)
            {
                string tileBelowFloorBuildingKey;
                var tileBelowInRangeOfPlacement = tilePlacementHasFloor && tilePlacement.TileInRange(tileX: tile.X, tileY: tile.Y + 1);
                if (tileBelowInRangeOfPlacement)
                {
                    tileBelowFloorBuildingKey = tilePlacement.BuildingKey;
                }
                else
                {
                    var tileBelow = GlobalState.TerrainManager.GetTile(tileX: tile.X, tileY: tile.Y + 1);
                    tileBelowFloorBuildingKey = tileBelow.Buildings.FirstOrDefault(BuildingData.BuildingHasFloor);
                }
                var building = GlobalState.BuildingData.Buildings[thisTileFloor];
                if ((playerIsInsideBuilding || (building.HasTransparency && tileBelowFloorBuildingKey == null)) && tileAboveFloorBuildingKey == null)
                {
                    if (building.InteriorWallAnimation != null)
                    {
                        DrawSprite(
                            spriteBatch,
                            _tileset[building.InteriorWallAnimation],
                            xDraw: xDraw,
                            yDraw: yDraw - TileSizeFloat * scale,
                            scale: scale,
                            color: playerIsInsideBuilding
                                ? defaultColor
                                : IndoorWhilePlayerIsOutsideColor);
                    }
                }
                else if (!playerIsInsideBuilding && tileBelowFloorBuildingKey != null)
                {
                    // defer the roof tile for exterior wall until the exterior wall is drawn
                    var buildingBelow = GlobalState.BuildingData.Buildings[tileBelowFloorBuildingKey];
                    if (buildingBelow.RoofAnimation != null)
                    {
                        DrawSprite(
                            spriteBatch,
                            _tileset[buildingBelow.RoofAnimation],
                            xDraw: xDraw,
                            yDraw: yDraw - WallHeightFloat * scale,
                            scale: scale,
                            color: defaultColor);
                    }
                }
            }
        }
        // defer drawing exterior walls so that they are always rendered over any entities inside
        if (thisTileFloor == null && tileAboveFloorBuildingKey != null && !tileAboveInRangeOfPlacement)
        {
            var buildingAbove = GlobalState.BuildingData.Buildings[tileAboveFloorBuildingKey];
            if (buildingAbove.ExteriorWallAnimation != null)
            {
                DrawSprite(
                    spriteBatch,
                    _tileset[buildingAbove.ExteriorWallAnimation],
                    xDraw: xDraw,
                    yDraw: yDraw - TileSizeFloat * scale,
                    scale: scale,
                    color: playerIsInsideBuilding
                        ? ExteriorWallTransparency
                        : defaultColor);
            }
            if (!playerIsInsideBuilding)
            {
                if (buildingAbove.Roof != null)
                {
                    DrawSprite(
                        spriteBatch,
                        _tileset[buildingAbove.RoofAnimation],
                        xDraw: xDraw,
                        yDraw: yDraw - (WallPlusTileSizeFloat) * scale,
                        scale: scale,
                        color: defaultColor);
                }
            }
        }
    }

    private void DrawSpriteAnimation(SpriteBatch spriteBatch, Animation animation, float xDraw, float yDraw, float zoomScale)
    {
        var entitySpriteSheet = _entitySpriteSheet[animation];
        var heightOffset = animation is IEntityAnimation entityAnimation && entityAnimation.Entity is IHasHeight entityHasHeight ? -entityHasHeight.HeightOffGroundInt : 0;
        var zoomedEntityScale = zoomScale * animation.Scale;
        if (heightOffset != 0)
        {
            DrawSprite(
                spriteBatch,
                entitySpriteSheet,
                xDraw: xDraw,
                yDraw: yDraw,
                scale: new Vector2(x: zoomedEntityScale.X, zoomedEntityScale.Y / 2),
                color: ItemShadow);
        }
        DrawSprite(
            spriteBatch,
            entitySpriteSheet,
            xDraw: xDraw,
            yDraw: yDraw + heightOffset,
            scale: zoomedEntityScale,
            color: animation.Color);
    }

    private void DrawPartialBuilding(SpriteBatch spriteBatch, Tile tile, float xDraw, float yDraw, ITilePlacement tilePlacement, bool playerIsInsideBuilding)
    {
        var scale = _viewportManager.Zoom;
        var tileIsBuildable = tilePlacement.AllTilesBuildable
            || BuildingExtensions.YieldTilesets(tile)
                .All(key => GlobalState.ConsolidatedZoningData[key].IsBuildable(tilePlacement.Buildable));
        var color = tileIsBuildable
            ? PartialBuildingColor
            : PartialBuildingInvalidColor;
        var building = GlobalState.BuildingData.Buildings[tilePlacement.BuildingKey];
        if (playerIsInsideBuilding && building.InteriorWallAnimation != null)
        {
            var tileAbove = GlobalState.TerrainManager.GetTile(tileX: tile.X, tileY: tile.Y - 1);
            var tileAboveHasFloor = tileAbove.Buildings.Any(BuildingData.BuildingHasFloor);
            if (!tileAboveHasFloor && tilePlacement.TileTopOfRange(tileX: tile.X, tileY: tile.Y))
            {
                DrawSprite(
                    spriteBatch,
                    _tileset[building.InteriorWallAnimation],
                    xDraw: xDraw,
                    yDraw: yDraw - (TileSize) * scale,
                    scale: scale,
                    color);
            }
            if (building.FloorAnimation != null)
            {
                DrawSprite(
                    spriteBatch,
                    _tileset[building.FloorAnimation],
                    xDraw: xDraw,
                    yDraw: yDraw,
                    scale: scale,
                    color);
            }
            if (tileAboveHasFloor && building.ExteriorWallAnimation != null)
            {
                var tileBelow = GlobalState.TerrainManager.GetTile(tileX: tile.X, tileY: tile.Y + 1);
                var tileBelowHasFloor = tileBelow.Buildings.Any(BuildingData.BuildingHasFloor);
                if (!tileBelowHasFloor && tilePlacement.TileBottomOfRange(tileX: tile.X, tileY: tile.Y))
                {
                    DrawSprite(
                        spriteBatch,
                        _tileset[building.ExteriorWallAnimation],
                        xDraw: xDraw,
                        yDraw: yDraw,
                        scale: scale,
                        color: tileIsBuildable
                            ? PartialBuildingExteriorWallTransparencyColor
                            : PartialBuildingInvalidExteriorWallTransparencyColor);
                }
            }
        }
        else if (building.ExteriorWallAnimation != null)
        {
            if (building.HasTransparency)
            {
                if (building.InteriorWallAnimation != null)
                {
                    DrawSprite(
                        spriteBatch,
                        _tileset[building.InteriorWallAnimation],
                        xDraw: xDraw,
                        yDraw: yDraw - (TileSize) * scale,
                        scale: scale,
                        color);
                }
                if (building.FloorAnimation != null)
                {
                    DrawSprite(
                        spriteBatch,
                        _tileset[building.FloorAnimation],
                        xDraw: xDraw,
                        yDraw: yDraw,
                        scale: scale,
                        color);
                }
            }
            if (tilePlacement.TileBottomOfRange(tileX: tile.X, tileY: tile.Y))
            {
                DrawSprite(
                    spriteBatch,
                    _tileset[building.ExteriorWallAnimation],
                    xDraw: xDraw,
                    yDraw: yDraw,
                    scale: scale,
                    color);
            }
            if (building.RoofAnimation != null)
            {
                DrawSprite(
                    spriteBatch,
                    _tileset[building.RoofAnimation],
                    xDraw: xDraw,
                    yDraw: yDraw - (WallHeightFloat) * scale,
                    scale: scale,
                    color);
            }
        }
        else if (building.FloorAnimation != null)
        {
            DrawSprite(
                spriteBatch,
                _tileset[building.FloorAnimation],
                xDraw: xDraw,
                yDraw: yDraw,
                scale: scale,
                color);
        }
    }

    private static void DrawSprite(
        SpriteBatch spriteBatch,
        ProcessedSpriteData entity,
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

    private static void DrawSprite(
        SpriteBatch spriteBatch,
        ProcessedSpriteData entity,
        float xDraw,
        float yDraw,
        Vector2 scale,
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