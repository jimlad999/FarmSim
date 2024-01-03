using FarmSim.Entities;
using FarmSim.Mobs;
using FarmSim.Player;
using FarmSim.Projectiles;
using FarmSim.Rendering;
using FarmSim.Terrain;
using FarmSim.UI;
using FarmSim.Utils;
using System.Collections.Generic;

namespace FarmSim;

static class GlobalState
{
    public static Dictionary<string, IBuildableData> ConsolidatedZoningData;
    public static BuildingData BuildingData;
    public static EntitiesData EntitiesData;
    public static Dictionary<string, ItemData> ItemData;
    public static Tileset Tileset;
    public static TerrainManager TerrainManager;
    public static PlayerManager PlayerManager;
    public static MobManager MobManager;
    public static ProjectileManager ProjectileManager;
    public static ItemManager ItemManager;
    public static AnimationManager AnimationManager;
    public static ViewportManager ViewportManager;
    public static EntitySpriteSheet EntitySpriteSheet;
}
