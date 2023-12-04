using System.Collections.Generic;

namespace FarmSim.Utils;
class EntitiesData
{
    public string BaseFolder { get; set; }
    public Dictionary<string, EntityData> Entities { get; set; }
}
