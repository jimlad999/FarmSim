using System.Collections.Generic;

namespace FarmSim.Utils;
class TilesetData
{
    public string BaseFolder { get; set; }
    public Dictionary<string, SpriteData> Tilesets { get; set; }
}
