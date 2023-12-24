using System.Collections.Generic;

namespace FarmSim.Utils;

class EntitiesData : ISpriteSheetData<EntityData>
{
    public string BaseFolder { get; set; }
    public Dictionary<string, EntityData> Data { get; set; }
}
