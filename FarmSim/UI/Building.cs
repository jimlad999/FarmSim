using FarmSim.Player;
using FarmSim.Terrain;
using FarmSim.Utils;

namespace FarmSim.UI;

class Building
{
    public BuildingType Type;
    public string Roof;
    public string ExteriorWall;
    public string InteriorWall;
    public string Floor;
    // SPEED HACK to determine rendering/not rendering the backwall
    public bool HasTransparency;
    // What this building requires to be built on
    public Zoning[] Buildable;
    public Cost Cost;
}