using System.Collections.Generic;

namespace FarmSim.Utils;

class TilesetData : ISpriteSheetData<TileData>
{
    public string BaseFolder { get; set; }
    public Dictionary<string, TileData> Data { get; set; }
}
