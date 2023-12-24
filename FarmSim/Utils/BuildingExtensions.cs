using FarmSim.Terrain;
using System.Collections.Generic;
using System.Linq;

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

    public static bool IsBuildable(this ProcessedSpriteData spriteData, ICollection<Zoning> buildable)
    {
        return spriteData.Buildable.Count > 0
            && spriteData.Buildable.All(buildable.Contains);
    }
}
