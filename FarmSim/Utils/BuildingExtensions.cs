using FarmSim.Terrain;
using System.Collections.Generic;
using System.Linq;
using static FarmSim.Utils.Tileset;

namespace FarmSim.Utils;

static class BuildingExtensions
{
    public static IEnumerable<string> YieldTilesets(Tile tile)
    {
        yield return tile.Terrain;
        if (tile.Trees != null)
            yield return tile.Trees.EntitySpriteKey;
        if (tile.Ores != null)
            yield return tile.Ores.EntitySpriteKey;
    }

    public static bool IsBuildable(this ProcessedTileData tileData, ICollection<Zoning> buildable)
    {
        return tileData.Buildable.Count > 0
            && tileData.Buildable.All(buildable.Contains);
    }
}
