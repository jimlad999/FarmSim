using System.Collections.Generic;

namespace FarmSim.UI;

class BuildingData
{
    public Dictionary<string, Building> Buildings;

    public static bool BuildingHasFloor(string buildingKey)
    {
        return GlobalState.BuildingData.Buildings[buildingKey].Floor != null;
    }

    public static bool BuildingIsEnclosed(string buildingKey)
    {
        var building = GlobalState.BuildingData.Buildings[buildingKey];
        return building.InteriorWall != null || building.ExteriorWall != null;
    }
}
