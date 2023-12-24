using System.Collections.Generic;
using Utils.Data;

namespace FarmSim.Utils;

interface ISpriteSheetData<T>
    where T : ISpriteData
{
    string BaseFolder { get; }
    Dictionary<string, T> Data { get; }
}

interface ISpriteData
{
    string Source { get; }
    OriginData Origin { get; }
}

interface IBuildableData
{
    Zoning[] Buildable { get; }
}
