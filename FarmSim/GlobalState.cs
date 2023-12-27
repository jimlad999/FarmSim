using FarmSim.Entities;
using FarmSim.Terrain;
using FarmSim.UI;
using FarmSim.Utils;

namespace FarmSim;

static class GlobalState
{
    public static BuildingData BuildingData;
    public static Tileset Tileset;
    public static TerrainManager TerrainManager;
    public static ProjectileManager ProjectileManager;
    public static ItemManager ItemManager;
}
