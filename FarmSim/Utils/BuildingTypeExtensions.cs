using FarmSim.Terrain;
using System.Collections.Generic;
using System.Linq;
using static FarmSim.Utils.Tileset;

namespace FarmSim.Utils;

static class BuildingTypeExtensions
{
    public static IEnumerable<string> YieldTilesets(Tile tile)
    {
        yield return tile.Terrain;
        if (tile.Trees != null)
            yield return tile.Trees;
        if (tile.Ores != null)
            yield return tile.Ores;
    }

    public static bool IsBuildable(this ProcessedTileData tileData, ICollection<BuildingType> buildable)
    {
        return tileData.Buildable.Count > 0
            && tileData.Buildable.All(buildable.Contains);
    }
}
