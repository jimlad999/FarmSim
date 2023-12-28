using System.Collections.Generic;

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
    public string DefaultAnimationKey { get; }
    public Dictionary<string, AnimationData> Animations { get; }
}

interface IBuildableData
{
    Zoning[] Buildable { get; }
}
